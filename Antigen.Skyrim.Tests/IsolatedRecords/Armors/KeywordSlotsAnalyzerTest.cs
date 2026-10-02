using Antigen.Skyrim.Record.Armor;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.IsolatedRecords.Armors;

public class KeywordSlotsAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void DetectsIncorrectArmorSlotKeywords(
        IsolatedRecordTestFixture<KeywordSlotsAnalyzer, Armor, IArmorGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.BodyTemplate.FirstPersonFlags = BipedObjectFlag.Body;
                //Set any Keyword to overwrite existing ones, to make sure the required keyword isn't set
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.ArmorHeavy];
            },
            prepForFix: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.BodyTemplate.FirstPersonFlags = BipedObjectFlag.Body;
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.ArmorCuirass];
            },
            new []
                {KeywordSlotsAnalyzer.ArmorMatchingKeywordSlots});
    }
}
