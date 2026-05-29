using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Scene;

public class FragmentAnalyzerScene : IIsolatedRecordAnalyzer<ISceneGetter>
{
    public static readonly TopicDefinition<string> DuplicateFragment = MutagenTopicBuilder.DevelopmentTopic(
            "Duplicate fragment scene",
            Severity.Error)
        .WithFormatting<string>("Fragment function {0} is used multiple times");

    public static readonly TopicDefinition EmptyFragment = MutagenTopicBuilder.DevelopmentTopic(
            "Empty fragment scene",
            Severity.Suggestion)
        .WithoutFormatting("Scene has script attached, but no fragments");

    public IEnumerable<TopicDefinition> Topics => [DuplicateFragment, EmptyFragment];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISceneGetter> param)
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
            var names = vmad.ScriptFragments.PhaseFragments
                .Select(f => f.FragmentName)
                .Append(vmad.ScriptFragments.OnBegin?.FragmentName)
                .Append(vmad.ScriptFragments.OnEnd?.FragmentName);
            FragmentAnalyzerUtil.CheckDuplicateFragments(
                param,
                DuplicateFragment,
                names);
        }
    }

    public IEnumerable<Func<ISceneGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
    }
}
