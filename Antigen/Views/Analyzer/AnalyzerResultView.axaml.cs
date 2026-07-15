using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace Antigen.Views.Analyzer;

public sealed partial class AnalyzerResultView : UserControl
{
    public AnalyzerResultView()
    {
        InitializeComponent();
    }

    private async void Copy_PointerPressed(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not ContentControl control) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is not { Clipboard: {} clipboard }) return;

        await clipboard.SetTextAsync(control.Content?.ToString());
    }
}