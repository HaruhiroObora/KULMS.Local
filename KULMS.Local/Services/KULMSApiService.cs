using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using KULMS.Local.Infrastructures;
using KULMS.Local.Models;

using static KULMS.Local.Services.GlobalSettings;

namespace KULMS.Local.Services;

public class KULMSApiService
{
    public static KULMSApiService KULMSApi { get; } = new();

    public bool LoginStatus { get; set; } = false;

    private const int siteLimit = 50;

    private List<SiteModel> sites = [];

    private List<DirectoryModel> directories = [];
    private List<FileModel> files = [];

    private List<AssignmentModel> assignments = [];

    private string lastFilesId = string.Empty;

    private ApiHttpClient client;

    private CancellationTokenSource cts = new();

    private KULMSApiService()
    {
        client = new();
        StartPeriodicRefresh();
    }

    public async Task Login()
    {
        LoginStatus = await Task.Run(client.SetHttpClientSelenium);
    }

    public void StartPeriodicRefresh()
    {
        _ = PeriodicRefresh(cts.Token);
    }

    public void StopPeriodicRefresh()
    {
        cts.Cancel();
    }

    private async Task RefreshSites()
    {
        List<SiteModel> newSites = [];
        int counter;
        int offset = 0;
        do
        {
            counter = 0;

            IAsyncEnumerable<XElement> sitesXml;
            try
            {
                sitesXml = client.GetXmlAsync(GlobalSetting.Settings.SitesPath + $"?_limit={siteLimit}&_start={siteLimit * offset}", "site");
            }
            catch
            {
                LoginStatus = false;
                throw;
            }

            await foreach (var s in sitesXml)
            {
                counter++;
                newSites.Add
                (
                    new SiteModel
                    {
                        Title = s.Element("title")!.Value,
                        Id = s.Element("id")!.Value
                    }
                );
            }

            offset++;
        } while (counter == siteLimit);
        sites = newSites;
    }

    public async IAsyncEnumerable<SiteModel> GetSites(bool refresh = true)
    {
        if (refresh)
        {
            await RefreshSites();
        }
        foreach (var s in sites)
        {
            yield return s;
        }
    }

