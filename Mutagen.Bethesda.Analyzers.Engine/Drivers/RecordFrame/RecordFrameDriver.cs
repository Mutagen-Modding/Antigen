using System.Buffers;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Meta;
using Noggog;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Drivers.RecordFrame;

public class RecordFrameDriver : IIsolatedDriver, IContextualDriver
{
    private readonly IWorkDropoff _dropoff;
    private readonly GameConstants _constants;
    private readonly IGameReleaseContext _gameReleaseContext;
    private readonly IDataDirectoryProvider _dataDataDirectoryProvider;
    private readonly IBlacklistedModsProvider _blacklistedModsProvider;

    private class Drivers
    {
        public IIsolatedRecordFrameAnalyzerDriver[] Isolated = [];
        public IContextualRecordFrameAnalyzerDriver[] Contextual = [];
    }

    private readonly Dictionary<RecordType, Drivers> _mapping = new();
    public bool Applicable => _mapping.Count > 0;

    public IEnumerable<IAnalyzer> Analyzers => _mapping.Values.SelectMany(x =>
    {
        return x.Contextual.SelectMany(x => x.Analyzers)
            .Concat(x.Isolated.SelectMany(x => x.Analyzers));
    });

    public RecordFrameDriver(
        IGameReleaseContext gameReleaseContext,
        IDataDirectoryProvider dataDataDirectoryProvider,
        IIsolatedRecordFrameAnalyzerDriver[] isolatedDrivers,
        IContextualRecordFrameAnalyzerDriver[] contextualDrivers,
        IBlacklistedModsProvider blacklistedModsProvider,
        IWorkDropoff dropoff)
    {
        _gameReleaseContext = gameReleaseContext;
        _dataDataDirectoryProvider = dataDataDirectoryProvider;
        _blacklistedModsProvider = blacklistedModsProvider;
        _dropoff = dropoff;
        _constants = GameConstants.Get(_gameReleaseContext.Release);
        foreach (var drivers in isolatedDrivers
                     .Where(x => x.Applicable)
                     .GroupBy(x => x.TargetType))
        {
            _mapping.GetOrAdd(drivers.Key).Isolated = drivers.ToArray();
        }
        foreach (var drivers in contextualDrivers
                     .Where(x => x.Applicable)
                     .GroupBy(x => x.TargetType))
        {
            _mapping.GetOrAdd(drivers.Key).Contextual = drivers.ToArray();
        }
    }

