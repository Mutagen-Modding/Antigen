using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Antigen.Views;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class HomeVM : ResizablePanelVM
{
    private readonly IMainWindow _mainWindow;
    private readonly Subject<ModKey> _startRequested = new();
    private readonly ObservableAsPropertyHelper<IEnumerable<ModKey>> _filteredModKeys;

    public HomeVM(
        IMainWindow mainWindow,
        IFileSystem fileSystem,
        IDataDirectoryProvider dataDirectoryProvider,
        ILoadOrderListingsProvider loadOrderListingsProvider,
        ILogger<HomeVM> logger)
    {
        _mainWindow = mainWindow;
        IsExpanded = true;
        ExpandedHeight = 400.0;

        _filteredModKeys = this.WhenAnyValue(x => x.ModKeys, x => x.SearchText)
            .Select(t => Filter(t.Item1, t.Item2))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToProperty(this, nameof(FilteredModKeys));

        Task.Run(() => LoadModKeys(fileSystem, dataDirectoryProvider, loadOrderListingsProvider))
            .FireAndForget(ex => logger.LogError(ex, "Error loading mod keys"));
    }

    public override double MinResizeHeight => 100.0;

    public IObservable<ModKey> StartRequested => _startRequested;
    public IEnumerable<ModKey> FilteredModKeys => _filteredModKeys.Value;

    [Reactive] public partial ModKey[] ModKeys { get; set; } = [];
    [Reactive] public partial string SearchText { get; set; } = string.Empty;

    private static IEnumerable<ModKey> Filter(ModKey[] keys, string search) =>
        string.IsNullOrWhiteSpace(search)
            ? keys
            : keys.Where(m => m.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));

    private void LoadModKeys(
        IFileSystem fileSystem,
        IDataDirectoryProvider dataDirectoryProvider,
        ILoadOrderListingsProvider loadOrderListingsProvider)
    {
        ModKeys = loadOrderListingsProvider.Get()
            .Where(l => fileSystem.File.Exists(fileSystem.Path.Combine(dataDirectoryProvider.Path, l.FileName)))
            .Select(l => ModKey.FromFileName(l.FileName))
            .ToArray();
    }

    [ReactiveCommand]
    private void StartWatching(ModKey modKey)
    {
        if (modKey.IsNull) return;

        _startRequested.OnNext(modKey);
    }

    [ReactiveCommand]
    private void Close()
    {
        _mainWindow.Close();
    }
}
