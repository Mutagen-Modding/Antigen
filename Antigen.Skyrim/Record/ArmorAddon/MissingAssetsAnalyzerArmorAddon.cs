using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.ArmorAddon;

public class MissingAssetsAnalyzerArmorAddon : IIsolatedRecordAnalyzer<IArmorAddonGetter>
{
    private readonly MissingAssetsAnalyzerUtil _util;

    public static readonly TopicDefinition<string, string?> MissingArmorAddonWorldModel = MutagenTopicBuilder.FromDiscussion(
            84,
            "Missing Armor Addon Model file",
            Severity.Error)
        .WithFormatting<string, string?>("Missing {0} Armor Addon Model file at {1}");

    public static readonly TopicDefinition<string, string?> MissingArmorAddonFirstPersonModel = MutagenTopicBuilder.FromDiscussion(
            85,
            "Missing Armor Addon 1st Person Model file",
            Severity.Error)
        .WithFormatting<string, string?>("Missing {0} 1st Person Armor Addon Model file at {1}");

    public MissingAssetsAnalyzerArmorAddon(MissingAssetsAnalyzerUtil util)
    {
        _util = util;
    }

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingArmorAddonWorldModel, MissingArmorAddonFirstPersonModel];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IArmorAddonGetter> param)
    {
        var femaleWorldModel = param.Record.WorldModel?.Female?.File;
        if (!_util.FileExistsIfNotNull(femaleWorldModel))
        {
            param.AddTopic(
                MissingArmorAddonWorldModel.Format("female", femaleWorldModel));
        }

        var maleWorldModel = param.Record.WorldModel?.Male?.File;
        if (!_util.FileExistsIfNotNull(maleWorldModel))
        {
            param.AddTopic(
                MissingArmorAddonWorldModel.Format("male", maleWorldModel));
        }

        var femaleFirstPersonModel = param.Record.FirstPersonModel?.Female?.File;
        if (!_util.FileExistsIfNotNull(femaleFirstPersonModel))
        {
            param.AddTopic(
                MissingArmorAddonFirstPersonModel.Format("female", femaleFirstPersonModel));
        }

        var maleFirstPersonModel = param.Record.FirstPersonModel?.Male?.File;
        if (!_util.FileExistsIfNotNull(maleFirstPersonModel))
        {
            param.AddTopic(
                MissingArmorAddonFirstPersonModel.Format("male", maleFirstPersonModel));
        }
    }

    IEnumerable<Func<IArmorAddonGetter, object?>> IIsolatedRecordAnalyzer<IArmorAddonGetter>.FieldsOfInterest()
    {
        yield return x => x.WorldModel;
        yield return x => x.FirstPersonModel;
    }
}
