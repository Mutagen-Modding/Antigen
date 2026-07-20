using Antigen.ViewModels;
using Antigen.ViewModels.Singleton;
using Avalonia.Controls;
using Avalonia.Input;

namespace Antigen.Views;

public partial class MainWindow : PinnedWindow, IMainWindow
{
    private bool _isResizing;
    private double _dragStartY;
    private double _originalHeight;

    private IResizablePanel? Panel => (DataContext as MainVM)?.ActivePanel;

    public MainWindow()
    {
        InitializeComponent();
        PositionChanged += OnPositionChanged;
    }

    public void Minimize() => WindowState = WindowState.Minimized;

    public void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (DataContext is not MainVM vm) return;

        vm.WindowX = e.Point.X;
        vm.WindowY = e.Point.Y;
    }

    private void ResizeGrip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Panel is null) return;

        _isResizing = true;
        _dragStartY = e.GetPosition(null).Y;
        _originalHeight = Panel.CurrentWindowHeight;
        e.Pointer.Capture(sender as IInputElement);
        e.Handled = true;
    }

    private void ResizeGrip_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizing || Panel is null) return;

        Panel.Resize(_originalHeight + (e.GetPosition(null).Y - _dragStartY));
    }

    private void ResizeGrip_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isResizing = false;
        e.Pointer.Capture(null);
    }
}
