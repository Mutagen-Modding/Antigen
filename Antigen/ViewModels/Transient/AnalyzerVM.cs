using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Antigen.Models.Settings;
using Antigen.Services;
using Antigen.Services.Singleton;
using Antigen.ViewModels.Analyzer;
using Antigen.Views;
using Antigen.Views.Analyzer;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SettingsWindow = Antigen.Views.Settings.SettingsWindow;

namespace Antigen.ViewModels.Transient;

public sealed partial class AnalyzerVM : ResizablePanelVM
{
    private readonly Subject<Unit> _returnTrigger = new();

    private readonly Func<ModKey, SettingsVM> _settingsVMFactory;
    private readonly Func<AnalyzerVM, DashboardVM> _dashboardVMFactory;
    private readonly IMainWindow _mainWindow;

    private AnalyzerDashboard? _dashboardWindow;

    public IObservable<Unit> ReturnRequested => _returnTrigger;
    public ISettingsService SettingsService { get; }
    public ModWatcherVM ModWatcher { get; }
    public ObservableCollectionExtended<Severity> EnabledSeverities { get; } = new(Enum.GetValues<Severity>());
    public ReadOnlyObservableCollection<AnalyzerResultVM> FilteredResults { get; }

    [Reactive] public partial string SearchText { get; set; } = string.Empty;
    [Reactive] public partial AnalyzerResultVM? CurrentSettingsViewResult { get; set; }

    public AnalyzerVM(
        Func<ModKey, SettingsVM> settingsVMFactory,
        ISettingsService settingsService,
        ModWatcherVM modWatcher,
        IMainWindow mainWindow,
        Func<AnalyzerVM, DashboardVM> dashboardVMFactory)
    {
        _settingsVMFactory = settingsVMFactory;
        _mainWindow = mainWindow;
        SettingsService = settingsService;
        ModWatcher = modWatcher;
        _dashboardVMFactory = dashboardVMFactory;

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
    private void ToggleSeverity(Severity severity)
    {
        if (!EnabledSeverities.Remove(severity))
        {
            EnabledSeverities.Add(severity);
        }
    }

    [ReactiveCommand]
    private void Return()
    {
        _returnTrigger.OnNext(Unit.Default);
    }

    [ReactiveCommand]
    private void Close()
    {
        _mainWindow.Close();
    }

    [ReactiveCommand]
    private void OpenDashboard()
    {
        if (_dashboardWindow?.PlatformImpl is null)
        {
            _dashboardWindow = new AnalyzerDashboard(_dashboardVMFactory(this));
        }

        if (_dashboardWindow.WindowState == WindowState.Minimized)
        {
            _dashboardWindow.WindowState = WindowState.Normal;
        }
        _dashboardWindow.Topmost = true;
        _dashboardWindow.Show();
        _dashboardWindow.Topmost = false;
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
