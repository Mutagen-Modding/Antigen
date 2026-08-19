using System.Reactive.Linq;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

/// <summary>
/// Owns what is currently being watched, and the analysis it feeds.
/// Probably will be refactored away once we're watching more than one mod at a time
///</summary>
public sealed partial class SessionVM : ViewModel, ISingleton
{
    private readonly NavigationController _navigation;
    private readonly Func<ModKey, ModWatcherVM> _modWatcherVMFactory;
    private readonly Func<ModWatcherVM, AnalyzerVM> _analyzerVMFactory;

    [Reactive] public partial ModWatcherVM? CurrentWatcher { get; private set; }
    [Reactive] public partial AnalyzerVM? CurrentAnalyzer { get; private set; }

    public SessionVM(
        HomeVM homeVM,
        NavigationController navigation,
        Func<ModKey, ModWatcherVM> modWatcherVMFactory,
        Func<ModWatcherVM, AnalyzerVM> analyzerVMFactory)
    {
        _navigation = navigation;
        _modWatcherVMFactory = modWatcherVMFactory;
        _analyzerVMFactory = analyzerVMFactory;

        homeVM.StartRequested
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(StartWatching)
            .DisposeWith(this);
    }

    private void StartWatching(ModKey modKey)
    {
        CurrentWatcher?.Dispose();
        CurrentWatcher = _modWatcherVMFactory(modKey);
        CurrentAnalyzer = _analyzerVMFactory(CurrentWatcher);

        _navigation.GoTo(CurrentAnalyzer);
    }
}
