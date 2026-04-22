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

        // DataContext経由でViewModelを取得し、停止メソッドを呼ぶ
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel is not null)
        {
            viewModel.ContentsViewModel.StopService();
        }
        KULMSApiService.KULMSApi.StopPeriodicRefresh();
    }
}