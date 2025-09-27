using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class GhostKeywordAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition GhostScriptMissingKeyword = MutagenTopicBuilder.FromDiscussion(
            246,
            "Ghost With Script Missing Keyword",
            Severity.Suggestion)
        .WithoutFormatting("Npc has ghost script but no ghost keyword");

    public static readonly TopicDefinition GhostScriptMissingDoesntBleedFlag = MutagenTopicBuilder.FromDiscussion(
            319,
            "Ghost With Script Missing DoesNotBleed Flag",
            Severity.Warning)
        .WithoutFormatting("Npc has ghost script but no DoesNotBleed flag");

    public static readonly TopicDefinition GhostFlagMissingKeyword = MutagenTopicBuilder.FromDiscussion(
            321,
            "Ghost With Flag Missing Keyword",
            Severity.Suggestion)
        .WithoutFormatting("Npc has ghost flag but no ghost keyword");

    public IEnumerable<TopicDefinition> Topics { get; } = [GhostScriptMissingKeyword, GhostFlagMissingKeyword];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        var hasKeyword = npc.HasKeyword(FormKeys.SkyrimSE.Skyrim.Keyword.ActorTypeGhost);
        var hasFlag = npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.IsGhost);
        var hasScript = npc.HasScript("defaultGhostScript");

        if (hasScript)
        {
            if (!hasKeyword)
            {
                param.AddTopic(
                    GhostScriptMissingKeyword.Format());
            }

            if (npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.DoesNotBleed) == false)
            {
                param.AddTopic(
                    GhostScriptMissingDoesntBleedFlag.Format());
            }
        }

        if (hasFlag && !hasKeyword)
        {
            param.AddTopic(
                GhostFlagMissingKeyword.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.Configuration.Flags;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
