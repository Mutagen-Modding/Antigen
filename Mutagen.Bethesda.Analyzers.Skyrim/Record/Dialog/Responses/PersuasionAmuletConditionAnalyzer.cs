using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class PersuasionAmuletConditionAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition MissingCreatedObject = MutagenTopicBuilder.FromDiscussion(
            271,
            "Missing Amulet of Articulation Condition",
            Severity.Suggestion)
        .WithoutFormatting("Persuasion check is missing auto pass condition when Amulet of Articulation is equipped");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingCreatedObject];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        var isPersuade = false;
        var hasAmuletOfArticulation = false;
        foreach (var conditionGetter in dialogResponses.Conditions) {
            switch (conditionGetter.Data) {
                case IGetActorValueConditionDataGetter { ActorValue: ActorValue.Speech }:
                    isPersuade = true;
                    break;
                case IGetEquippedConditionDataGetter { RunOnType: Condition.RunOnType.Reference } getEquipped
                    when getEquipped.Reference.Equals(FormKeys.SkyrimSE.Skyrim.PlayerRef)
                         && getEquipped.ItemOrList.Link.FormKey.Equals(FormKeys.SkyrimSE.Skyrim.FormList.TGAmuletofArticulationList.FormKey)
                         && conditionGetter.CompareOperator == CompareOperator.EqualTo
                         && Math.Abs(((IConditionFloatGetter) conditionGetter).ComparisonValue - 1) < float.Epsilon:
                    hasAmuletOfArticulation = true;
                    break;
            }
        }

        if (isPersuade && !hasAmuletOfArticulation)
        {
            param.AddTopic(
                MissingCreatedObject.Format());
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Conditions;
    }
}
