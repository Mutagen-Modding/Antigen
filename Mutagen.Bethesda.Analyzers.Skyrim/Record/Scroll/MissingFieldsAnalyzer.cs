using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Scroll;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<IScrollGetter>
{
    public static readonly TopicDefinition MissingVendorKeyword = MutagenTopicBuilder.FromDiscussion(
            256,
            "Missing VendorItemScroll Keyword",
            Severity.Warning)
        .WithoutFormatting("Scroll is missing VendorItemScroll keyword");

    public static readonly TopicDefinition EmptyEffectList = MutagenTopicBuilder.FromDiscussion(
            331,
            "Empty Effect List",
            Severity.Suggestion)
        .WithoutFormatting("Scroll has no effect");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingVendorKeyword, EmptyEffectList];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IScrollGetter> param)
    {
        var scroll = param.Record;

        if (scroll.Keywords == null || !scroll.Keywords.Contains(FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemScroll))
        {
            param.AddTopic(
                MissingVendorKeyword.Format());
        }

        if (scroll.Effects.Count == 0)
        {
            param.AddTopic(
                EmptyEffectList.Format());
        }
    }

    public IEnumerable<Func<IScrollGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.Effects;
    }
}
