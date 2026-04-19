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
}