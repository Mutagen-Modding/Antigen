using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class AmbushAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition AmbushMissingScript = MutagenTopicBuilder.DevelopmentTopic(
            "Ambush requires script",
            Severity.Suggestion)
        .WithoutFormatting("Npc is called ambush npc but does not have an ambush script");

    public static readonly TopicDefinition AmbushNotInEditorId = MutagenTopicBuilder.DevelopmentTopic(
            "Ambush not in EditorId,",
            Severity.Suggestion)
        .WithoutFormatting("Npc has ambush script but is not called 'Ambush' in the EditorId");

    public static readonly TopicDefinition<Aggression> AmbushAggressive = MutagenTopicBuilder.FromDiscussion(
            176,
            "Ambush npc aggressive",
            Severity.Error)
        .WithFormatting<Aggression>("NPC with ambush script is {0} not Unaggressive");

    public static readonly TopicDefinition AmbushPackages = MutagenTopicBuilder.DevelopmentTopic(
            "Ambush npc without ambush packages",
            Severity.Warning)
        .WithoutFormatting("NPC with ambush script is not using ambush packages");

    public IEnumerable<TopicDefinition> Topics { get; } = [AmbushMissingScript, AmbushNotInEditorId, AmbushAggressive];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
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

        var hasAmbushPackages = npc.Packages
            .Select(p => p.TryResolve(param.LinkCache))
            .WhereNotNull()
            .Any(p => p.EditorID is not null && p.EditorID.Contains("ambush", StringComparison.OrdinalIgnoreCase));

        var hasTemplatePackages = npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.AIPackages);

        // Assume when using template packages that the template doesn't use ambush packages
        // Maybe revisit to analyze the template(s) as well in the future
        if (!hasAmbushPackages || hasTemplatePackages)
        {
            param.AddTopic(AmbushPackages.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.AIData;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
