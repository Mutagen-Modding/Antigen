using System.Text.RegularExpressions;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public partial class FragmentAnalyzerQuest : IIsolatedRecordAnalyzer<IQuestGetter>
{
    [GeneratedRegex(@"^([A-Z0-9]{1,4}_)?QF_", RegexOptions.IgnoreCase)]
    private static partial Regex FragmentRegex { get; }

    public static readonly TopicDefinition<string> DuplicateFragment = MutagenTopicBuilder.DevelopmentTopic(
            "Duplicate fragment",
            Severity.Error)
        .WithFormatting<string>("Fragment function {0} is used multiple times");

    public static readonly TopicDefinition<string> EmptyFragment = MutagenTopicBuilder.DevelopmentTopic(
            "Empty fragment script",
            Severity.Suggestion)
        .WithFormatting<string>("Quest has empty fragment script {0}");

    public IEnumerable<TopicDefinition> Topics => [DuplicateFragment];

    // Find the first attached script that appears to be a fragment script
    public static string? GetFragmentScriptName(IEnumerable<IScriptEntryGetter> scripts)
    {
        return scripts
            .Select(s => s.Name)
            .FirstOrDefault(n => FragmentRegex.IsMatch(n));
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;
        if (quest.VirtualMachineAdapter == null)
            return;

        var duplicates = quest.VirtualMachineAdapter.Fragments
            .GroupBy(f => f.FragmentName)
            .Where(g => g.CountGreaterThan(1));

        foreach (var dupe in duplicates)
        {
            param.AddTopic(DuplicateFragment.Format(dupe.Key), ("Usages", dupe));
        }

        if (quest.VirtualMachineAdapter.Fragments.Count == 0)
        {
            var fragment = GetFragmentScriptName(quest.VirtualMachineAdapter.Scripts);
            if (fragment != null)
            {
                param.AddTopic(EmptyFragment.Format(fragment));
            }
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
    }
}
