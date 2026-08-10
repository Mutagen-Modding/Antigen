using System.Reactive.Disposables;
using System.Reactive.Linq;
using Antigen.Models.Settings;
using Antigen.Services;
using Antigen.Views;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class MainVM : ViewModel, ISingleton
{
    private readonly Func<ModKey, ModWatcherVM> _modWatcherVMFactory;
    private readonly Func<ModWatcherVM, AnalyzerVM> _analyzerVMFactory;
    private readonly GuiSettingsService _guiSettings;
    private readonly GlobalSettingsVM _globalSettings;
    private readonly ActiveVmController _activeVm;
    private readonly IMainWindow _mainWindow;
    private readonly ILogger<MainVM> _logger;

    private ResizablePanelVM? _sizedPanel;
    private double _expandedHeight;

    public static Severity[] SeverityValues { get; } = Enum.GetValues<Severity>();

    [Reactive] public partial ModWatcherVM? CurrentWatcher { get; set; }
    [Reactive] public partial AnalyzerVM? CurrentAnalyzer { get; set; }
    [Reactive] public partial int WindowX { get; set; }
    [Reactive] public partial int WindowY { get; set; }
    [Reactive] public partial bool AnchoredToBottom { get; set; }

    public string Version { get; }
    public string ProfileName { get; }
    public GuiSettings? SavedSettings { get; }

    [ObservableAsProperty(PropertyName = "ActivePanel")]
    private IObservable<ResizablePanelVM?> ActivePanelObservable() =>
        _activeVm.WhenAnyValue(x => x.Active);

    [ObservableAsProperty(PropertyName = "IsExpanded", InitialValue = "true")]
    private IObservable<bool> IsExpandedObservable() =>
        _activeVm.WhenAnyValue(x => x.Active)
            .Select(panel => panel?.WhenAnyValue(x => x.IsExpanded) ?? Observable.Return(false))
            .Switch();

    [ObservableAsProperty(PropertyName = "ShowPeek", InitialValue = "false")]
    private IObservable<bool> ShowPeekObservable() =>
        _activeVm.WhenAnyValue(x => x.Active)
            .Select(panel => panel?.WhenAnyValue(x => x.IsExpanded, x => x.IsPeeking, (expanded, peeking) => !expanded && peeking)
                ?? Observable.Return(false))
            .Switch();

    [ObservableAsProperty(PropertyName = "ShowStatusBar", InitialValue = "false")]
    private IObservable<bool> ShowStatusBarObservable() =>
        this.WhenAnyValue(x => x.CurrentWatcher).Select(watcher => watcher is not null);

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
        HomeVM homeVM,
        GuiSettingsService guiSettings,
        GlobalSettingsVM globalSettings,
        ActiveVmController activeVm,
        VersionProvider versionProvider,
        IMainWindow mainWindow,
        IGameReleaseContext gameReleaseContext,
        Func<ModKey, ModWatcherVM> modWatcherVMFactory,
        Func<ModWatcherVM, AnalyzerVM> analyzerVMFactory,
        ILogger<MainVM> logger)
    {
        _guiSettings = guiSettings;
        _globalSettings = globalSettings;
        _activeVm = activeVm;
        _mainWindow = mainWindow;
        _logger = logger;
        _modWatcherVMFactory = modWatcherVMFactory;
        _analyzerVMFactory = analyzerVMFactory;

        Version = $"v{versionProvider.Current}";
        ProfileName = gameReleaseContext.Release.ToString();

        SavedSettings = guiSettings.Load();
        if (SavedSettings is { } saved)
        {
            WindowX = saved.WindowX;
            WindowY = saved.WindowY;
        }
        _expandedHeight = SavedSettings?.ExpandedHeight ?? homeVM.ExpandedHeight;

        InitializeOAPH();

        _activeVm.WhenAnyValue(x => x.Active)
            .Subscribe(CarryHeight)
            .DisposeWith(this);

        _activeVm.Active = homeVM;

        homeVM.StartRequested
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(StartWatching)
            .DisposeWith(this);
    }

    public void Exit()
    {
        _logger.LogInformation("Exiting");

        var settings = (_guiSettings.Load() ?? new GuiSettings()) with
        {
            WindowX = WindowX,
            WindowY = WindowY,
            ExpandedHeight = ActivePanel?.ExpandedHeight ?? _expandedHeight,
            WorkerThreadPercentage = _globalSettings.CorePercentage,
            ColorScheme = _globalSettings.ColorScheme
        };
        _guiSettings.Save(settings);
    }

    [ReactiveCommand]
    private void OpenSettings()
    {
        _globalSettings.ReturnTo = _activeVm.Active;
        _activeVm.Active = _globalSettings;
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

    private void StartWatching(ModKey modKey)
    {
        CurrentWatcher?.Dispose();
        CurrentWatcher = _modWatcherVMFactory(modKey);
        CurrentAnalyzer = _analyzerVMFactory(CurrentWatcher);

        _activeVm.Active = CurrentAnalyzer;
    }

    // Carry the resized height across panel switches so the window keeps its size.
    private void CarryHeight(ResizablePanelVM? panel)
    {
        if (_sizedPanel is { } leaving)
        {
            _expandedHeight = leaving.ExpandedHeight;
        }

        _sizedPanel = panel;
        if (panel is null) return;

        panel.ExpandedHeight = Math.Clamp(_expandedHeight, panel.MinResizeHeight, panel.MaxResizeHeight);
    }
}
