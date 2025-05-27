using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Outfit;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<IOutfitGetter>
{
    public static readonly TopicDefinition NoItems = MutagenTopicBuilder.FromDiscussion(
            248,
            "No Items",
            Severity.Warning)
        .WithoutFormatting("Outfit has no items");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoItems];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IOutfitGetter> param)
    {
        var outfit = param.Record;

        if (outfit.Items is { Count: 0 })
        {
            param.AddTopic(
                NoItems.Format()
            );
        }
    }

    public IEnumerable<Func<IOutfitGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Items;
    }
}
