using Avalonia.Controls;
using Avalonia.Input;
using KULMS.Local.Models;
using KULMS.Local.ViewModels;

namespace KULMS.Local.Views;

public partial class ContentsTab : UserControl
{
    public ContentsTab()
    {
        InitializeComponent();
    }

    private void SiteClicked(object? sender, TappedEventArgs e)
    {
        var viewModel = DataContext as ContentsTabViewModel;

        if (viewModel is not null)
        {
            _ = viewModel.SiteEnterPressed();
        }
    }
}