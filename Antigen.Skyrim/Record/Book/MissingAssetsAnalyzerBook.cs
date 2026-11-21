using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Book;

public class MissingAssetsAnalyzerBook(MissingAssetsAnalyzerUtil util) : IIsolatedRecordAnalyzer<IBookGetter>
{
    public static readonly TopicDefinition<string> MissingModel = MutagenTopicBuilder.FromDiscussion(
            668,
            "Missing Book Model file",
            Severity.Error)
        .WithFormatting<string>("Missing Model file {0}");

    public IEnumerable<TopicDefinition> Topics => [MissingModel];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IBookGetter> param)
    {
        util.CheckForMissingModelAsset(param, MissingModel);
    }

    public IEnumerable<Func<IBookGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Model;
    }
}
