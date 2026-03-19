using Mutagen.Bethesda.Analyzers.Skyrim.Record.MagicEffect;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.MagicEffects;

public class DescriptionAnalyzerTests
{
    [Theory, MutagenModAutoData]
    public void DetectsSinglePercent(
        IsolatedRecordTestFixture<DescriptionAnalyzer, MagicEffect, IMagicEffectGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.Description = new TranslatedString(Language.English,
                new KeyValuePair<Language, string>(Language.English, "Deals 50% damage"),
                new KeyValuePair<Language, string>(Language.French, "Inflige 50% de dégâts"),
                new KeyValuePair<Language, string>(Language.Danish, "50%"),
                new KeyValuePair<Language, string>(Language.German, "%50"),
                new KeyValuePair<Language, string>(Language.Arabic, "%")),
            prepForFix: rec => rec.Description = new TranslatedString(Language.English,
                new KeyValuePair<Language, string>(Language.English, "Deals 50%% damage"),
                new KeyValuePair<Language, string>(Language.French, "Inflige 50%% de dégâts"),
                new KeyValuePair<Language, string>(Language.Danish, "50%%"),
                new KeyValuePair<Language, string>(Language.German, "%%50"),
                new KeyValuePair<Language, string>(Language.Arabic, "%%")),
            new[]
            {
                DescriptionAnalyzer.MagicEffectDescriptionList,
                DescriptionAnalyzer.MagicEffectDescriptionList,
                DescriptionAnalyzer.MagicEffectDescriptionList,
                DescriptionAnalyzer.MagicEffectDescriptionList,
                DescriptionAnalyzer.MagicEffectDescriptionList
            }
            );
    }
}
