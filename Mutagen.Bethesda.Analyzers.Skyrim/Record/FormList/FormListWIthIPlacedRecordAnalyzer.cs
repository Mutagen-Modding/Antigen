using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.FormList;

public class FormListWithPlacedRecordAnalyzer : IContextualRecordAnalyzer<IFormListGetter>
{
    public static readonly TopicDefinition<IFormListGetter?,IFormKeyGetter?> FormListWithIPlacedRecord = MutagenTopicBuilder.FromDiscussion(
            597,
            "FormList holds PlacedRecords",
            Severity.Error)
        .WithFormatting<IFormListGetter?,IFormKeyGetter?>("FormList {0} holds PlacedRecord {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [FormListWithIPlacedRecord];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IFormListGetter> param)
    {
        foreach (var recordItem in param.Record.Items)
        {
            if (param.LinkCache.TryResolve(recordItem.FormKey, typeof(IPlaced), out _))
            {
                param.AddTopic(FormListWithIPlacedRecord.Format(param.Record, recordItem.ToLinkGetter()));
            }
        }
    }

    public IEnumerable<Func<IFormListGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Items;
    }
}
