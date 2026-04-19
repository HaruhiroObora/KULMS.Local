using Avalonia;
using Avalonia.Controls;
using KULMS.Local.ViewModels;

namespace KULMS.Local.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
    }
}