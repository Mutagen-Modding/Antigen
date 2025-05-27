using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.ArmorAddon;

public class ModelAnalyzer : IIsolatedRecordAnalyzer<IArmorAddonGetter>
{
    public static readonly TopicDefinition RedundantFemaleWorldModel = MutagenTopicBuilder.FromDiscussion(
            194,
            "Redundant Female World Model",
            Severity.Suggestion)
        .WithoutFormatting("Female world model is the same as male world model and can be removed");

    public static readonly TopicDefinition MissingMaleWorldModel = MutagenTopicBuilder.FromDiscussion(
            192,
            "Missing Male World Model",
            Severity.Error)
        .WithoutFormatting("Male world model is missing and nothing will show up in game");

    public static readonly TopicDefinition RedundantFemaleFirstPersonModel = MutagenTopicBuilder.FromDiscussion(
            195,
            "Redundant Female First Person Model",
            Severity.Suggestion)
        .WithoutFormatting("Female First person model is the same as male First person model and can be removed");

    public IEnumerable<TopicDefinition> Topics => [RedundantFemaleWorldModel, MissingMaleWorldModel, RedundantFemaleFirstPersonModel];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IArmorAddonGetter> param)
    {
        var armorAddon = param.Record;

        if (armorAddon.WorldModel?.Male is null)
        {
            param.AddTopic(MissingMaleWorldModel.Format());
        }
        else if (armorAddon.WorldModel.Female?.File.DataRelativePath == armorAddon.WorldModel.Male.File.DataRelativePath)
        {
            param.AddTopic(RedundantFemaleWorldModel.Format());
        }

        if (armorAddon.FirstPersonModel?.Male is not null
            && armorAddon.FirstPersonModel.Female?.File.DataRelativePath == armorAddon.FirstPersonModel.Male.File.DataRelativePath)
        {
            param.AddTopic(RedundantFemaleFirstPersonModel.Format());
        }
    }

    public IEnumerable<Func<IArmorAddonGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.WorldModel?.Female;
        yield return x => x.WorldModel?.Male;
        yield return x => x.FirstPersonModel?.Female;
        yield return x => x.FirstPersonModel?.Male;
    }
}
