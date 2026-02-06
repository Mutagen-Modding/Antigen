using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Scene;

public class SceneActionOverlapAnalyzer : IIsolatedRecordAnalyzer<ISceneGetter>
{
    public static readonly TopicDefinition<uint?, uint?, uint> OverlappingActionsOfSameType = MutagenTopicBuilder.FromDiscussion(
            530,
            "Overlapping Scene Actions of Same Type",
            Severity.Error)
        .WithFormatting<uint?, uint?, uint>(
            "Scene actions {0} and {1} for actor {2} are both of the same type and have overlapping phases");

    public IEnumerable<TopicDefinition> Topics { get; } = [OverlappingActionsOfSameType];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISceneGetter> param)
    {
        var scene = param.Record;

        // Group actions by actor ID
        var actionsByActor = scene.Actions
            .Where(a => a.ActorID is not null && a.ActorID >= 0)
            .GroupBy(a => (uint)a.ActorID!.Value);

        foreach (var actorActions in actionsByActor)
        {
            var actorId = actorActions.Key;
            var actions = actorActions.ToList();

            // Check each pair of actions for the same actor
            for (var i = 0; i < actions.Count; i++)
            {
                for (var j = i + 1; j < actions.Count; j++)
                {
                    var action1 = actions[i];
                    var action2 = actions[j];

                    // Check if actions are of the same type
                    if (action1.Type != action2.Type) continue;

                    // Check if phases overlap
                    var action1Start = action1.StartPhase ?? 0;
                    var action1End = action1.EndPhase ?? int.MaxValue;
                    var action2Start = action2.StartPhase ?? 0;
                    var action2End = action2.EndPhase ?? int.MaxValue;

                    // Check for overlap: actions overlap if one starts before the other ends
                    var overlaps = action1Start <= action2End && action2Start <= action1End;

                    if (overlaps)
                    {
                        param.AddTopic(
                            OverlappingActionsOfSameType.Format(
                                action1.Index,
                                action2.Index,
                                actorId));
                    }
                }
            }
        }
    }

    public IEnumerable<Func<ISceneGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Actions;
        yield return x => x.Phases;
    }
}
