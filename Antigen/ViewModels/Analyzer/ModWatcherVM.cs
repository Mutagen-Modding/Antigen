using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Antigen.Extensions;
using Antigen.Models.Analyzer;
using Antigen.Models.Settings;
using Antigen.Services;
using DynamicData.Binding;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels.Analyzer;

public sealed partial class ModWatcherVM : ViewModel
{
    private readonly IModWatcher _modWatcher;
    private readonly ISettingsService _settingsService;

    private string[] _previousResultHashes = [];

    public ModWatcherVM(
        ModKey modKey,
        Func<ModKey, IModWatcher> modWatcherFactory,
        ISettingsService settingsService)
    {
        ModKey = modKey;
        _settingsService = settingsService;
        _modWatcher = modWatcherFactory(modKey)
            .DisposeWith(this);

        Status = "Initializing...";
        AnalyzerStatus = AnalyzerStatus.Idle;
        AllResults = [];
        NewResults = [];

        _modWatcher.AnalysisCompleted
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Subscribe(observable =>
            {
                RxSchedulers.MainThreadScheduler.Schedule(() =>
                {
                    _previousResultHashes = NewResults
                        .Select(result => result.GetIdentifier())
                        .ToArray();

                    NewResults.Clear();
                    AllResults.Clear();
                });

                observable
                    .Buffer(TimeSpan.FromMilliseconds(1000), RxSchedulers.TaskpoolScheduler)
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(UpdateResults, _ => {}, OnAnalysisCompleted);
            })
            .DisposeWith(this);

        _modWatcher.StatusUpdates
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(UpdateStatus)
            .DisposeWith(this);

        _modWatcher.Start();

        _settingsService.RulesChanged
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Select(_ => (AllResults.Count(result => !_settingsService.IsIgnored(ModKey, result)), NewResults.Count(result => !_settingsService.IsIgnored(ModKey, result))))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(x =>
            {
                TotalResults = x.Item1;
                NewResultsCount = x.Item2;
            })
            .DisposeWith(this);

        Status = $"Watching {modKey.FileName}...";
    }

    [Reactive] public partial bool IsAnalyzing { get; set; }
    [Reactive] public partial string Status { get; set; }
    [Reactive] public partial AnalyzerStatus AnalyzerStatus { get; set; }
    [Reactive] public partial Severity MinimumSeverity { get; set; } = Severity.None;
    [Reactive] public partial int TotalResults { get; set; }
    [Reactive] public partial int NewResultsCount { get; set; }
    [Reactive] public partial int ResolvedResults { get; set; }
    [Reactive] public partial ObservableCollectionExtended<AnalyzerResultInfo> AllResults { get; set; }
    [Reactive] public partial ObservableCollectionExtended<AnalyzerResultInfo> NewResults { get; set; }

    public ModKey ModKey { get; }

    private void UpdateResults(IList<AnalyzerResultInfo> incomingResults)
    {
        var incomingResultsArray = incomingResults.ToArray();

        var newIncomingResults = incomingResultsArray
            .Where(result => !_previousResultHashes.Contains(result.GetIdentifier()))
            .ToArray();
        NewResults.AddRangeOptimized(newIncomingResults);

        AllResults.InsertRangeOptimized(newIncomingResults, 0);
        AllResults.AddRangeOptimized(incomingResultsArray.Except(newIncomingResults));

        TotalResults = AllResults.Count(result => !_settingsService.IsIgnored(ModKey, result));
        NewResultsCount = NewResults.Count(result => !_settingsService.IsIgnored(ModKey, result));
    }

    private void OnAnalysisCompleted()
    {
        ResolvedResults += _previousResultHashes.Length - AllResults.Count + NewResults.Count;
    }

    [ReactiveCommand]
    private void UpdateStatus(StatusUpdate update)
    {
        AnalyzerStatus = update.Status;
        Status = update.Message ?? Status;
        IsAnalyzing = update.Status is AnalyzerStatus.Analyzing or AnalyzerStatus.Preparing;
    }

    [ReactiveCommand]
    private void ChangeMinimumSeverity(Severity severity)
    {
        MinimumSeverity = severity;

        // Trigger re-analysis with new severity
        _modWatcher.Start();
    }

    [ReactiveCommand]
    public void IgnoreResult(AnalyzerResultInfo resultInfo, IgnoreType ignoreType)
    {
        _settingsService.AddRule(ModKey, resultInfo, ignoreType);
    }
}
