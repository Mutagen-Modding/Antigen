using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Scene;

public class SceneActionCompletedAnalyzer : IIsolatedRecordAnalyzer<ISceneGetter>
{
    public static readonly TopicDefinition<ISceneGetter, int, uint?> StartConditionReferencesFutureSceneAction = MutagenTopicBuilder.FromDiscussion(
            462,
            "Scene Start Condition References Future Scene Action",
            Severity.Error)
        .WithFormatting<ISceneGetter, int, uint?>(
            "Scene {0} has a start condition IsSceneActionComplete in phase {1} that references a scene action {2} that doesn't complete before the current phase");

    public static readonly TopicDefinition<ISceneGetter, int, uint?> EndConditionReferencesFutureSceneAction = MutagenTopicBuilder.FromDiscussion(
            463,
            "Scene End Condition References Future Scene Action",
            Severity.Error)
        .WithFormatting<ISceneGetter, int, uint?>(
            "Scene {0} has an end condition IsSceneActionComplete in phase {1} that references a scene action {2} that doesn't complete before or in the current phase");

    public static readonly TopicDefinition<ISceneGetter, int, uint?> IsSceneActionCompleteReferencesMissingAction = MutagenTopicBuilder.FromDiscussion(
            464,
            "IsSceneActionComplete References Missing Action",
            Severity.Error)
        .WithFormatting<ISceneGetter, int, uint?>(
            "Scene {0} has a IsSceneActionComplete condition in phase {1} that references a scene action {2} that does not exist in the scene");

    public IEnumerable<TopicDefinition> Topics { get; } = [
        StartConditionReferencesFutureSceneAction,
        EndConditionReferencesFutureSceneAction,
        IsSceneActionCompleteReferencesMissingAction
    ];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISceneGetter> param)
    {
        var scene = param.Record;

        for (var phaseIndex = 0; phaseIndex < scene.Phases.Count; phaseIndex++)
        {
            var phase = scene.Phases[phaseIndex];
            var actualPhaseIndex = phaseIndex + 1;
            foreach (var sceneActionCondition in phase.StartConditions
                         .Select(c => c.Data)
                         .OfType<IIsSceneActionCompleteConditionDataGetter>())
            {
                if (!sceneActionCondition.Scene.UsesLink()) continue;
                if (sceneActionCondition.Scene.Link.FormKey != scene.FormKey) continue;

                var action = scene.GetAction(sceneActionCondition.SceneActionIndex);
                if (action == null)
                {
                    param.AddTopic(
                        IsSceneActionCompleteReferencesMissingAction.Format(scene, actualPhaseIndex, (uint)sceneActionCondition.SceneActionIndex));
                    continue;
                }

                if (action.EndPhase >= actualPhaseIndex)
                {
                    param.AddTopic(
                        StartConditionReferencesFutureSceneAction.Format(scene, actualPhaseIndex, action.Index));
                }
            }

            foreach (var sceneActionCondition in phase.CompletionConditions
                         .Select(c => c.Data)
                         .OfType<IIsSceneActionCompleteConditionDataGetter>())
            {
                if (!sceneActionCondition.Scene.UsesLink()) continue;
                if (sceneActionCondition.Scene.Link.FormKey != scene.FormKey) continue;

                var action = scene.GetAction(sceneActionCondition.SceneActionIndex);
                if (action == null)
                {
                    param.AddTopic(
                        IsSceneActionCompleteReferencesMissingAction.Format(scene, actualPhaseIndex, (uint)sceneActionCondition.SceneActionIndex));
                    continue;
                }

                if (action.EndPhase > actualPhaseIndex)
                {
                    param.AddTopic(
                        EndConditionReferencesFutureSceneAction.Format(scene, actualPhaseIndex, action.Index));
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
