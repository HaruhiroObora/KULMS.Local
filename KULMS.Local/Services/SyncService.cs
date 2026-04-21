using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using KULMS.Local.Models;
using System.Diagnostics;

using static KULMS.Local.Services.GlobalSettings;

using static KULMS.Local.Services.KULMSApiService;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace KULMS.Local.Services;

public class SyncService
{
    public static SyncService Syncer { get; } = new();

    public async IAsyncEnumerable<FileModelBase> DirectoryFilter(IAsyncEnumerable<FileModelBase> files, string path)
    {
        await foreach (var f in files)
        {
            if (f.Parent == path)
            {
                yield return f;
            }
        }
    }

    private SyncService()
    {
    }

    public async Task CheckDownloaded(IAsyncEnumerable<FileModelBase> files)
    {
        await foreach (var f in files)
        {
            if (f is URLModel urlModel)
            {
                urlModel.DownloadStatus = Path.Exists(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, urlModel.NoExtentionPath) + ".html") ? Status.Offline : Status.Cloud;
            }
            else if (f is FileModel fileModel)
            {
                fileModel.DownloadStatus = Path.Exists(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, fileModel.Path)) ? Status.Offline : Status.Cloud;
            }
        }
    }

    public async Task<FileStream> GetDownloadStream(FileModel file)
    {
        if (!Path.Exists(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Parent)))
        {
            Directory.CreateDirectory(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Parent));
        }
        return new FileStream(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Path), FileMode.Create, FileAccess.Write);
    }

    public void DeleteStream(FileModel file)
    {
        File.Delete(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Path));
    }

    public void OpenFile(FileModel file)
    {
        try
        {
            ProcessStartInfo psInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file is URLModel urlModel ? urlModel.NoExtentionPath + ".html" : file.Path),
                UseShellExecute = true
            };
            Process.Start(psInfo);
        }
        catch
        {
            throw;
        }
    }

    public async Task Download(FileModel file)
    {
        if (file.DownloadStatus == Status.Downloading)
        {
            return;
        }
        if (file is URLModel uRL)
        {
            if (!Path.Exists(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, uRL.Parent)))
            {
                Directory.CreateDirectory(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, uRL.Parent));
            }
            var path = Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, uRL.NoExtentionPath) + ".html";
            try
            {
                CreateHtmlShortcut(path, uRL.URL);
                uRL.DownloadStatus = Status.Offline;
                return;
            }
            catch
            {
                uRL.DownloadStatus = Status.Failed;
                return;
            }
        }
        FileStream stream;
        try
        {
            stream = await GetDownloadStream(file);
        }
        catch
        {
            file.DownloadStatus = Status.Failed;
            return;
        }
        file.DownloadStatus = Status.Downloading;
        try
        {
            await KULMSApi.Download(file, stream);

            file.DownloadStatus = Status.Offline;
        }
        catch
        {
            file.DownloadStatus = Status.Failed;
            try
            {
                DeleteStream(file);
            }
            catch
            {
            }
            return;
        }
    }

    public async Task DownloadAll(SiteModel site, DirectoryModel directory, bool chain = false, bool refresh = false)
    {
        var files = KULMSApi.GetFiles(site, refresh);
        var filtered = Syncer.DirectoryFilter(files, directory.Path);
        await Syncer.CheckDownloaded(filtered);
        var fileList = await filtered.ToListAsync();
        foreach (var f in fileList)
        {
            if (f is FileModel file)
            {
                if (file.DownloadStatus != Status.Offline)
                {
                    await Download(file);
                }
            }
            else if (f is DirectoryModel directoryModel && chain)
            {
                await DownloadAll(site, directoryModel, true);
            }
        }
    }

    private static void CreateHtmlShortcut(string fullPath, string url)
    {
        var content = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""refresh"" content=""0;url={url}"">
    <title>Redirecting to {url}</title>
</head>
<body>
    <script>window.location.href=""{url}"";</script>
    <p>自動的に移動しない場合は、<a href=""{url}"">こちらをクリック</a>してください。</p>
</body>
</html>";

        File.WriteAllText(fullPath, content.Trim(), Encoding.UTF8);
    }

    public void OpeninFileApp(FileModelBase file)
    {
        if (file is DirectoryModel directory)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, directory.Path),
                UseShellExecute = true
            });
            return;
        }
        string path;
        if (file is URLModel uRL)
        {
            path = Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, uRL.NoExtentionPath) + ".html";
        }
        else
        {
            path = Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Path);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", $"/select,\"{path}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string appleScript = $"tell application \"Finder\" to reveal POSIX file \"{path}\"";
            Process.Start("osascript", $"-e '{appleScript}' -e 'tell application \"Finder\" to activate'");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var parentDir = Path.GetDirectoryName(path);
            Process.Start("xdg-open", $"\"{parentDir}\"");
        }
    }
}
