using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;

public class VendorKeywordAnalyzer : IIsolatedRecordAnalyzer<IArmorGetter>
{
    public static readonly TopicDefinition<FormLink<IKeywordGetter>> ArmorMissingVendorKeyword = MutagenTopicBuilder.FromDiscussion(
            219,
            "Armor is missing Vendor Keyword",
            Severity.Warning)
        .WithFormatting<FormLink<IKeywordGetter>>("Missing vendor keyword {0}");

    public static readonly TopicDefinition UnsuitableVendorKeyword = MutagenTopicBuilder.FromDiscussion(
            303,
            "Armor has unsuitable Vendor Keyword",
            Severity.Suggestion)
        .WithoutFormatting("Armor has unsuitable vendor keywords");

    public IEnumerable<TopicDefinition> Topics => [ArmorMissingVendorKeyword, UnsuitableVendorKeyword];

    private static readonly HashSet<FormLink<IKeywordGetter>> AllArmorVendorKeywords =
    [
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemDaedricArtifact,
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorNoSale,
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemArmor,
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemClothing,
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemJewelry,
    ];

    private static readonly HashSet<FormLink<IKeywordGetter>> AlwaysValidVendorKeywords =
    [
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemDaedricArtifact,
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorNoSale,
        FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemArmor,
    ];

    private static readonly HashSet<FormLink<IKeywordGetter>> JeweleryKeywords =
    [
        FormKeys.SkyrimSE.Skyrim.Keyword.ClothingCirclet,
        FormKeys.SkyrimSE.Skyrim.Keyword.ClothingNecklace,
        FormKeys.SkyrimSE.Skyrim.Keyword.ClothingRing,
        FormKeys.SkyrimSE.Skyrim.Keyword.ArmorJewelry,
        FormKeys.SkyrimSE.Skyrim.Keyword.JewelryExpensive,
    ];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IArmorGetter> param)
    {
        var armor = param.Record;

        // Armor with template armor inherit all relevant data from the template armor and should not be checked themselves
        if (!armor.TemplateArmor.IsNull) return;

        // Non-playable armor should not have vendor keywords
        if (armor.MajorFlags.HasFlag(Bethesda.Skyrim.Armor.MajorFlag.NonPlayable)) return;

        // Ignore armor with no keywords, these are usually skin armor
        if (armor.Keywords is null) return;

        // No sale or Daedric artifact or the default armor vendor keywords are always allowed
        if (armor.Keywords.Intersect(AlwaysValidVendorKeywords).Any()) return;

        // Determine the expected vendor keyword based on the armor type
        var expectedVendorKeyword = armor.BodyTemplate?.ArmorType switch
        {
            ArmorType.Clothing or ArmorType.LightArmor when armor.Keywords.Intersect(JeweleryKeywords).Any()
                => FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemJewelry,
            ArmorType.Clothing
                => FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemClothing,
            _
                => FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemArmor,
        };

        if (armor.Keywords.Contains(expectedVendorKeyword)) return;

        // Collect all vendor keywords - at this point we know that these are all not the vendor keywords we are looking for
        var vendorKeywords = armor.Keywords.Intersect(AllArmorVendorKeywords).ToList();

        if (vendorKeywords.Count == 0)
        {
            param.AddTopic(
                ArmorMissingVendorKeyword.Format(expectedVendorKeyword));
            return;
        }

        param.AddTopic(
            UnsuitableVendorKeyword.Format(),
            ("Unsuitable Vendor Keywords", vendorKeywords));
    }

    public IEnumerable<Func<IArmorGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.TemplateArmor;
        yield return x => x.Keywords;
        yield return x => x.MajorFlags;
    }
}
