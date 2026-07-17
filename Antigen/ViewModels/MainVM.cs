using System.Reactive.Linq;
using Antigen.Models.Settings;
using Antigen.Services;
using Antigen.ViewModels.Analyzer;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class MainVM : ViewModel
{
    private readonly Func<ModKey, ModWatcherVM> _modWatcherVMFactory;
    private readonly Func<ModWatcherVM, AnalyzerVM> _analyzerVMFactory;
    private readonly HomeVM _homeVM;
    private readonly GuiSettingsService _guiSettings;

    private ModWatcherVM? _currentWatcher;
    private IDisposable? _returnSubscription;
    private double _expandedHeight;

    public MainVM(
        HomeVM homeVM,
        GuiSettingsService guiSettings,
        Func<ModKey, ModWatcherVM> modWatcherVMFactory,
        Func<ModWatcherVM, AnalyzerVM> analyzerVMFactory)
    {
        _homeVM = homeVM;
        _guiSettings = guiSettings;
        _modWatcherVMFactory = modWatcherVMFactory;
        _analyzerVMFactory = analyzerVMFactory;

        SavedSettings = guiSettings.Load();
        if (SavedSettings is { } saved)
        {
            WindowX = saved.WindowX;
            WindowY = saved.WindowY;
        }
        _expandedHeight = SavedSettings?.ExpandedHeight ?? homeVM.ExpandedHeight;

        SetActivePanel(homeVM);

        homeVM.StartRequested
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(StartWatching)
            .DisposeWith(this);
    }

    public static Severity[] SeverityValues { get; } = Enum.GetValues<Severity>();

    [Reactive] public partial IResizablePanel? ActivePanel { get; set; }
    [Reactive] public partial int WindowX { get; set; }
    [Reactive] public partial int WindowY { get; set; }

    public GuiSettings? SavedSettings { get; }

    public void SaveGuiSettings()
    {
        var settings = (_guiSettings.Load() ?? new GuiSettings()) with
        {
            WindowX = WindowX,
            WindowY = WindowY,
            ExpandedHeight = ActivePanel?.ExpandedHeight ?? _expandedHeight
        };
        _guiSettings.Save(settings);
    }

    private void StartWatching(ModKey modKey)
    {
        _currentWatcher?.Dispose();
        _currentWatcher = _modWatcherVMFactory(modKey);

        var analyzer = _analyzerVMFactory(_currentWatcher);

        _returnSubscription?.Dispose();
        _returnSubscription = analyzer.ReturnRequested
            .Subscribe(_ => SetActivePanel(_homeVM));

        SetActivePanel(analyzer);
    }

    // Carry the resized height across panel switches so the window keeps its size.
    private void SetActivePanel(IResizablePanel panel)
    {
        if (ActivePanel is { } current)
        {
            _expandedHeight = current.ExpandedHeight;
        }

        panel.ExpandedHeight = Math.Clamp(_expandedHeight, panel.MinResizeHeight, panel.MaxResizeHeight);
        ActivePanel = panel;
    }
}
