namespace KULMS.Local.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ContentsTabViewModel ContentsViewModel { get; } = new();
    public SettingsTabViewModel SettingsViewModel { get; } = new();
}
