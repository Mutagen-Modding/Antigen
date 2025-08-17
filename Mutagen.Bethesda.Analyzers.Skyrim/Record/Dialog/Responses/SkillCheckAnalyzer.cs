using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class SkillCheckAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<Condition.RunOnType> NonPlayerSkillCheck = MutagenTopicBuilder.FromDiscussion(
            273,
            "Non-Player Skill Check",
            Severity.Warning)
        .WithFormatting<Condition.RunOnType>("Skill check in dialog are not checked on the player but on {0} - this is usually a sign of a mistake");

    public static readonly TopicDefinition<float> NonGlobalSkillCheck = MutagenTopicBuilder.FromDiscussion(
            340,
            "Non-Global Skill Check",
            Severity.Suggestion)
        .WithFormatting<float>("Skill check in dialog doesn't use global to evaluate skill level but {0} - this is usually a sign of a mistake");

    public IEnumerable<TopicDefinition> Topics { get; } = [NonPlayerSkillCheck, NonGlobalSkillCheck];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        foreach (var condition in dialogResponses.Conditions)
        {
            if (condition.Data is not IGetActorValueConditionDataGetter getActorValue) continue;
            if (!getActorValue.ActorValue.IsSkill()) continue;

            // Non-Player Skill Check
            if (getActorValue.RunOnType != Condition.RunOnType.Target && !getActorValue.RunsOnPlayer())
            {
                param.AddTopic(
                    NonPlayerSkillCheck.Format(condition.Data.RunOnType));
            }
            else if (condition is IConditionFloatGetter conditionFloatGetter)
            {
                // Non-Global Skill Check on player
                param.AddTopic(
                    NonGlobalSkillCheck.Format(conditionFloatGetter.ComparisonValue));
            }
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Conditions;
    }
}
