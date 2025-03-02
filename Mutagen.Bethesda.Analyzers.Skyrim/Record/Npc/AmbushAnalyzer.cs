using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class AmbushAnalyzer : IIsolatedRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition AmbushMissingScript = MutagenTopicBuilder.DevelopmentTopic(
            "Ambush requires script",
            Severity.Suggestion)
        .WithoutFormatting("Npc is called ambush npc but does not have an ambush script");

    public static readonly TopicDefinition AmbushNotInEditorId = MutagenTopicBuilder.DevelopmentTopic(
        "Ambush not in EditorId,",
        Severity.Suggestion)
        .WithoutFormatting("Npc has ambush script but is not called 'Ambush' in the EditorId");

    public static readonly TopicDefinition<Aggression> AmbushAggressive = MutagenTopicBuilder.DevelopmentTopic(
            "Ambush npc aggressive",
            Severity.Error)
        .WithFormatting<Aggression>("NPC with ambush script is {0} not Unaggressive");

    public IEnumerable<TopicDefinition> Topics { get; } = [AmbushMissingScript, AmbushNotInEditorId, AmbushAggressive];

    void IIsolatedRecordAnalyzer<INpcGetter>.AnalyzeRecord(IsolatedRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        bool editorIdContainsAmbush
            = npc.EditorID?.Contains("Ambush", StringComparison.OrdinalIgnoreCase) == true;

        bool hasScript
            = npc.VirtualMachineAdapter?.Scripts
                .Any(s => s.Name.Contains("Ambush", StringComparison.OrdinalIgnoreCase)) == true;

        if (editorIdContainsAmbush && !hasScript)
        {
            param.AddTopic(AmbushMissingScript.Format());
        }
        if (!editorIdContainsAmbush && hasScript)
        {
            param.AddTopic(AmbushNotInEditorId.Format());
        }

        if (!hasScript) return;

        var aggression = npc.AIData.Aggression;
        if (aggression != Aggression.Unaggressive)
        {
            param.AddTopic(AmbushAggressive.Format(aggression));
        }
    }

    IEnumerable<Func<INpcGetter, object?>> IIsolatedRecordAnalyzer<INpcGetter>.FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.AIData;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
