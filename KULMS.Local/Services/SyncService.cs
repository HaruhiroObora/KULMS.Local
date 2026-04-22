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

public interface ISyncService
{
    public IAsyncEnumerable<FileModelBase> DirectoryFilter(IAsyncEnumerable<FileModelBase> files, string path);
    public Task CheckDownloaded(IAsyncEnumerable<FileModelBase> files);
    public void OpenFile(FileModel file);
    public Task Download(FileModel file, string? customPath = null, bool statusChange = true);
    public Task DownloadAll(SiteModel site, DirectoryModel directory, bool chain = false, bool refresh = false);
    public void OpeninFileApp(FileModelBase file);
}

public class SyncService : ISyncService
{
    public static ISyncService Syncer { get; } = new SyncService();

    private SyncService()
    {
    }

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

    private FileStream GetDownloadStream(FileModel file)
    {
        if (!Path.Exists(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Parent)))
        {
            Directory.CreateDirectory(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Parent));
        }
        return new FileStream(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Path), FileMode.Create, FileAccess.Write);
    }

    private FileStream GetDownloadStream(string path)
    {
        if (!Path.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        }
        return new FileStream(Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, path), FileMode.Create, FileAccess.Write);
    }

    private void DeleteStream(FileModel file)
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

    public async Task Download(FileModel file, string? customPath = null, bool statusChange = true)
    {
        if (file.DownloadStatus == Status.Downloading)
        {
            return;
        }
        customPath ??= Path.Combine(GlobalSetting.Settings.LocalDirectoryPrefix, file.Path);

        if (file is URLModel uRL)
        {
            if (!Path.Exists(Path.GetDirectoryName(customPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(customPath)!);
            }
            var path = Path.Combine(Path.GetDirectoryName(customPath)!, Path.GetFileNameWithoutExtension(customPath)) + ".html";
            try
            {
                CreateHtmlShortcut(path, uRL.URL);
                if (statusChange)
                {
                    uRL.DownloadStatus = Status.Offline;
                }
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
            stream = GetDownloadStream(customPath);
        }
        catch
        {
            file.DownloadStatus = Status.Failed;
            return;
        }
        var cachedStatus = file.DownloadStatus;
        file.DownloadStatus = Status.Downloading;
        try
        {
            await KULMSApi.Download(file, stream);

            if (statusChange)
            {
                file.DownloadStatus = Status.Offline;
            }
            else
            {
                file.DownloadStatus = cachedStatus;
            }
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
