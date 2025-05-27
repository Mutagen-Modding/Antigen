using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Key;

public class VendorKeywordAnalyzer : IIsolatedRecordAnalyzer<IKeyGetter>
{
    public static readonly TopicDefinition MissingVendorItemKeyword = MutagenTopicBuilder.FromDiscussion(
            228,
            "Missing VendorItemKey Keyword",
            Severity.Suggestion)
        .WithoutFormatting("Key is missing the VendorItemKey keyword");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingVendorItemKeyword];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IKeyGetter> param)
    {
        var key = param.Record;
        if (key.IsDeleted) return;

        if (key.Keywords is null || !key.Keywords.Contains(FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemKey))
        {
            param.AddTopic(MissingVendorItemKeyword.Format());
        }
    }

    public IEnumerable<Func<IKeyGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
    }
}
