using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Scene;

public class ConflictingVoiceTypesAnalyzer : IContextualRecordAnalyzer<ISceneGetter>
{
    public static readonly TopicDefinition<ISceneGetter, int, IFormLinkNullableGetter<IVoiceTypeGetter>> NpcsWithSameVoiceType = MutagenTopicBuilder.FromDiscussion(
            255,
            "NPCs with the same voice type in same scene",
            Severity.Suggestion)
        .WithFormatting<ISceneGetter, int, IFormLinkNullableGetter<IVoiceTypeGetter>>("Scene {0} includes {1} npcs with the same voice type {2}");

    public IEnumerable<TopicDefinition> Topics { get; } = [NpcsWithSameVoiceType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISceneGetter> param)
    {
        var scene = param.Record;

        if (!param.LinkCache.TryResolve<IQuestGetter>(scene.Quest.FormKey, out var quest)) return;

        var npcVoiceTypes = scene.Actors
            .Select(a => quest.GetAlias(a.ID))
            .WhereNotNull()
            .Select(a => a.UniqueActor.TryResolve(param.LinkCache))
            .WhereNotNull()
            .GroupBy(x => x.Voice);

        foreach (var npcVoiceType in npcVoiceTypes)
        {
            var npcWithSameVoiceType = npcVoiceType.ToList();
            if (npcWithSameVoiceType.Count <= 1) continue;

            param.AddTopic(
                NpcsWithSameVoiceType.Format(scene, npcWithSameVoiceType.Count, npcVoiceType.Key),
                ("NPCs", npcWithSameVoiceType));
        }
    }

    public IEnumerable<Func<ISceneGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Quest;
        yield return x => x.Actors;
    }
}
