using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Antigen.Services;
using Antigen.ViewModels.Analyzer;
using DynamicData;
using DynamicData.Binding;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class AnalyzerVM : ResizablePanelVM, ITransient
{
    private readonly NavigationController _navigation;
    private readonly HomeVM _homeVM;
    private readonly Func<AnalyzerVM, SettingsVM> _settingsVMFactory;
    private readonly Func<AnalyzerVM, DashboardVM> _dashboardVMFactory;

    private SettingsVM? _settingsVM;
    private DashboardVM? _dashboardVM;
    private AnalyzerResultVM? _configuringResult;

    public ISettingsService SettingsService { get; }
    public ModWatcherVM ModWatcher { get; }
    public ObservableCollectionExtended<Severity> EnabledSeverities { get; } = new(Enum.GetValues<Severity>());
    public ReadOnlyObservableCollection<AnalyzerResultVM> FilteredResults { get; }

    [Reactive] public partial string SearchText { get; set; } = string.Empty;

    public AnalyzerVM(
        NavigationController navigation,
        HomeVM homeVM,
        Func<AnalyzerVM, SettingsVM> settingsVMFactory,
        ISettingsService settingsService,
        ModWatcherVM modWatcher,
        Func<AnalyzerVM, DashboardVM> dashboardVMFactory)
    {
        _navigation = navigation;
        _homeVM = homeVM;
        _settingsVMFactory = settingsVMFactory;
        SettingsService = settingsService;
        ModWatcher = modWatcher;
        _dashboardVMFactory = dashboardVMFactory;
        IsExpanded = true;

        // Transform to vms and apply filters
        ModWatcher.AllResults
            .ToObservableChangeSet()
            .Transform(info =>
            {
                var vm = new AnalyzerResultVM(info, ModWatcher.IgnoreResult);

                // Only one row's ignore overlay is open at a time; close the previous one
                vm.ConfigureRequested
                    .Subscribe(targetVm =>
                    {
                        if (_configuringResult is { } previous && previous != targetVm)
                        {
                            previous.IsConfiguring = false;
                        }

                        _configuringResult = targetVm;
                    })
                    .DisposeWith(this);

                return vm;
            })
            .Filter(EnabledSeverities.ObserveCollectionChanges()
                .Unit()
                .StartWith(Unit.Default)
                .Select(_ => new Func<AnalyzerResultVM, bool>(result => EnabledSeverities.Contains(result.Result.Topic.Severity))))
            .Filter(SettingsService.RulesChanged
                .Unit()
                .StartWith(Unit.Default)
                .Select(_ => new Func<AnalyzerResultVM, bool>(result => !SettingsService.IsIgnored(ModWatcher.ModKey, result.Info))))
            .Filter(this.WhenAnyValue(x => x.SearchText)
                .Unit()
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
            .Subscribe(_ => {})
            .DisposeWith(this);

        FilteredResults = readOnlyObservableCollection;
    }

    [ReactiveCommand]
    private void Back()
    {
        _navigation.GoTo(_homeVM);
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
    private void OpenDashboard()
    {
        _navigation.GoTo(_dashboardVM ??= _dashboardVMFactory(this));
    }

    [ReactiveCommand]
    private void OpenSettings()
    {
        _navigation.GoTo(_settingsVM ??= _settingsVMFactory(this));
    }
}
