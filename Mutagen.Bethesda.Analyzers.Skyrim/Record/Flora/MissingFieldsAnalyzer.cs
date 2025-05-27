using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Flora;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<IFloraGetter>
{
    public static readonly TopicDefinition NoHarvestSound = MutagenTopicBuilder.FromDiscussion(
            223,
            "No Harvest Sound",
            Severity.Suggestion)
        .WithoutFormatting("Flora has no harvest sound");

    public static readonly TopicDefinition NoIngredient = MutagenTopicBuilder.FromDiscussion(
            305,
            "No Ingredient",
            Severity.Warning)
        .WithoutFormatting("Flora has no ingredient");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoHarvestSound, NoIngredient];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IFloraGetter> param)
    {
        var flora = param.Record;

        if (flora.HarvestSound.IsNull)
        {
            param.AddTopic(NoHarvestSound.Format());
        }

        if (flora.Ingredient.IsNull)
        {
            param.AddTopic(NoIngredient.Format());
        }
    }

    public IEnumerable<Func<IFloraGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.HarvestSound;
        yield return x => x.Ingredient;
    }
}
