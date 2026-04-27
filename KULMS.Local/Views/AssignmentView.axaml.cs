using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using KULMS.Local.ViewModels;

namespace KULMS.Local.Views;

public partial class AssignmentView : UserControl
{
    public AssignmentView()
    {
        InitializeComponent();
    }

    public void OpenPage(object? sender, TappedEventArgs e)
    {
        _ = ((AssignmentViewModel?)DataContext)!.OpenPage();
    }
}