using Antigen.ViewModels.Analyzer;
using Avalonia.Controls;
using Avalonia.Input;

namespace Antigen.Views.Analyzer;

public partial class AnalyzerView : UserControl
{
    private double _dragStartY;

    private bool _isResizing;
    private double _originalHeight;

    public AnalyzerView()
    {
        InitializeComponent();
    }

    public AnalyzerVM? ViewModel => DataContext as AnalyzerVM;

    private void ResizeGrip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel is null) return;

        _isResizing = true;
        _dragStartY = e.GetPosition(null).Y;
        _originalHeight = ViewModel.ExpandedViewHeight;

        // Opt out of PinnedWindow's move-drag.
        e.Handled = true;
    }

    private void ResizeGrip_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizing || ViewModel is null) return;

        var currentY = e.GetPosition(null).Y;
        var delta = currentY - _dragStartY;
        var newHeight = _originalHeight + delta;

        // Enforce minimum and maximum heights
        const double minHeight = 200.0;
        const double maxHeight = 1000.0;

        newHeight = newHeight switch
        {
            < minHeight => minHeight,
            > maxHeight => maxHeight,
            _ => newHeight
        };

        ViewModel.ExpandedViewHeight = newHeight;
    }

    private void ResizeGrip_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isResizing = false;
    }
}
