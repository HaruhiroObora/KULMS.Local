using Avalonia;
using Avalonia.Controls;
using KULMS.Local.Services;
using KULMS.Local.ViewModels;

using static KULMS.Local.Services.TopLevelService;

namespace KULMS.Local.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        TopLevelServiceProvider.SetTopLevel(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        KULMSApiService.KULMSApi.StopPeriodicRefresh();
    }
}