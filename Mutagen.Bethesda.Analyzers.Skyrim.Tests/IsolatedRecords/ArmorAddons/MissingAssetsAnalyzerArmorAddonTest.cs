using Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.ArmorAddon;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.ArmorAddons;

public class MissingAssetsAnalyzerArmorAddonTest
{
    [Theory, MutagenModAutoData]
    public void TestMissingMaleWorldModel(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerArmorAddon, ArmorAddon, IArmorAddonGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
                rec.WorldModel = new GenderedItem<Model?>(new Model()
                    {
                        File = modelPath
                    }, null),
            prepForFix: rec =>
            {
                rec.WorldModel = new GenderedItem<Model?>(new Model()
                    {
                        File = existingModelPath
                    }, null);
            },
            MissingAssetsAnalyzerArmorAddon.MissingArmorAddonWorldModel);
    }

    [Theory, MutagenModAutoData]
    public void TestMissingFemaleWorldModel(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerArmorAddon, ArmorAddon, IArmorAddonGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
                rec.WorldModel = new GenderedItem<Model?>(null, new Model()
                {
                    File = modelPath
                }),
            prepForFix: rec =>
            {
                rec.WorldModel = new GenderedItem<Model?>(null, new Model()
                {
                    File = existingModelPath
                });
            },
            MissingAssetsAnalyzerArmorAddon.MissingArmorAddonWorldModel);
    }
    [Theory, MutagenModAutoData]
    public void TestMissingMaleFirstPersonModel(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerArmorAddon, ArmorAddon, IArmorAddonGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
                rec.FirstPersonModel = new GenderedItem<Model?>(new Model()
                {
                    File = modelPath
                }, null),
            prepForFix: rec =>
            {
                rec.FirstPersonModel = new GenderedItem<Model?>(new Model()
                {
                    File = existingModelPath
                }, null);
            },
            MissingAssetsAnalyzerArmorAddon.MissingArmorAddonFirstPersonModel);
    }

    [Theory, MutagenModAutoData]
    public void TestMissingFemaleFirstPersonModel(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerArmorAddon, ArmorAddon, IArmorAddonGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
                rec.FirstPersonModel = new GenderedItem<Model?>(null, new Model()
                {
                    File = modelPath
                }),
            prepForFix: rec =>
            {
                rec.FirstPersonModel = new GenderedItem<Model?>(null, new Model()
                {
                    File = existingModelPath
                });
            },
            MissingAssetsAnalyzerArmorAddon.MissingArmorAddonFirstPersonModel);
    }
}
