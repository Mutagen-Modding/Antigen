using Antigen.Models.Settings;
using Antigen.Services;
using DynamicData.Binding;
using Mutagen.Bethesda.Plugins;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels.Settings;

public sealed partial class SettingsVM : ViewModel
{

    public SettingsVM(ISettingsService settingsService, ModKey modKey)
    {
        SettingsService = settingsService;
        ModKey = modKey;

        LoadRules();
    }
    [Reactive] public partial ObservableCollectionExtended<IgnoreRuleItem> Rules { get; set; } = [];
    [Reactive] public partial int SelectedIndex { get; set; } = -1;

    public ISettingsService SettingsService { get; }
    public ModKey ModKey { get; }

    [ReactiveCommand]
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
    private void RemoveSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Rules.Count) return;

        SettingsService.RemoveRule(ModKey, SelectedIndex);
        Rules.RemoveAt(SelectedIndex);
        SelectedIndex = -1;
    }

    [ReactiveCommand]
    private void ClearAll()
    {
        SettingsService.ClearRules(ModKey);
        Rules.Clear();
    }
}
public sealed record IgnoreRuleItem(IgnoreRule Rule)
{
    public string Type => Rule.Type.ToString();
    public string Identifier => Rule.Identifier;
}