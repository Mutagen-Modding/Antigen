using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Antigen.Models.Settings;
using Antigen.Services;
using Antigen.ViewModels.Settings;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SettingsWindow = Antigen.Views.Settings.SettingsWindow;

namespace Antigen.ViewModels.Analyzer;

public enum ExpandableViewState
{
    Collapsed = 0,
    Expanded = 1,
    Custom = 2
}
public sealed partial class AnalyzerVM : ViewModel
{
    private const double CollapsedHeight = 40.0;

    private readonly Subject<Unit> _returnTrigger = new();
    private readonly Func<ModKey, SettingsVM> _settingsVMFactory;

    private AnalyzerDashboard? _dashboardWindow;

    public IObservable<Unit> ReturnRequested => _returnTrigger;
    public ISettingsService SettingsService { get; }
    public ModWatcherVM ModWatcher { get; }
    public ObservableCollectionExtended<Severity> EnabledSeverities { get; } = new(Enum.GetValues<Severity>());
    public ReadOnlyObservableCollection<AnalyzerResultVM> FilteredResults { get; }

    [Reactive] public partial bool ShowDetails { get; set; }
    [Reactive] public partial string SearchText { get; set; } = string.Empty;
    [Reactive] public partial AnalyzerResultVM? CurrentSettingsViewResult { get; set; }
    [Reactive] public partial ExpandableViewState ViewState { get; set; } = ExpandableViewState.Collapsed;
    [Reactive] public partial double ExpandedViewHeight { get; set; } = 500.0;
    [Reactive] public partial double CurrentWindowHeight { get; set; } = CollapsedHeight;

    public AnalyzerVM(
        Func<ModKey, SettingsVM> settingsVMFactory,
        ISettingsService settingsService,
        ModWatcherVM modWatcher,
        ILogger<AnalyzerVM> logger)
    {
        _settingsVMFactory = settingsVMFactory;
        SettingsService = settingsService;
        ModWatcher = modWatcher;

        // Transform to vms and apply filters
        ModWatcher.AllResults
            .ToObservableChangeSet()
            .Transform(info =>
            {
                var vm = new AnalyzerResultVM(info);

                // Subscribe once when VM is created
                vm.ConfigureRequested
                    .Subscribe(targetVm => CurrentSettingsViewResult = targetVm)
                    .DisposeWith(this);

                return vm;
            })
            .Filter(EnabledSeverities.ObserveCollectionChanges()
                .Select(_ => Unit.Default)
                .StartWith(Unit.Default)
                .Select(_ => new Func<AnalyzerResultVM, bool>(result => EnabledSeverities.Contains(result.Result.Topic.Severity))))
            .Filter(SettingsService.RulesChanged
                .Select(_ => Unit.Default)
                .StartWith(Unit.Default)
                .Select(_ => new Func<AnalyzerResultVM, bool>(result => !SettingsService.IsIgnored(ModWatcher.ModKey, result.Info))))
            .Filter(this.WhenAnyValue(x => x.SearchText)
                .Select(_ => Unit.Default)
                .StartWith(Unit.Default)
                .Select(_ => new Func<AnalyzerResultVM, bool>(result =>
                {
                    if (string.IsNullOrWhiteSpace(SearchText)) return true;

                    return result.RecordDisplayName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                        result.ParentDisplayName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                        result.Result.Topic.TopicDefinition.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                        result.Result.Topic.FormattedTopic.TopicDefinition.MessageFormat?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true;
                })))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Bind(out var readOnlyObservableCollection)
            .Subscribe(
                _ => {},
                ex => logger.LogError(ex, "Error in reactive filter chain"))
            .DisposeWith(this);

        FilteredResults = readOnlyObservableCollection;

        // Update window height when view state or expanded height changes
        this.WhenAnyValue(x => x.ViewState, x => x.ExpandedViewHeight)
            .Subscribe(UpdateWindowState)
            .DisposeWith(this);
    }

    private void UpdateWindowState()
    {
        CurrentWindowHeight = ViewState == ExpandableViewState.Collapsed ? CollapsedHeight : ExpandedViewHeight;
    }

    [ReactiveCommand]
    private void ToggleSeverity(Severity severity)
    {
        if (!EnabledSeverities.Remove(severity))
        {
            EnabledSeverities.Add(severity);
        }
    }

    [ReactiveCommand]
    private void ToggleDetails()
    {
        ShowDetails = !ShowDetails;
        ViewState = ViewState == ExpandableViewState.Collapsed
            ? ExpandableViewState.Expanded
            : ExpandableViewState.Collapsed;
        UpdateWindowState();
    }

    [ReactiveCommand]
    private void SetCustomHeight(double height)
    {
        if (height < 40) height = 40; // Minimum height
        ExpandedViewHeight = height;
        ViewState = ExpandableViewState.Custom;
        UpdateWindowState();
    }

    [ReactiveCommand]
    private void Return()
    {
        _returnTrigger.OnNext(Unit.Default);
    }

    [ReactiveCommand]
    private void EnterConfigureMode(AnalyzerResultVM resultVM)
    {
        CurrentSettingsViewResult = resultVM;
    }

    [ReactiveCommand]
    private void LeaveConfigureMode()
    {
        CurrentSettingsViewResult = null;
    }

    [ReactiveCommand]
    private void IgnoreInstance(AnalyzerResultVM resultVM)
    {
        ModWatcher.IgnoreResult(resultVM.Info, IgnoreType.Instance);
        LeaveConfigureMode();
    }

    [ReactiveCommand]
    private void IgnoreTopicType(AnalyzerResultVM resultVM)
    {
        ModWatcher.IgnoreResult(resultVM.Info, IgnoreType.Topic);
        LeaveConfigureMode();
    }

    [ReactiveCommand]
    private void IgnoreRecord(AnalyzerResultVM resultVM)
    {
        ModWatcher.IgnoreResult(resultVM.Info, IgnoreType.Record);
        LeaveConfigureMode();
    }

    [ReactiveCommand]
    private void OpenSettings()
    {
        var managerWindow = new SettingsWindow(_settingsVMFactory(ModWatcher.ModKey));
        managerWindow.Show();
    }
}