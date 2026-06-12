using Mutagen.Bethesda.Analyzers.Skyrim.Record.Landscape;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Landscapes;

using Fixture = ContextualRecordTestFixture<LandscapeSeamAnalyzer, Landscape, ILandscapeGetter>;

public class LandscapeSeamAnalyzerTest
{
    public static readonly ModPath TestPlugin = "Files/LandSeamTest.esp";
    // Landscape that is flush on its south edge with TestLandNorth
    public static readonly FormKey TestLandGood = new(TestPlugin.ModKey, 0x000D63);
    // Landscape that has seams on its south edge with TestLandNorth
    public static readonly FormKey TestLandBad = new(TestPlugin.ModKey, 0x000D8F);

    // Landscape that is flush on its north edge with TestLandGood but has seams with TestLandBad
    public static readonly FormKey TestLandSouth = new(TestPlugin.ModKey, 0x000D8D);

    public static ISkyrimModDisposableGetter GetTestingPlugin()
    {
        return SkyrimMod.CreateFromBinaryOverlay(TestPlugin, SkyrimRelease.SkyrimSE);
    }

    [Theory, MutagenModAutoData]
    public void LandscapeSeam(Fixture fixture)
    {
        using var dataMod = GetTestingPlugin();
        using var linkCache = dataMod.ToImmutableLinkCache();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var world = mod.Worldspaces.AddNew();
                var landCell = new Cell(mod) { Grid = new() { Point = new(0, 0) } };
                var southCell = new Cell(mod) { Grid = new() { Point = new(0, -1) } };
                world.AddCell(landCell);
                world.AddCell(southCell);

                landCell.Landscape = rec;
                southCell.Landscape = new Landscape(mod);

                rec.VertexHeightMap = linkCache.Resolve<ILandscapeGetter>(TestLandBad).VertexHeightMap!.DeepCopy();
                southCell.Landscape.VertexHeightMap = linkCache.Resolve<ILandscapeGetter>(TestLandSouth).VertexHeightMap!.DeepCopy();
            },
            prepForFix: (rec, mod) =>
            {
                rec.VertexHeightMap = linkCache.Resolve<ILandscapeGetter>(TestLandGood).VertexHeightMap!.DeepCopy();
            },
            LandscapeSeamAnalyzer.HeightMapSeam);
    }
}
