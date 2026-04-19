using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using static KULMS.Local.Services.GlobalSettings;

namespace KULMS.Local.ViewModels;

public partial class SettingsTabViewModel : ViewModelBase
{
    [ObservableProperty]
    private string domain = GlobalSetting.Settings.Domain;
    [ObservableProperty]
    private string sitesPath = GlobalSetting.Settings.SitesPath;
    [ObservableProperty]
    private string contentsPath = GlobalSetting.Settings.ContentsPath;
    [ObservableProperty]
    private string filePath = GlobalSetting.Settings.FilePath;
    [ObservableProperty]
    private string fileRootPath = GlobalSetting.Settings.FileRootPath;
    [ObservableProperty]
    private string iD = GlobalSetting.Settings.ID;
    [ObservableProperty]
    private string browserExecutablePath = GlobalSetting.Settings.BrowserExecutablePath;
    [ObservableProperty]
    private string loginPath = GlobalSetting.Settings.LoginPath;
    [ObservableProperty]
    private string topPagePath = GlobalSetting.Settings.TopPagePath;
    [ObservableProperty]
    private string localDirectoryPrefix = GlobalSetting.Settings.LocalDirectoryPrefix;

    [RelayCommand]
    public void SaveSettings()
    {
        GlobalSetting.Settings.Domain = Domain;
        GlobalSetting.Settings.SitesPath = SitesPath;
        GlobalSetting.Settings.ContentsPath = ContentsPath;
        GlobalSetting.Settings.FilePath = FilePath;
        GlobalSetting.Settings.FileRootPath = FileRootPath;
        GlobalSetting.Settings.ID = ID;
        GlobalSetting.Settings.BrowserExecutablePath = BrowserExecutablePath;
        GlobalSetting.Settings.LoginPath = LoginPath;
        GlobalSetting.Settings.TopPagePath = TopPagePath;
        GlobalSetting.Settings.LocalDirectoryPrefix = LocalDirectoryPrefix;

        GlobalSetting.SaveSettings();
    }
}
