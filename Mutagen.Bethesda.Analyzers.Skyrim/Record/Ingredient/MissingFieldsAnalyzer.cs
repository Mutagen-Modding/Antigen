using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Ingredient;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<IIngredientGetter>
{
    public static readonly TopicDefinition NoPickupSound = MutagenTopicBuilder.FromDiscussion(
            226,
            "No Pickup Sound",
            Severity.Suggestion)
        .WithoutFormatting("Ingredient has no pickup sound");

    public static readonly TopicDefinition NoPutDownSound = MutagenTopicBuilder.FromDiscussion(
            307,
            "No Put Down Sound",
            Severity.Suggestion)
        .WithoutFormatting("Ingredient has no put down sound");

    public static readonly TopicDefinition<int> NotFourEffects = MutagenTopicBuilder.FromDiscussion(
            308,
            "Not Four Effects",
            Severity.Suggestion)
        .WithFormatting<int>("Ingredient has {0} effects, not 4 as it is expected");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoPickupSound, NoPutDownSound, NotFourEffects];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IIngredientGetter> param)
    {
        var ingredient = param.Record;

        if (ingredient.PickUpSound.IsNull)
        {
            param.AddTopic(
                NoPickupSound.Format());
        }

        if (ingredient.PutDownSound.IsNull)
        {
            param.AddTopic(
                NoPutDownSound.Format());
        }

        if (ingredient.Effects.Count != 4)
        {
            param.AddTopic(
                NotFourEffects.Format(ingredient.Effects.Count));
        }
    }

    public IEnumerable<Func<IIngredientGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.PickUpSound;
        yield return x => x.PutDownSound;
        yield return x => x.Effects;
    }
}
