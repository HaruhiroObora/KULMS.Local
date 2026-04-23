using Avalonia.Controls;
using Avalonia.Input;
using KULMS.Local.ViewModels;

namespace KULMS.Local.Views;

public partial class FileView : UserControl
{
    public FileView()
    {
        InitializeComponent();
    }

    private void DoubleClicked(object? sender, TappedEventArgs e)
    {
        _ = ((FileViewModel?)DataContext)!.Open();
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