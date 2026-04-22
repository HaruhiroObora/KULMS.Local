using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using static KULMS.Local.Services.GlobalSettings;

namespace KULMS.Local.ViewModels;

public partial class SettingsTabViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Domain { get; set; } = GlobalSetting.Settings.Domain;
    [ObservableProperty]
    public partial string ID { get; set; } = GlobalSetting.Settings.ID;
    [ObservableProperty]
    public partial string BrowserExecutablePath { get; set; } = GlobalSetting.Settings.BrowserExecutablePath;
    [ObservableProperty]
    public partial string LocalDirectoryPrefix { get; set; } = GlobalSetting.Settings.LocalDirectoryPrefix;
    [ObservableProperty]
    public partial bool SiteRefresh { get; set; } = GlobalSetting.Settings.SiteRefresh;



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
