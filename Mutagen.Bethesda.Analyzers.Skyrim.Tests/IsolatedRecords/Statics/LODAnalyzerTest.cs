using Mutagen.Bethesda.Analyzers.Skyrim.Record.Static;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Statics;

public class LODAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void TestFullModelInLod(
        IsolatedRecordTestFixture<LODAnalyzer, Static, IStaticGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                var assetLink = new AssetLink<SkyrimModelAssetType>("meshes/some/path.nif");
                rec.Model ??= new Model();
                rec.Model.File = assetLink;
                rec.Lod = new Lod
                {
                    Level0 = assetLink
                };
            },
            prepForFix: rec =>
            {
                rec.Lod?.Level0 = new AssetLink<SkyrimModelAssetType>("meshes/lod/some/path.nif");
            },
            LODAnalyzer.FullModelInLod);
    }
}
