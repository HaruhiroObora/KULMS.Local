using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
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
            if (viewModel.SelectedSite is SiteModel site)
            {
                viewModel.CurrentDirectory = site.Title;
                viewModel.browsedSite = site;
                viewModel.SelectedFile = null;
                viewModel.browsedDirectory = null;
                _ = viewModel.ChangeDirectory();
            }
        }
    }

    private async void SiteEnterPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var viewModel = DataContext as ContentsTabViewModel;

            if (viewModel is not null)
            {
                if (viewModel.SelectedSite is SiteModel site)
                {
                    viewModel.CurrentDirectory = site.Title;
                    viewModel.browsedSite = site;
                    viewModel.SelectedFile = null;
                    _ = viewModel.ChangeDirectory();
                }
            }
            Dispatcher.UIThread.Post(() => ((ListBox?)sender)?.Focus(), DispatcherPriority.Render);
        }
    }

    private async void ListEnterPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await ((ContentsTabViewModel?)DataContext)!.ListEnterPressed();
            Dispatcher.UIThread.Post(() => ((ListBox?)sender)?.Focus(), DispatcherPriority.Render);
        }
    }
}