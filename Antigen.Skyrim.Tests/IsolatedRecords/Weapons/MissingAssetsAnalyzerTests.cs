using Antigen.Skyrim.Record.Weapon;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.IsolatedRecords.Weapons;

public class MissingAssetsAnalyzerTests
{
    [Theory, MutagenModAutoData]
    public void Typical(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerWeapon, Weapon, IWeaponGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.Model = new Model()
            {
                File = modelPath
            },
            prepForFix: rec => rec.Model = new Model()
            {
                File = existingModelPath
            },
            MissingAssetsAnalyzerWeapon.MissingWeaponModel);
    }
}
