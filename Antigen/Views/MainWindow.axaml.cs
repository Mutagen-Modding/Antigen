using Antigen.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Antigen.Views;

public partial class MainWindow : PinnedWindow, IMainWindow
{
    private const int DragThreshold = 4;

    private readonly record struct Anchor(PixelPoint Corner, bool Right, bool Bottom);

    private bool _isResizing;
    private bool _resizesWidth;
    private bool _resizesHeight;
    private bool _resizeRequested;
    private Point _dragStart;
    private double _originalWidth;
    private double _originalHeight;
    private Anchor? _anchor;
    private PixelPoint? _placed;

    private bool _dragArmed;
    private bool _isDragging;
    private PixelPoint _dragOrigin;
    private PixelVector _dragOffset;
    private bool _restorePeeking;

    private ResizablePanelVM? Panel => (DataContext as MainVM)?.ActivePanel;

    public MainWindow()
    {
        InitializeComponent();
        PositionChanged += OnPositionChanged;
        SizeChanged += OnSizeChanged;
    }

    public void Minimize() => WindowState = WindowState.Minimized;

    public void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        CaptureAnchor(CurrentSize);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WidthProperty || change.Property == HeightProperty)
        {
            _resizeRequested = true;
        }
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (DataContext is not MainVM vm) return;

        vm.WindowX = e.Point.X;
        vm.WindowY = e.Point.Y;

        if (_placed == e.Point)
        {
            _placed = null;
            return;
        }

        if (_resizeRequested) return;

        CaptureAnchor(CurrentSize);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var requested = _resizeRequested;
        _resizeRequested = false;

        if (!requested
            || _isResizing
            || _isDragging
            || WindowState != WindowState.Normal
            || _anchor is not { } anchor
            || MonitorBounds is not { } area)
        {
            CaptureAnchor(e.NewSize);
            return;
        }

        var width = ToPixels(e.NewSize.Width);
        var height = ToPixels(e.NewSize.Height);

        var placed = new PixelPoint(
            OnScreen(anchor.Right ? anchor.Corner.X - width : anchor.Corner.X, width, area.X, area.Width),
            OnScreen(anchor.Bottom ? anchor.Corner.Y - height : anchor.Corner.Y, height, area.Y, area.Height));
        if (placed == Position) return;

        _placed = placed;
        Position = placed;
    }

    protected override void BeginDrag(PointerPressedEventArgs e)
    {
        if (Panel is not { IsExpanded: false })
        {
            base.BeginDrag(e);
            return;
        }

        _dragArmed = true;
        _dragOrigin = ScreenPoint(e);
        _dragOffset = _dragOrigin - Position;
        e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_dragArmed && !_isDragging) return;

        var point = ScreenPoint(e);
        if (!_isDragging)
        {
            var moved = point - _dragOrigin;
            if (Math.Abs(moved.X) < DragThreshold && Math.Abs(moved.Y) < DragThreshold) return;

            StartDrag();
        }

        Position = point - _dragOffset;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        EndDrag();
        e.Pointer.Capture(null);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndDrag();
    }

    private void StartDrag()
    {
        _isDragging = true;

        if (Panel is not { } panel) return;

        _restorePeeking = panel.IsPeeking;
        panel.IsPeeking = false;

        var height = ToPixels(panel.CurrentWindowHeight);
        if (_dragOffset.Y > height)
        {
            _dragOffset = new PixelVector(_dragOffset.X, height / 2);
        }
    }

    private void EndDrag()
    {
        _dragArmed = false;
        if (!_isDragging) return;

        _isDragging = false;
        CaptureAnchor(CurrentSize);

        if (Panel is not { } panel) return;

        panel.IsPeeking = _restorePeeking;
    }

    private PixelPoint ScreenPoint(PointerEventArgs e) => this.PointToScreen(e.GetPosition(this));

    private void CaptureAnchor(Size size)
    {
        if (NearestCorner(Position, size) is not { } corner) return;

        _anchor = new Anchor(
            new PixelPoint(
                corner.Right ? Position.X + ToPixels(size.Width) : Position.X,
                corner.Bottom ? Position.Y + ToPixels(size.Height) : Position.Y),
            corner.Right,
            corner.Bottom);

        if (DataContext is MainVM vm)
        {
            vm.AnchoredToBottom = corner.Bottom;
        }
    }

    private (bool Right, bool Bottom)? NearestCorner(PixelPoint position, Size size)
    {
        if (MonitorBounds is not { } area) return null;

        var centerX = position.X + ToPixels(size.Width) / 2.0;
        var centerY = position.Y + ToPixels(size.Height) / 2.0;

        return (centerX > area.X + area.Width / 2.0, centerY > area.Y + area.Height / 2.0);
    }

    private static int OnScreen(int start, int length, int areaStart, int areaLength) =>
        length > areaLength
            ? start
            : Math.Clamp(start, areaStart, areaStart + areaLength - length);
            
    private PixelRect? MonitorBounds => (Screens.ScreenFromWindow(this) ?? Screens.Primary)?.Bounds;

    private Size CurrentSize => Bounds.Size is { Width: > 0, Height: > 0 } bounds ? bounds : new Size(Width, Height);

    private int ToPixels(double dips) => (int)Math.Round(dips * RenderScaling);

    private void ResizeGrip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Panel is not { } panel) return;

        _isResizing = true;
        _resizesWidth = !ReferenceEquals(sender, ResizeGripBottom);
        _resizesHeight = !ReferenceEquals(sender, ResizeGripRight);
        _dragStart = e.GetPosition(null);
        _originalWidth = panel.CurrentWindowWidth;
        _originalHeight = panel.CurrentWindowHeight;
        e.Pointer.Capture(sender as IInputElement);
        e.Handled = true;
    }

    private void ResizeGrip_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizing || Panel is not { } panel) return;

        var moved = e.GetPosition(null) - _dragStart;
        panel.Resize(
            _resizesWidth ? _originalWidth + moved.X : _originalWidth,
            _resizesHeight ? _originalHeight + moved.Y : _originalHeight);
    }

    private void ResizeGrip_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isResizing = false;
        e.Pointer.Capture(null);
    }
}