    private async Task RefreshFiles(SiteModel site)
    {
        List<DirectoryModel> newDirectories = [];
        List<FileModel> newFiles = [];

        Dictionary<string, string> urlPathToPath = [];

        urlPathToPath.Add("", "");

        IAsyncEnumerable<XElement> contentsXml;
        try
        {
            contentsXml = client.GetXmlAsync(string.Format(GlobalSetting.Settings.ContentsPath, site.Id), "content");
        }
        catch (HttpRequestException)
        {
            LoginStatus = false;
            throw;
        }
        catch
        {
            throw;
        }

        lastFilesId = site.Id;

        try
        {
            await foreach (var c in contentsXml)
            {
                if (c.Element("type")!.Value == "collection")
                {
                    var pathAsContainer = WebUtility.UrlDecode(c.Element("url")!.Value.Replace(GlobalSetting.Settings.Domain + GlobalSetting.Settings.FilePath + GlobalSetting.Settings.FileRootPath, "").ToString().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    if (urlPathToPath.TryGetValue(Path.GetDirectoryName(pathAsContainer)!.Replace("\\", "/"), out var parentPath))
                    {
                        urlPathToPath.Add(pathAsContainer, Path.Combine(parentPath, c.Element("title")!.Value));
                    }
                    else
                    {
                        throw new Exception("Invalid Context.");
                    }

                    newDirectories.Add
                    (
                        new DirectoryModel
                        {
                            Name = c.Element("title")!.Value,
                            Type = string.Empty,
                            UrlPath = c.Element("url")!.Value.Replace(GlobalSetting.Settings.Domain, "").ToString(),
                            Parent = urlPathToPath[RemoveStart(c.Element("container")!.Value, GlobalSetting.Settings.FileRootPath).ToString().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)],
                            DownloadStatus = Status.Folder,
                            LastModified = DateTime.ParseExact(c.Element("modifiedDate")!.Value, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture)
                        }
                    );
                }
                else if (c.Element("type")!.Value == "text/url")
                {
                    newFiles.Add
                    (
                        new URLModel
                        {
                            Name = Path.GetFileNameWithoutExtension(c.Element("title")!.Value),
                            Type = Path.GetExtension(c.Element("url")!.Value).AsSpan().TrimStart(".").ToString(),
                            UrlPath = c.Element("url")!.Value.Replace(GlobalSetting.Settings.Domain, "").ToString(),
                            Parent = urlPathToPath[RemoveStart(c.Element("container")!.Value, GlobalSetting.Settings.FileRootPath).ToString().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)],
                            LastModified = DateTime.ParseExact(c.Element("modifiedDate")!.Value, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture),
                            URL = c.Element("webLinkUrl")!.Value
                        }
                    );
                }
                else
                {
                    newFiles.Add
                    (
                        new FileModel
                        {
                            Name = Path.GetFileNameWithoutExtension(c.Element("title")!.Value),
                            Type = Path.GetExtension(c.Element("url")!.Value).AsSpan().TrimStart(".").ToString(),
                            UrlPath = c.Element("url")!.Value.Replace(GlobalSetting.Settings.Domain, "").ToString(),
                            Parent = urlPathToPath[RemoveStart(c.Element("container")!.Value, GlobalSetting.Settings.FileRootPath).ToString().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)],
                            LastModified = DateTime.ParseExact(c.Element("modifiedDate")!.Value, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture)
                        }
                    );
                }
            }
        }
        catch (HttpRequestException)
        {
            LoginStatus = false;
            throw;
        }
        catch
        {
            throw;
        }
        directories = newDirectories;
        files = newFiles;
    }

    private async Task RefreshAssignments()
    {
        List<AssignmentModel> newAssignments = [];

        IAsyncEnumerable<XElement> assignmentsXml;
        try
        {
            assignmentsXml = client.GetXmlAsync(GlobalSetting.Settings.AssignmentPath, "assignment");
        }
        catch (HttpRequestException)
        {
            LoginStatus = false;
            throw;
        }
        catch
        {
            throw;
        }

        try
        {
            await foreach (var a in assignmentsXml)
            {
                newAssignments.Add
                (
                    new AssignmentModel
                    {
                        Title = a.Element("title")!.Value,
                        Url = a.Element("entityURL")!.Value,
                        SiteId = a.Element("context")!.Value,
                        DueDate = DateTime.ParseExact(a.Element("closeTimeString")!.Value, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                        Status = AssignmentModel.AssignmentStatusFromString(a.Element("status")!.Value),
                        SubmissionStatus = AssignmentModel.SubmissionStatusFromString(a.Element("submissions")!.Element("simplesubmission")!.Element("status")!.Value)
                    }
                );
            }
        }
        catch (HttpRequestException)
        {
            LoginStatus = false;
            throw;
        }
        catch
        {
            throw;
        }
        assignments = newAssignments;
    }

    public async IAsyncEnumerable<AssignmentModel> GetAssignments(SiteModel? site = null, bool refresh = true)
    {
        if (refresh)
        {
            await RefreshAssignments();
        }
        if (site is null)
        {
            foreach (var a in assignments)
            {
                yield return a;
            }
        }
        else
        {
            foreach (var a in assignments)
            {
                if (a.SiteId == site.Id)
                {
                    yield return a;
                }
            }
        }
    }

    public async IAsyncEnumerable<FileModelBase> GetFiles(SiteModel site, bool refresh = true)
    {
        if (refresh || site.Id != lastFilesId)
        {
            if (!LoginStatus)
            {
                await Login();
            }
            await RefreshFiles(site);
        }

        foreach (var d in directories)
        {
            yield return d;
        }
        foreach (var f in files)
        {
            yield return f;
        }
    }

    public async Task<DirectoryModel?> GetDirectory(SiteModel site, bool refresh = true, string path = "")
    {
        if (refresh || site.Id != lastFilesId)
        {
            await RefreshFiles(site);
        }
        foreach (var d in directories)
        {
            if (d.Parent == path)
            {
                return d;
            }
        }
        return null;
    }

    public async Task Download(FileModel fileModel, FileStream stream)
    {
        await client.DownloadAsync(fileModel.UrlPath, stream);
    }

    private static string RemoveStart(string origin, string prefix)
    {
        if (origin.StartsWith(prefix))
        {
            return origin.Substring(prefix.Length);
        }
        else
        {
            return origin;
        }
    }

    private async Task PeriodicRefresh(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(ct))
        {
            if (!LoginStatus)
            {
                continue;
            }
            try
            {
                await RefreshSites();
            }
            catch
            {
            }
        }
    }
}
