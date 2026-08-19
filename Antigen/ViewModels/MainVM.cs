using System.Reactive.Linq;
using Antigen.Services;
using Antigen.Views;
using Avalonia.Controls;
using Mutagen.Bethesda.Environments.DI;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class MainVM : ViewModel, ISingleton
{
    private readonly GlobalSettingsVM _globalSettings;
    private readonly NavigationController _navigation;
    private readonly IMainWindow _mainWindow;

    private ResizablePanelVM? _sizedPanel;
    private double _expandedHeight;
    private double _expandedWidth;

    [Reactive] public partial int WindowX { get; set; }
    [Reactive] public partial int WindowY { get; set; }
    [Reactive] public partial bool AnchoredToBottom { get; set; }

    public string Version { get; }
    public string ProfileName { get; }
    public SessionVM Session { get; }

    public double ExpandedHeight => ActivePanel?.ExpandedHeight ?? _expandedHeight;
    public double ExpandedWidth => ActivePanel?.ExpandedWidth ?? _expandedWidth;

    [ObservableAsProperty(PropertyName = "ActivePanel")]
    private IObservable<ResizablePanelVM?> ActivePanelObservable() =>
        _navigation.WhenAnyValue(x => x.Active);

    [ObservableAsProperty(PropertyName = "IsExpanded", InitialValue = "true")]
    private IObservable<bool> IsExpandedObservable() =>
        _navigation.WhenAnyValue(x => x.Active)
            .Select(panel => panel?.WhenAnyValue(x => x.IsExpanded) ?? Observable.Return(false))
            .Switch();

    [ObservableAsProperty(PropertyName = "ShowPeek", InitialValue = "false")]
    private IObservable<bool> ShowPeekObservable() =>
        _navigation.WhenAnyValue(x => x.Active)
            .Select(panel => panel?.WhenAnyValue(x => x.IsExpanded, x => x.IsPeeking, (expanded, peeking) => !expanded && peeking)
                ?? Observable.Return(false))
            .Switch();

    [ObservableAsProperty(PropertyName = "ShowStatusBar", InitialValue = "false")]
    private IObservable<bool> ShowStatusBarObservable() =>
        Session.WhenAnyValue(x => x.CurrentWatcher).Select(watcher => watcher is not null);

    [ObservableAsProperty(PropertyName = "StatusBarDock", InitialValue = "global::Avalonia.Controls.Dock.Bottom")]
    private IObservable<Dock> StatusBarDockObservable() =>
        this.WhenAnyValue(x => x.ShowPeek, x => x.AnchoredToBottom,
            (peeking, bottom) => peeking && !bottom ? Dock.Top : Dock.Bottom);

    [ObservableAsProperty(PropertyName = "PeekArrowDown", InitialValue = "true")]
    private IObservable<bool> PeekArrowDownObservable() =>
        this.WhenAnyValue(x => x.ShowPeek, x => x.AnchoredToBottom, (peeking, bottom) => peeking == bottom);

    [ObservableAsProperty(PropertyName = "ShowStatusDivider", InitialValue = "false")]
    private IObservable<bool> ShowStatusDividerObservable() =>
        this.WhenAnyValue(x => x.IsExpanded, x => x.ShowPeek, x => x.ShowStatusBar,
            (expanded, peeking, status) => (expanded || peeking) && status);

    public MainVM(
        GuiSettingsService guiSettings,
        GlobalSettingsVM globalSettings,
        NavigationController navigation,
        SessionVM session,
        VersionProvider versionProvider,
        IMainWindow mainWindow,
        IGameReleaseContext gameReleaseContext)
    {
        _globalSettings = globalSettings;
        _navigation = navigation;
        _mainWindow = mainWindow;
        Session = session;

        Version = $"v{versionProvider.Current}";
        ProfileName = gameReleaseContext.Release.ToString();

        var saved = guiSettings.Current;
        WindowX = saved.WindowX ?? 0;
        WindowY = saved.WindowY ?? 0;
        _expandedHeight = saved.ExpandedHeight;
        _expandedWidth = saved.ExpandedWidth;

        InitializeOAPH();

        _navigation.WhenAnyValue(x => x.Active)
            .Subscribe(CarrySize)
            .DisposeWith(this);
    }

    [ReactiveCommand]
    private void OpenSettings()
    {
        _navigation.Push(_globalSettings);
    }

    // Profiles aren't implemented yet.
    [ReactiveCommand]
    private void OpenProfile()
    {
    }

    [ReactiveCommand]
    private void ToggleCollapsed()
    {
        if (ActivePanel is not { } panel) return;

        panel.IsExpanded = !panel.IsExpanded;
        panel.IsPeeking = false;
    }

    [ReactiveCommand]
    private void TogglePeek()
    {
        if (ActivePanel is not { } panel) return;

        panel.IsPeeking = !panel.IsPeeking;
    }

    [ReactiveCommand]
    private void Minimize()
    {
        _mainWindow.Minimize();
    }

    [ReactiveCommand]
    private void ToggleMaximize()
    {
        _mainWindow.ToggleMaximize();
    }

    [ReactiveCommand]
    private void Close()
    {
        _mainWindow.Close();
    }

    // Carry the resized height across panel switches so the window keeps its size.
    private void CarrySize(ResizablePanelVM? panel)
    {
        if (_sizedPanel is { } leaving)
        {
            _expandedHeight = leaving.ExpandedHeight;
            _expandedWidth = leaving.ExpandedWidth;
        }

        _sizedPanel = panel;
        if (panel is null) return;

        panel.ExpandedHeight = Math.Clamp(_expandedHeight, panel.MinResizeHeight, panel.MaxResizeHeight);
        panel.ExpandedWidth = Math.Clamp(_expandedWidth, panel.MinResizeWidth, panel.MaxResizeWidth);
    }
}
