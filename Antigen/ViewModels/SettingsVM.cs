using System.Reactive;
using System.Reactive.Linq;
using Antigen.Models.Settings;
using Antigen.Services;
using DynamicData.Binding;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class SettingsVM : ResizablePanelVM, ITransient
{
    private readonly NavigationController _navigation;
    private readonly AnalyzerVM _analyzerVM;

    [Reactive] public partial ObservableCollectionExtended<IgnoreRuleItem> Rules { get; set; } = [];
    [Reactive] public partial int SelectedIndex { get; set; } = -1;

    public ISettingsService SettingsService { get; }
    public ModKey ModKey => _analyzerVM.ModWatcher.ModKey;

    public SettingsVM(NavigationController navigation, AnalyzerVM analyzerVM, ISettingsService settingsService)
    {
        _navigation = navigation;
        _analyzerVM = analyzerVM;
        SettingsService = settingsService;
        IsExpanded = true;

        SettingsService.RulesChanged
            .Unit()
            .StartWith(Unit.Default)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => LoadRules())
            .DisposeWith(this);
    }

    private void LoadRules()
    {
        var rules = SettingsService.GetRules(ModKey);
        Rules.Clear();

        foreach (var rule in rules)
        {
            Rules.Add(new IgnoreRuleItem(rule));
        }
    }

    [ReactiveCommand]
    private void Back()
    {
        _navigation.GoTo(_analyzerVM);
    }

    [ReactiveCommand]
    private void RemoveSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Rules.Count) return;

        SettingsService.RemoveRule(ModKey, SelectedIndex);
        SelectedIndex = -1;
    }

    [ReactiveCommand]
    private void ClearAll()
    {
        SettingsService.ClearRules(ModKey);
    }
}

public sealed record IgnoreRuleItem(IgnoreRule Rule)
{
    public string Type => Rule.Type.ToString();
    public string Identifier => Rule.Identifier;
}
