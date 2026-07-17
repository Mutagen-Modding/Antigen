using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Antigen.Views;

public abstract class PinnedWindow : Window
{
    protected PinnedWindow()
    {
        // Bubbling: interactive controls (and the resize grip) handle the press
        // first and opt out of the drag; empty areas fall through to here.
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);

        Background = Brushes.Transparent;
        Topmost = true;
        WindowDecorations = WindowDecorations.None;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        BeginMoveDrag(e);
    }
}
