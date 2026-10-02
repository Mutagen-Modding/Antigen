using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.HeadPart;

public class MissingAssetsAnalyzerHeadPart : IIsolatedRecordAnalyzer<IHeadPartGetter>
{
    private readonly MissingAssetsAnalyzerUtil _util;

    public static readonly TopicDefinition<string> MissingHeadPartModel = MutagenTopicBuilder.FromDiscussion(
            87,
            "Missing Head Part Model file",
            Severity.Error)
        .WithFormatting<string>("Missing Model file {0}");

    public static readonly TopicDefinition<int, string?> MissingHeadPartFile = MutagenTopicBuilder.FromDiscussion(
            89,
            "Missing Head Part file",
            Severity.CTD)
        .WithFormatting<int, string?>("Missing file for Head Part Part {0} at {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingHeadPartModel, MissingHeadPartFile];

    public MissingAssetsAnalyzerHeadPart(MissingAssetsAnalyzerUtil util)
    {
        _util = util;
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IHeadPartGetter> param)
    {
        _util.CheckForMissingModelAsset(param, MissingHeadPartModel);

        var i = 0;
        foreach (var part in param.Record.Parts)
        {
            if (!_util.FileExistsIfNotNull(part.FileName))
            {
                param.AddTopic(
                    MissingHeadPartFile.Format(i, part.FileName));
            }
            i++;
        }
    }

    IEnumerable<Func<IHeadPartGetter, object?>> IIsolatedRecordAnalyzer<IHeadPartGetter>.FieldsOfInterest()
    {
        yield return x => x.Parts;
    }
}
