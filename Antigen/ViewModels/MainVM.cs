using System.Reactive.Linq;
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

    private ModWatcherVM? _currentWatcher;
    private IDisposable? _returnSubscription;

    public MainVM(
        HomeVM homeVM,
        Func<ModKey, ModWatcherVM> modWatcherVMFactory,
        Func<ModWatcherVM, AnalyzerVM> analyzerVMFactory)
    {
        _homeVM = homeVM;
        _modWatcherVMFactory = modWatcherVMFactory;
        _analyzerVMFactory = analyzerVMFactory;

        ActivePanel = homeVM;

        homeVM.StartRequested
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(StartWatching)
            .DisposeWith(this);
    }

    public static Severity[] SeverityValues { get; } = Enum.GetValues<Severity>();

    [Reactive] public partial IResizablePanel? ActivePanel { get; set; }

    private void StartWatching(ModKey modKey)
    {
        _currentWatcher?.Dispose();
        _currentWatcher = _modWatcherVMFactory(modKey);

        var analyzer = _analyzerVMFactory(_currentWatcher);

        _returnSubscription?.Dispose();
        _returnSubscription = analyzer.ReturnRequested
            .Subscribe(_ => ActivePanel = _homeVM);

        ActivePanel = analyzer;
    }
}
