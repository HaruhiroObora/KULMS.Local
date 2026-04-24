using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KULMS.Local.Services;

public class GlobalSettings
{
    public Settings Settings = new();
    public static GlobalSettings GlobalSetting { get; } = new();
    private static readonly JsonSerializerOptions options = new(){ WriteIndented = true };

    private GlobalSettings()
    {
        LoadSettings();
    }

    public void LoadSettings()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal", "settings.json");

        if (!Path.Exists(path))
        {
            return;
        }

        using (var jsonStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            Settings = JsonSerializer.Deserialize<Settings>(jsonStream) ?? Settings;
        }
    }

    public void SaveSettings()
    {
        var dirpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KULMSLocal");
        if (!Path.Exists(dirpath))
        {
            Directory.CreateDirectory(dirpath);
        }
        var path = Path.Combine(dirpath, "settings.json");

        using (var jsonStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            JsonSerializer.Serialize(jsonStream, Settings, options);
        }
    }
}

public class Settings
{
    public string Domain { get; set; } = "https://lms.gakusei.kyoto-u.ac.jp";
    public string SitesPath { get; set; } = "/direct/site.xml";
    public string ContentsPath { get; set; } = "/direct/content/site/{0}.xml";
    public string FilePath { get; set; } = "/access";
    public string FileRootPath { get; set; } = "/content/group/";
    public string ID { get; set; } = "";
    public string BrowserExecutablePath { get; set; } = GetChromePath() ?? "";
    public string LoginPath { get; set; } = "/portal/login";
    public string TopPagePath { get; set; } = "/portal";
    public string LocalDirectoryPrefix { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "授業資料");
    public bool SiteRefresh { get; set; } = true;
    public string AssignmentPath { get; set; } = "/direct/assignment/my.xml";
    public string AssignmentSitePath { get; set; } = "/direct/assignment/site/{0}.xml";

    public static string? GetChromePath()
    {
        if (OperatingSystem.IsWindows())
        {
            string[] paths = [
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
        ];
            return paths.FirstOrDefault(File.Exists);
        }

        if (OperatingSystem.IsMacOS())
        {
            string path = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
            return File.Exists(path) ? path : null;
        }

        if (OperatingSystem.IsLinux())
        {
            string[] paths = [
            "/usr/bin/google-chrome",
            "/usr/bin/chrome"
        ];
            return paths.FirstOrDefault(File.Exists);
        }

        return null;
    }
}