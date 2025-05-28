using Mutagen.Bethesda.Analyzers.Skyrim.Record.TextureSet;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.TextureSets;

public class MissingAssetsAnalyzerTests
{
    [Theory, MutagenModAutoData]
    public void Diffuse(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.Diffuse = path,
            prepForFix: rec => rec.Diffuse = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }

    [Theory, MutagenModAutoData]
    public void NormalOrGloss(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.NormalOrGloss = path,
            prepForFix: rec => rec.NormalOrGloss = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }

    [Theory, MutagenModAutoData]
    public void EnvironmentMaskOrSubsurfaceTint(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.EnvironmentMaskOrSubsurfaceTint = path,
            prepForFix: rec => rec.EnvironmentMaskOrSubsurfaceTint = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }

    [Theory, MutagenModAutoData]
    public void GlowOrDetailMap(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.GlowOrDetailMap = path,
            prepForFix: rec => rec.GlowOrDetailMap = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }

    [Theory, MutagenModAutoData]
    public void Height(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.Height = path,
            prepForFix: rec => rec.Height = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }

    [Theory, MutagenModAutoData]
    public void Environment(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.Environment = path,
            prepForFix: rec => rec.Environment = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }

    [Theory, MutagenModAutoData]
    public void Multilayer(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.Multilayer = path,
            prepForFix: rec => rec.Multilayer = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }

    [Theory, MutagenModAutoData]
    public void BacklightMaskOrSpecular(
        AssetLink<SkyrimTextureAssetType> path,
        AssetLink<SkyrimTextureAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerTextureSet, TextureSet, ITextureSetGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.BacklightMaskOrSpecular = path,
            prepForFix: rec => rec.BacklightMaskOrSpecular = existingPath,
            MissingAssetsAnalyzerTextureSet.MissingTextureInTextureSet);
    }
}
