using Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Armors;

public class VendorKeywordAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void DetectMissingVendorKeyword(
        IsolatedRecordTestFixture<VendorKeywordAnalyzer, Armor, IArmorGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.Keywords = [];
            },
            prepForFix: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemArmor];
            },
            new[]
            {
                VendorKeywordAnalyzer.ArmorMissingVendorKeyword
            });
    }

    [Theory, MutagenModAutoData]
    public void DetectUnsuitableVendorKeyword(
        IsolatedRecordTestFixture<VendorKeywordAnalyzer, Armor, IArmorGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemJewelry];
            },
            prepForFix: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.VendorItemArmor];
            },
            new[]
            {
                VendorKeywordAnalyzer.UnsuitableVendorKeyword
            });
    }

}
