using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Antigen.Models.Analyzer;
using Antigen.Services.Game;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog;

namespace Antigen.Services.Singleton;

public interface IAnalyzerService
{
    IObservable<StatusUpdate> StatusUpdates { get; }
    Severity MinimumSeverity { get; set; }
    IAsyncEnumerable<AnalyzerResultInfo> AnalyzeAsync(ModKey modKey, CancellationToken cancellationToken = default);
}

public sealed class AnalyzerService(
    IFileSystem fileSystem,
    IDataDirectoryProvider dataDirectoryProvider,
    IModInfoProvider modInfoProvider,
    IReadOnlyList<IAnalyzer> analyzers,
    ILoadOrderListingsProvider loadOrderListingsProvider,
    IGameReleaseContext gameReleaseContext,
    IAnalyzerFilter analyzerFilter,
    IAnalyzerResultInfoFactory infoFactory,
    ILogger<AnalyzerService> logger) : IAnalyzerService
{
    private readonly Subject<StatusUpdate> _statusSubject = new();

    public IObservable<StatusUpdate> StatusUpdates => _statusSubject;

    public Severity MinimumSeverity { get; set; } = Severity.None;

    public async IAsyncEnumerable<AnalyzerResultInfo> AnalyzeAsync(ModKey modKey, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<AnalyzerResult>? results = null;

        logger.LogInformation("Starting analysis of {ModKey} with minimum severity {MinimumSeverity}", modKey, MinimumSeverity);
        var stopwatch = Stopwatch.StartNew();

        var modInfos = loadOrderListingsProvider.Get()
            .Select(l => modInfoProvider.GetModInfo(fileSystem.Path.Combine(dataDirectoryProvider.Path, l.FileName), fileSystem, gameReleaseContext.Release))
            .WhereNotNull()
            .ToArray();

        var masterInfos = modInfoProvider.GetMasterInfos(modInfos);
        var loadOrder = loadOrderListingsProvider.Get()
            .Where(l => l.ModKey == modKey || masterInfos.TryGetValue(modKey, out var masterInfo) && masterInfo.Masters.Contains(l.ModKey))
            .ToArray();

        logger.LogInformation("Resolved {LoadOrderCount} of {ModCount} mods", loadOrder.Length, modInfos.Length);

        IGameEnvironment? env = null;
        try
        {
            // Try to create the environment multiple times in case a mod file is being written to and causes an error
            var retryCount = 0;
            while (env is null && retryCount < 3)
            {
                try
                {
                    // Create environment for only the mod and its transitive dependencies
                    env = GameEnvironmentBuilder.Create(gameReleaseContext.Release)
                        .WithLoadOrder(loadOrder)
                        .Build();
                }
                catch (Exception ex)
                {
                    // We might get errors to create the environment if a mod file is currently being written to - Retry
                    retryCount++;
                    logger.LogWarning(ex, "Failed to build game environment for {ModKey} (attempt {Attempt})", modKey, retryCount);
                }
            }

            if (env is null)
            {
                ReportStatus(new StatusUpdate(AnalyzerStatus.Error, "Failed to create game environment"));
                logger.LogError("Giving up building the game environment for {ModKey} after {Attempts} attempts", modKey, retryCount);
                yield break;
            }

            try
            {
                ReportStatus(new StatusUpdate(AnalyzerStatus.Preparing, "Preparing analysis..."));

                // Get all mods except the one we're analyzing (treat as blacklisted)
                var allMods = loadOrderListingsProvider.Get().ToList();
                var notSelectedMods = allMods
                    .Where(l => l.FileName != modKey.FileName)
                    .Select(l => l.ModKey)
                    .ToArray();

                ReportStatus(new StatusUpdate(AnalyzerStatus.Preparing, "Building analyzer..."));

                // Create analyzer with all built-in analyzers
                var enabledAnalyzers = analyzers.Where(analyzerFilter.ShouldAnalyze).ToArray();
                var analyzer = AnalyzerRunnerBuilder.Create(gameReleaseContext.Release)
                    .WithLinkCache(env.LinkCache)
                    .WithAnalyzers(enabledAnalyzers)
                    .WithBlacklistedMods(notSelectedMods)
                    .WithMinimumSeverity(MinimumSeverity)
                    .WithFileSystem(fileSystem)
                    .Build();

                logger.LogInformation("Built analyzer with {AnalyzerCount} of {TotalAnalyzerCount} analyzers enabled", enabledAnalyzers.Length, analyzers.Count);

                ReportStatus(new StatusUpdate(AnalyzerStatus.Analyzing, "Running analyzers..."));

                results = analyzer.Analyze();

                ReportStatus(new StatusUpdate(AnalyzerStatus.Analyzing, "Processing results..."));
            }
            catch (Exception ex)
            {
                ReportStatus(new StatusUpdate(AnalyzerStatus.Error, $"Analysis failed: {ex.Message}"));
                logger.LogError(ex, "Analysis error");
            }

            if (results is null)
            {
                logger.LogWarning("Analysis of {ModKey} produced no runner - aborting", modKey);
                yield break;
            }

            var count = 0;
            await foreach (var result in results)
            {
                count++;
                if (count % 10 == 0)
                {
                    ReportStatus(new StatusUpdate(AnalyzerStatus.Analyzing, $"Found {count} issues..."));
                }

                yield return infoFactory.Create(result, env.LinkCache);
            }

            ReportStatus(new StatusUpdate(AnalyzerStatus.Completed, $"Analysis complete - {count} issues found"));
            logger.LogInformation("Analysis of {ModKey} completed - {IssueCount} issues found in {ElapsedMs}ms", modKey, count, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Analysis of {ModKey} was cancelled after {ElapsedMs}ms", modKey, stopwatch.ElapsedMilliseconds);
            }

            env?.Dispose();
        }
    }

    public void ReportStatus(StatusUpdate update)
    {
        _statusSubject.OnNext(update);
    }
}
