using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Armor;

public class MissingAssetsAnalyzerArmor : IIsolatedRecordAnalyzer<IArmorGetter>
{
    private readonly MissingAssetsAnalyzerUtil _util;

    public static readonly TopicDefinition<string, string?> MissingArmorModel = MutagenTopicBuilder.FromDiscussion(
            82,
            "Missing Armor Model file",
            Severity.Error)
        .WithFormatting<string, string?>("Missing {0} model file at {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingArmorModel];

    public MissingAssetsAnalyzerArmor(MissingAssetsAnalyzerUtil util)
    {
        _util = util;
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IArmorGetter> param)
    {
        var femaleFile = param.Record.WorldModel?.Female?.Model?.File;
        if (!_util.FileExistsIfNotNull(femaleFile))
        {
            param.AddTopic(
                MissingArmorModel.Format("female", femaleFile));
        }
        var maleFile = param.Record.WorldModel?.Male?.Model?.File;
        if (!_util.FileExistsIfNotNull(maleFile))
        {
            param.AddTopic(
                MissingArmorModel.Format("male", maleFile));
        }
    }

    IEnumerable<Func<IArmorGetter, object?>> IIsolatedRecordAnalyzer<IArmorGetter>.FieldsOfInterest()
    {
        yield return x => x.WorldModel;
    }
}
