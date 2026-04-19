using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
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

    private readonly List<SiteModel> sites = [];

    private readonly List<DirectoryModel> directories = [];
    private readonly List<FileModel> files = [];

    private readonly Dictionary<string, string> urlPathToPath = [];
    private readonly Dictionary<string, string> pathToUrlPath = [];

    private string lastFilesId = string.Empty;

    private ApiHttpClient client;

    private KULMSApiService()
    {
        client = new();
    }

    public async Task Login()
    {
        LoginStatus = await Task.Run(client.SetHttpClientSelenium);
    }

    public async IAsyncEnumerable<SiteModel> GetSites(bool refresh = true)
    {
        if (refresh)
        {
            sites.Clear();
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
                    sites.Add
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
        }
        foreach (var s in sites)
        {
            yield return s;
        }
    }

    private async Task RefreshFiles(SiteModel site)
    {

        directories.Clear();
        files.Clear();

        urlPathToPath.Clear();
        pathToUrlPath.Clear();

        urlPathToPath.Add("", "");

        IAsyncEnumerable<XElement> contentsXml;
        try
        {
            contentsXml = client.GetXmlAsync(string.Format(GlobalSetting.Settings.ContentsPath, site.Id), "content");
        }
        catch
        {
            LoginStatus = false;
            throw;
        }

        lastFilesId = site.Id;

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

                directories.Add
                (
                    new DirectoryModel
                    {
                        Name = c.Element("title")!.Value,
                        Type = string.Empty,
                        UrlPath = c.Element("url")!.Value.Replace(GlobalSetting.Settings.Domain, "").ToString(),
                        Parent = urlPathToPath[RemoveStart(c.Element("container")!.Value, GlobalSetting.Settings.FileRootPath).ToString().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)],
                        DownloadStatus = Status.Folder,
                        LastModified = DateTime.ParseExact(c.Element("modifiedDate")!.Value, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture)
                        // DecodedPathAsContainer = WebUtility.UrlDecode(c.Element("url")!.Value.Replace(Domain + FilePath + FileRootPath, "").ToString().Replace(site.Id, site.Title).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                    }
                );
            }
            else if (c.Element("type")!.Value == "text/url")
            {
                files.Add
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
                files.Add
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

    public async IAsyncEnumerable<FileModelBase> GetFiles(SiteModel site, bool refresh = true)
    {
        if (refresh || site.Id != lastFilesId)
        {
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
}
