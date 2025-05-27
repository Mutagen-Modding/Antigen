using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;

public class KeywordArmorTypeAnalyzer : IIsolatedRecordAnalyzer<IArmorGetter>
{
    public static readonly TopicDefinition<ArmorType> ArmorMatchingKeywordArmorType = MutagenTopicBuilder.FromDiscussion(
            216,
            "Armor keywords don't match their equipped armor type",
            Severity.Suggestion)
        .WithFormatting<ArmorType>("Has armor type {0} but doesn't have matching keywords");

    public IEnumerable<TopicDefinition> Topics => [ArmorMatchingKeywordArmorType];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IArmorGetter> param)
    {
        var armor = param.Record;

        // Armor with template armor inherit all relevant data from the template armor and should not be checked themselves
        if (!armor.TemplateArmor.IsNull) return;

        // Armor with no slots are not relevant
        if (armor.BodyTemplate is null) return;

        // Ignore armor with no keywords, these are usually skin armor
        if (armor.Keywords is null) return;

        // Shields always have the same keyword ArmorShield
        if (armor.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Shield)) return;

        List<FormLink<IKeywordGetter>> matchingKeywords = armor.BodyTemplate.ArmorType switch
        {
            ArmorType.LightArmor => [FormKeys.SkyrimSE.Skyrim.Keyword.ArmorLight],
            ArmorType.HeavyArmor => [FormKeys.SkyrimSE.Skyrim.Keyword.ArmorHeavy],
            ArmorType.Clothing => [FormKeys.SkyrimSE.Skyrim.Keyword.ArmorClothing, FormKeys.SkyrimSE.Skyrim.Keyword.ArmorJewelry],
            _ => throw new InvalidOperationException()
        };

        foreach (var keyword in matchingKeywords)
        {
            if (armor.Keywords.Contains(keyword)) return;
        }

        param.AddTopic(
            ArmorMatchingKeywordArmorType.Format(armor.BodyTemplate.ArmorType),
            ("Matching Keywords", matchingKeywords));
    }

    public IEnumerable<Func<IArmorGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
    }
}
