using Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Armors;

public class KeywordArmorTypeAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void DetectsIncorrectArmorTypeKeywords(
        IsolatedRecordTestFixture<KeywordArmorTypeAnalyzer, Armor, IArmorGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.ArmorLight];
            },
            prepForFix: rec =>
            {
                rec.BodyTemplate = new BodyTemplate();
                rec.BodyTemplate.ArmorType = ArmorType.HeavyArmor;
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.ArmorHeavy];
            },
            new []
                {KeywordArmorTypeAnalyzer.ArmorMatchingKeywordArmorType});
    }
}
