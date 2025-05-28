using Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Armors;

public class MissingAssetsAnalyzerTests
{
    [Theory, MutagenModAutoData]
    public void TestMissingMaleArmorModel(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerArmor, Armor, IArmorGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
                rec.WorldModel = new GenderedItem<ArmorModel?>(new ArmorModel()
                {
                    Model = new Model()
                    {
                        File = modelPath
                    }
                }, null),
            prepForFix: rec =>
            {
                rec.WorldModel = new GenderedItem<ArmorModel?>(new ArmorModel()
                {
                    Model = new Model()
                    {
                        File = existingModelPath
                    }
                }, null);
            },
            MissingAssetsAnalyzerArmor.MissingArmorModel);
    }

    [Theory, MutagenModAutoData]
    public void TestMissingFemaleArmorModel(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerArmor, Armor, IArmorGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
                rec.WorldModel = new GenderedItem<ArmorModel?>(null, new ArmorModel()
                {
                    Model = new Model()
                    {
                        File = modelPath
                    }
                }),
            prepForFix: rec =>
            {
                rec.WorldModel = new GenderedItem<ArmorModel?>(null, new ArmorModel()
                {
                    Model = new Model()
                    {
                        File = existingModelPath
                    }
                });
            },
            MissingAssetsAnalyzerArmor.MissingArmorModel);
    }
}
