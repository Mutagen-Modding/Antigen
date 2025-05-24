using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class DefaultFaceAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition DefaultFaceMorph = MutagenTopicBuilder.FromDiscussion(
            243,
            "Default Face Morph",
            Severity.Suggestion)
        .WithoutFormatting("Npc has no custom face morph data");

    public static readonly TopicDefinition DefaultFaceParts = MutagenTopicBuilder.FromDiscussion(
            318,
            "Default Face Parts",
            Severity.Suggestion)
        .WithoutFormatting("Npc has no custom face parts data");

    public IEnumerable<TopicDefinition> Topics { get; } = [DefaultFaceMorph, DefaultFaceParts];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        if (!npc.HasKeyword(FormKeys.SkyrimSE.Skyrim.Keyword.ActorTypeNPC)) return;

        var race = npc.Race.TryResolve(param.LinkCache);
        if (race is null || !race.Flags.HasFlag(Race.Flag.FaceGenHead)) return;

        if (npc.FaceMorph is null)
        {
            param.AddTopic(DefaultFaceMorph.Format());
        }

        if (npc.FaceParts is null)
        {
            param.AddTopic(DefaultFaceParts.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.Race;
        yield return x => x.FaceMorph;
        yield return x => x.FaceParts;
    }
}
