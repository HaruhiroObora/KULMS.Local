using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using KULMS.Local.ViewModels;

using static KULMS.Local.Services.SyncService;

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