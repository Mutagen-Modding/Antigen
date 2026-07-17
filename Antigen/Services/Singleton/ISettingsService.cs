using System.Collections.Immutable;
using Antigen.Models.Analyzer;
using Antigen.Models.Settings;
using Mutagen.Bethesda.Plugins;

namespace Antigen.Services.Singleton;

public interface ISettingsService
{
    IObservable<ImmutableArray<IgnoreRule>> RulesChanged { get; }
    ImmutableArray<IgnoreRule> GetRules(ModKey modKey);
    void AddRule(ModKey modKey, IgnoreRule rule);
    void AddRule(ModKey modKey, AnalyzerResultInfo resultInfo, IgnoreType ignoreType);
    void RemoveRule(ModKey modKey, int index);
    void ClearRules(ModKey modKey);
    bool IsIgnored(ModKey modKey, AnalyzerResultInfo resultInfo);
}