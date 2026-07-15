using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Antigen.Views;

public abstract class PinnedWindow : Window
{

    protected PinnedWindow()
    {
        // Enable dragging - use tunneling to check before buttons handle the event
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);

        Background = Brushes.Transparent;
        Topmost = true;
        WindowDecorations = WindowDecorations.None;
    }
    public abstract Control MainBar { get; set; }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Don't drag if clicking on interactive controls
        var source = e.Source as Control;
        if (source is Button || source.FindAncestorOfType<Button>() is not null) return;
        if (source is AutoCompleteBox || source.FindAncestorOfType<AutoCompleteBox>() is not null) return;

        // Check if we're clicking on the main bar or a child of it
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (source?.Name != MainBar.Name && !MainBar.IsVisualAncestorOf(source)) return;

        // Start window drag
        BeginMoveDrag(e);
        e.Handled = true;
    }
}