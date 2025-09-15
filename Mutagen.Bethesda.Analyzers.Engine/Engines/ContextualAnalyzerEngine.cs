using System.IO.Abstractions;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.Drivers;
using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Engines;

public interface IContextualAnalyzerEngine : IEngine
{
    Task Run(CancellationToken cancel);
}

public class ContextualAnalyzerEngine : IContextualAnalyzerEngine
{
    private readonly IFileSystem _fileSystem;
    private readonly IDataDirectoryProvider? _dataDirectoryProvider;
    private readonly ILoadOrderGetter<IModListingGetter<IModGetter>> _loadOrder;
    private readonly ILinkCache _linkCache;
    private readonly ICacheConstructor[] _cacheConstructors;
    private readonly IBlacklistedModsProvider _blacklistedModsProvider;
    private readonly IWorkDropoff _workDropoff;
    public IReportDropbox ReportDropbox { get; }
    public IDriverProvider<IContextualDriver> ContextualModDrivers { get; }
    public IDriverProvider<IIsolatedDriver> IsolatedModDrivers { get; }

    public IEnumerable<IDriver> Drivers => ContextualModDrivers.Drivers
        .Concat<IDriver>(IsolatedModDrivers.Drivers);

    public ContextualAnalyzerEngine(
        IFileSystem fileSystem,
        IDataDirectoryProvider? dataDirectoryProvider,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder,
        ILinkCache linkCache,
        IDriverProvider<IContextualDriver> contextualDrivers,
        IDriverProvider<IIsolatedDriver> isolatedDrivers,
        IReportDropbox reportDropbox,
        ICacheConstructor[] cacheConstructors,
        IBlacklistedModsProvider blacklistedModsProvider,
        IWorkDropoff workDropoff)
    {
        _fileSystem = fileSystem;
        _dataDirectoryProvider = dataDirectoryProvider;
        _loadOrder = loadOrder;
        _linkCache = linkCache;
        _cacheConstructors = cacheConstructors;
        _blacklistedModsProvider = blacklistedModsProvider;
        _workDropoff = workDropoff;
        ReportDropbox = reportDropbox;
        ContextualModDrivers = contextualDrivers;
        IsolatedModDrivers = isolatedDrivers;
    }

    public async Task Run(CancellationToken cancel)
    {
        if (cancel.IsCancellationRequested) return;
        var cacheCache = new ProvideCaches(_linkCache, _cacheConstructors);

        List<Task> toDo = new();

        var isolatedDrivers = IsolatedModDrivers.Drivers;
        if (isolatedDrivers.Count > 0)
        {
            foreach (var listing in _loadOrder.ListedOrder)
            {
                if (cancel.IsCancellationRequested) return;

                if (listing.Mod is null) continue;
                if (_blacklistedModsProvider.IsBlacklisted(listing.ModKey)) continue;

                ModPath? modPath = _dataDirectoryProvider == null
                    ? null
                    : new ModPath(Path.Combine(_dataDirectoryProvider.Path, listing.ModKey.FileName));

                var isolatedParam = new IsolatedDriverParams(
                    listing.Mod.ToUntypedImmutableLinkCache(),
                    ReportDropbox,
                    listing.Mod,
                    modPath == null
                        ? null
                        : new IsolatedDriverFileParams(
                            _fileSystem,
                            modPath),
                    cancel);

                toDo.Add(Task.WhenAll(IsolatedModDrivers.Drivers.Select(driver =>
                {
                    return _workDropoff.EnqueueAndWait(() =>
                    {
                        return driver.Drive(isolatedParam);
                    }, cancel);
                })));
            }
        }

        var contextualParam = new ContextualDriverParams(
            _linkCache,
            _loadOrder,
            ReportDropbox,
            cacheCache,
            cancel);

        toDo.Add(Task.WhenAll(ContextualModDrivers.Drivers.Select(driver =>
        {
            return _workDropoff.EnqueueAndWait(() =>
            {
                return driver.Drive(contextualParam);
            }, cancel);
        })));

        if (cancel.IsCancellationRequested) return;
        await Task.WhenAll(toDo);
    }
}
