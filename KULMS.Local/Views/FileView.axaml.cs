using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using KULMS.Local.ViewModels;

namespace KULMS.Local.Views;

public partial class FileView : UserControl
{
    private bool pointerPressed = false;
    private PointerPressedEventArgs? pressedEvent = null;
    private Point? _startPoint = null;
    private const int DragThreshold = 4;

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
        _startPoint = e.GetPosition(this);
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
        var currentPoint = e.GetPosition(this);
        var diff = (Point)(currentPoint - _startPoint)!;
        if (Math.Abs(diff.X) < DragThreshold && Math.Abs(diff.Y) < DragThreshold)
        {
            return;
        }
        pointerPressed = false;
        await ((FileViewModel?)DataContext)!.DoDragAsync(pressedEvent!);
    }
}