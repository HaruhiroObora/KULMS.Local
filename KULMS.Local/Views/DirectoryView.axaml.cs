using Avalonia.Controls;
using Avalonia.Input;

using KULMS.Local.ViewModels;

namespace KULMS.Local.Views;

public partial class DirectoryView : UserControl
{
    public DirectoryView()
    {
        InitializeComponent();
    }

    private void DoubleClicked(object? sender, TappedEventArgs e)
    {
        DirectoryViewModel? viewModel = DataContext as DirectoryViewModel;
        viewModel?.contentsTab.CurrentDirectory = viewModel.DirectoryModel.Path;
        viewModel?.contentsTab.browsedDirectory = viewModel;
        _ = viewModel?.contentsTab.ChangeDirectory();
    }
}