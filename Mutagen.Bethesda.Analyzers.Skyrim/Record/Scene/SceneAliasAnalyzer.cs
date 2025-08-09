using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Scene;

public class SceneAliasAnalyzer : IContextualRecordAnalyzer<ISceneGetter>
{
    public static readonly TopicDefinition<uint, IQuestGetter> SceneActorMissingAlias = MutagenTopicBuilder.FromDiscussion(
            458,
            "Scene Actor Missing Alias",
            Severity.Error)
        .WithFormatting<uint, IQuestGetter>("Actor {0} in scene has no equivalent alias in quest {1}");

    public static readonly TopicDefinition<uint, uint?, IQuestGetter> SceneActionActorMissingAlias = MutagenTopicBuilder.FromDiscussion(
            459,
            "Scene Action Actor Missing Alias",
            Severity.Error)
        .WithFormatting<uint, uint?, IQuestGetter>("Actor {0} in scene action {1} has no equivalent alias in quest {2}");

    public static readonly TopicDefinition<uint, uint?, IQuestGetter> SceneActionHeadtrackActorMissingAlias = MutagenTopicBuilder.FromDiscussion(
            460,
            "Scene Action Headtrack Actor Missing Alias",
            Severity.Error)
        .WithFormatting<uint, uint?, IQuestGetter>("Headtrack actor {0} in scene action {1} has no equivalent alias in quest {2}");

    public IEnumerable<TopicDefinition> Topics { get; } = [SceneActorMissingAlias, SceneActionActorMissingAlias, SceneActionHeadtrackActorMissingAlias];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISceneGetter> param)
    {
        var scene = param.Record;
        if (scene.IsDeleted) return;

        var quest = scene.Quest.TryResolve(param.LinkCache);
        if (quest is null) return;

        foreach (var actor in scene.Actors)
        {
            if (!quest.HasAlias(actor.ID))
            {
                param.AddTopic(
                    SceneActorMissingAlias.Format(actor.ID, quest));
            }
        }

        foreach (var action in scene.Actions)
        {
            if (action.ActorID is not null)
            {
                var id = (uint)action.ActorID;
                if (!quest.HasAlias(id))
                {
                    param.AddTopic(
                        SceneActionActorMissingAlias.Format(id, action.Index, quest));
                }
            }

            if (action.HeadtrackActorID is not null)
            {
                var id = (uint)action.HeadtrackActorID;
                if (!quest.HasAlias(id))
                {
                    param.AddTopic(
                        SceneActionHeadtrackActorMissingAlias.Format(id, action.Index, quest));
                }
            }
        }
    }

    public IEnumerable<Func<ISceneGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Actors;
    }
}