    public async Task Drive(ContextualDriverParams driverParams)
    {
        if (driverParams.CancellationToken.IsCancellationRequested) return;
        await Task.WhenAll(driverParams.LoadOrder.ListedOrder
            .Select(x => x.Mod)
            .WhereNotNull()
            .Select(async mod =>
            {
                if (driverParams.CancellationToken.IsCancellationRequested) return;
                if (_blacklistedModsProvider.IsBlacklisted(mod.ModKey)) return;

                var modPath = Path.Combine(_dataDataDirectoryProvider.Path, mod.ModKey.FileName);

                var parsingMeta = ParsingMeta.Factory(new BinaryReadParameters(), _gameReleaseContext.Release, modPath);
                await using var stream = new MutagenBinaryReadStream(modPath, parsingMeta);
                var locs = await _dropoff.EnqueueAndWait(() => RecordLocator.GetLocations(stream));

                var amount = locs.ListedRecords.Count;
                var tasks = ArrayPool<Task?>.Shared.Rent(locs.ListedRecords.Count);
                var bufs = ArrayPool<byte[]?>.Shared.Rent(locs.ListedRecords.Count);

                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        if (driverParams.CancellationToken.IsCancellationRequested) return;
                        var recordLocationMarker = locs.ListedRecords[i];
                        if (!_mapping.TryGetValue(recordLocationMarker.Record, out var analyzerBundles)) continue;
                        if (analyzerBundles.Isolated.Length == 0 && analyzerBundles.Contextual.Length == 0) continue;

                        stream.Position = recordLocationMarker.Location.Min;

                        var width = checked((int)recordLocationMarker.Location.Width);

                        var buf = ArrayPool<byte>.Shared.Rent(width);
                        bufs[i] = buf;

                        var amountRead = stream.Read(buf.AsSpan().Slice(0, width));
                        if (amountRead != width)
                        {
                            throw new DataMisalignedException();
                        }

                        var frame = new MajorRecordFrame(_constants, new ReadOnlyMemorySlice<byte>(buf).Slice(width));

                        List<Task> toDo = new();

                        if (analyzerBundles.Isolated.Length > 0)
                        {
                            var isolatedParams = new IsolatedDriverParams(
                                mod.ToUntypedImmutableLinkCache(),
                                driverParams.ReportDropbox,
                                mod,
                                modPath,
                                driverParams.CancellationToken);

                            toDo.Add(Task.WhenAll(analyzerBundles.Isolated.Select(analyzer =>
                            {
                                return _dropoff.EnqueueAndWait(() =>
                                {
                                    analyzer.Drive(isolatedParams, frame);
                                }, driverParams.CancellationToken);
                            })));
                        }

                        toDo.Add(Task.WhenAll(analyzerBundles.Contextual.Select(analyzer =>
                        {
                            return _dropoff.EnqueueAndWait(() =>
                            {
                                analyzer.Drive(driverParams, frame);
                            }, driverParams.CancellationToken);
                        })));

                        tasks[i] = Task.WhenAll(toDo);
                    }

                    for (int i = 0; i < amount; i++)
                    {
                        if (driverParams.CancellationToken.IsCancellationRequested) return;
                        var task = tasks[i];
                        if (task != null)
                        {
                            await task;
                        }
                    }
                }
                finally
                {
                    ArrayPool<Task?>.Shared.Return(tasks);

                    for (int i = 0; i < amount; i++)
                    {
                        var buf = bufs[i];
                        if (buf == null) continue;
                        ArrayPool<byte>.Shared.Return(buf);
                    }
                    ArrayPool<byte[]?>.Shared.Return(bufs);
                }
            }));
    }

    public async Task Drive(IsolatedDriverParams driverParams)
    {
        if (driverParams.CancellationToken.IsCancellationRequested) return;
        if (driverParams.TargetModPath == null) return;

        var parsingMeta = ParsingMeta.Factory(
            new BinaryReadParameters(),
            _gameReleaseContext.Release,
            driverParams.TargetModPath);
        using var stream = new MutagenBinaryReadStream(driverParams.TargetModPath, parsingMeta);
        var locs = RecordLocator.GetLocations(stream);

        var amount = locs.ListedRecords.Count;
        var tasks = ArrayPool<Task>.Shared.Rent(locs.ListedRecords.Count);
        var bufs = ArrayPool<byte[]?>.Shared.Rent(locs.ListedRecords.Count);
        for (int i = 0; i < amount; i++)
        {
            bufs[i] = null;
        }

        try
        {
            for (int i = 0; i < amount; i++)
            {
                if (driverParams.CancellationToken.IsCancellationRequested) return;
                var recordLocationMarker = locs.ListedRecords[i];
                if (!_mapping.TryGetValue(recordLocationMarker.Record, out var analyzerBundles)
                    || analyzerBundles.Isolated.Length == 0) continue;

                stream.Position = recordLocationMarker.Location.Min;

                var width = checked((int)recordLocationMarker.Location.Width);

                var buf = ArrayPool<byte>.Shared.Rent(width);
                bufs[i] = buf;

                var amountRead = stream.Read(buf.AsSpan().Slice(0, width));
                if (amountRead != width)
                {
                    throw new DataMisalignedException();
                }

                var frame = new MajorRecordFrame(GameConstants.Get(_gameReleaseContext.Release), new ReadOnlyMemorySlice<byte>(buf).Slice(width));

                tasks[i] = Task.WhenAll(analyzerBundles.Isolated.Select(analyzer =>
                {
                    return _dropoff.EnqueueAndWait(() =>
                    {
                        analyzer.Drive(driverParams, frame);
                    }, driverParams.CancellationToken);
                }));
            }

            for (int i = 0; i < amount; i++)
            {
                if (driverParams.CancellationToken.IsCancellationRequested) return;
                await tasks[i];
            }
        }
        finally
        {
            ArrayPool<Task>.Shared.Return(tasks);

            for (int i = 0; i < amount; i++)
            {
                var buf = bufs[i];
                if (buf == null) continue;
                ArrayPool<byte>.Shared.Return(buf);
            }
            ArrayPool<byte[]?>.Shared.Return(bufs);
        }

    }
}
