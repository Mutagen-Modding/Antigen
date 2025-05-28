using Mutagen.Bethesda.Analyzers.Skyrim.Record.HeadPart;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.HeadParts;

public class MissingAssetsAnalyzerTests
{
    [Theory, MutagenModAutoData]
    public void Model(
        AssetLink<SkyrimModelAssetType> modelPath,
        AssetLink<SkyrimModelAssetType> existingModelPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerHeadPart, HeadPart, IHeadPartGetter> fixture)
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
            MissingAssetsAnalyzerHeadPart.MissingHeadPartModel);
    }

    [Theory, MutagenModAutoData]
    public void PartsFile(
        AssetLink<SkyrimDeformedModelAssetType> path,
        AssetLink<SkyrimDeformedModelAssetType> existingPath,
        IsolatedRecordTestFixture<MissingAssetsAnalyzerHeadPart, HeadPart, IHeadPartGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.Parts.SetTo(new Part()
                {
                    FileName = path
                });
            },
            prepForFix: rec =>
            {
                rec.Parts.SetTo(new Part()
                {
                    FileName = existingPath
                });
            },
            MissingAssetsAnalyzerHeadPart.MissingHeadPartFile);
    }
}
