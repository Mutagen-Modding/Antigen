using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;

public class KeywordSlotsAnalyzer : IIsolatedRecordAnalyzer<IArmorGetter>
{
    public static readonly TopicDefinition<string, FormLink<IKeywordGetter>> ArmorMatchingKeywordSlots = MutagenTopicBuilder.FromDiscussion(
            217,
            "Armor keywords don't match their equipped slot",
            Severity.Suggestion)
        .WithFormatting<string, FormLink<IKeywordGetter>>("Equipped in slot {0} but doesn't have keyword {1}");

    public IEnumerable<TopicDefinition> Topics => [ArmorMatchingKeywordSlots];

    public static readonly IReadOnlyList<(BipedObjectFlag Slots, FormLink<IKeywordGetter> Keyword)> ClothingKeywords =
    [
        (BipedObjectFlag.Body, FormKeys.SkyrimSE.Skyrim.Keyword.ClothingBody),
        (BipedObjectFlag.Feet, FormKeys.SkyrimSE.Skyrim.Keyword.ClothingFeet),
        (BipedObjectFlag.Hands, FormKeys.SkyrimSE.Skyrim.Keyword.ClothingHands),
        (BipedObjectFlag.Head | BipedObjectFlag.Hair, FormKeys.SkyrimSE.Skyrim.Keyword.ClothingHead),
    ];

    public static readonly IReadOnlyList<(BipedObjectFlag Slots, FormLink<IKeywordGetter> Keyword)> ArmorKeywords =
    [
        (BipedObjectFlag.Body, FormKeys.SkyrimSE.Skyrim.Keyword.ArmorCuirass),
        (BipedObjectFlag.Feet, FormKeys.SkyrimSE.Skyrim.Keyword.ArmorBoots),
        (BipedObjectFlag.Hands, FormKeys.SkyrimSE.Skyrim.Keyword.ArmorGauntlets),
        (BipedObjectFlag.Head | BipedObjectFlag.Hair, FormKeys.SkyrimSE.Skyrim.Keyword.ArmorHelmet),
    ];

    public static readonly IReadOnlyList<(BipedObjectFlag Slots, FormLink<IKeywordGetter> Keyword)> MiscKeywords =
    [
        (BipedObjectFlag.Shield, FormKeys.SkyrimSE.Skyrim.Keyword.ArmorShield),
        (BipedObjectFlag.Circlet, FormKeys.SkyrimSE.Skyrim.Keyword.ClothingCirclet),
        (BipedObjectFlag.Amulet, FormKeys.SkyrimSE.Skyrim.Keyword.ClothingNecklace),
        (BipedObjectFlag.Ring, FormKeys.SkyrimSE.Skyrim.Keyword.ClothingRing),
    ];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IArmorGetter> param)
    {
        var armor = param.Record;

        // Armor with template armor inherit all relevant data from the template armor and should not be checked themselves
        if (!armor.TemplateArmor.IsNull) return;

        // Armor with no slots are not relevant
        if (armor.BodyTemplate is null) return;

        // Ignore armor with no keywords, these are usually skin armor
        if (armor.Keywords is null) return;

        // Armor type dependent conditions for main armor slots

        var conditions = armor.BodyTemplate.ArmorType switch
        {
            ArmorType.Clothing => ClothingKeywords.ToList(),
            _ => ArmorKeywords.ToList()
        };

        // Armor type independent conditions
        conditions.AddRange(MiscKeywords);

        foreach (var (slots, keyword) in conditions)
        {
            // Check if any of the slots are selected
            if ((armor.BodyTemplate.FirstPersonFlags & slots) != 0)
            {
                if (armor.Keywords.Contains(keyword))
                {
                    return;
                }

                param.AddTopic(
                    ArmorMatchingKeywordSlots.Format(slots.ToString(), keyword));
                return;
            }
        }
    }

    public IEnumerable<Func<IArmorGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.TemplateArmor;
        yield return x => x.BodyTemplate;
        yield return x => x.Keywords;
    }
}
