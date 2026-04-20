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
    private string iD = GlobalSetting.Settings.ID;
    [ObservableProperty]
    private string browserExecutablePath = GlobalSetting.Settings.BrowserExecutablePath;
    [ObservableProperty]
    private string localDirectoryPrefix = GlobalSetting.Settings.LocalDirectoryPrefix;
    [ObservableProperty]
    private bool siteRefresh = GlobalSetting.Settings.SiteRefresh;



    [RelayCommand]
    public void SaveSettings()
    {
        GlobalSetting.Settings.Domain = Domain;
        GlobalSetting.Settings.ID = ID;
        GlobalSetting.Settings.BrowserExecutablePath = BrowserExecutablePath;
        GlobalSetting.Settings.LocalDirectoryPrefix = LocalDirectoryPrefix;
        GlobalSetting.Settings.SiteRefresh = SiteRefresh;

        GlobalSetting.SaveSettings();
    }
}
