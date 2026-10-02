using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Static;

public class MissingAssetsAnalyzerStatic : IIsolatedRecordAnalyzer<IStaticGetter>
{
    private readonly MissingAssetsAnalyzerUtil _util;

    public static readonly TopicDefinition<string> MissingStaticModel = MutagenTopicBuilder.FromDiscussion(
            90,
            "Missing Static Model file",
            Severity.Error)
        .WithFormatting<string>("Missing Model file {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingStaticModel];

    public MissingAssetsAnalyzerStatic(MissingAssetsAnalyzerUtil util)
    {
        _util = util;
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IStaticGetter> param)
    {
        _util.CheckForMissingModelAsset(param, MissingStaticModel);
    }

    IEnumerable<Func<IStaticGetter, object?>> IIsolatedRecordAnalyzer<IStaticGetter>.FieldsOfInterest()
    {
        yield return x => x.Model!.File;
    }
}
