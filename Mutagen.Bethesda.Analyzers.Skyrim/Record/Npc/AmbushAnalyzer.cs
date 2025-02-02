using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

class AmbushAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition AmbushMissingScript = MutagenTopicBuilder.DevelopmentTopic(
            "Ambush requires script",
            Severity.Suggestion)
        .WithoutFormatting("Npc is an ambush npc but does not have masterAmbushScript");

    public static readonly TopicDefinition AmbushAggressive = MutagenTopicBuilder.DevelopmentTopic(
            "Ambush npc aggressive",
            Severity.Warning
        ).WithoutFormatting("Ambush npcs need to be unaggressive");

    public IEnumerable<TopicDefinition> Topics { get; } = [AmbushMissingScript, AmbushAggressive];

    void IContextualRecordAnalyzer<INpcGetter>.AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        bool EditorIdContainsAmbush
            = npc.EditorID?.Contains("Ambush", StringComparison.OrdinalIgnoreCase) == true;

        bool hasScript
            = npc.HasScript("masterambushscript");
        bool unaggressive
            = npc.AIData.Aggression == Aggression.Unaggressive;

        if(EditorIdContainsAmbush && !hasScript)
        {
            param.AddTopic(AmbushMissingScript.Format());
        }

        if(!unaggressive && (EditorIdContainsAmbush || hasScript))
        {
            param.AddTopic(AmbushAggressive.Format());
        }
    }

    IEnumerable<Func<INpcGetter, object?>> IContextualRecordAnalyzer<INpcGetter>.FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.AIData;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
