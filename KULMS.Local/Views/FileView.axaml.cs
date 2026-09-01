using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using KULMS.Local.ViewModels;

namespace KULMS.Local.Views;

public partial class FileView : UserControl
{
    private bool pointerPressed = false;
    private PointerPressedEventArgs? pressedEvent = null;

    public FileView()
    {
        InitializeComponent();
    }

    private void DoubleClicked(object? sender, TappedEventArgs e)
    {
        _ = ((FileViewModel?)DataContext)!.Open();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        pointerPressed = true;
        pressedEvent = e;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        pointerPressed = false;
    }

    private void OnPointerCaupureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        pointerPressed = false;
    }

    private async void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!pointerPressed)
        {
            return;
        }
        await ((FileViewModel?)DataContext)!.DoDragAsync(pressedEvent!);
    }
}