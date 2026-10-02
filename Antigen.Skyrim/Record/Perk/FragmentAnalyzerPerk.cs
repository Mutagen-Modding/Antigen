using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Perk;

public class FragmentAnalyzerPerk : IIsolatedRecordAnalyzer<IPerkGetter>
{
    public static readonly TopicDefinition<string> DuplicateFragment = MutagenTopicBuilder.FromDiscussion(
            589,
            "Duplicate perk fragment name",
            Severity.Error)
        .WithFormatting<string>("Fragment function {0} is used multiple times");

    public static readonly TopicDefinition EmptyFragment = MutagenTopicBuilder.FromDiscussion(
            590,
            "Empty perk fragment script",
            Severity.Suggestion)
        .WithoutFormatting("Perk has script attached, but no fragments");

    public IEnumerable<TopicDefinition> Topics => [DuplicateFragment, EmptyFragment];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IPerkGetter> param)
    {
        var vmad = param.Record.VirtualMachineAdapter;
        if (vmad == null)
            return;

        if (vmad.ScriptFragments == null)
        {
            param.AddTopic(EmptyFragment.Format());
        }
        else
        {
            FragmentAnalyzerUtil.CheckDuplicateFragments(
                param,
                DuplicateFragment,
                vmad.ScriptFragments.Fragments.Select(f => f.FragmentName));
        }
    }

    public IEnumerable<Func<IPerkGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
    }
}
