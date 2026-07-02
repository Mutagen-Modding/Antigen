using Mutagen.Bethesda.Analyzers.Skyrim.Extensions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Landscape;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
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

    static Array2d<P3UInt8> CreateColorArray(P3UInt8 fill)
    {
        return new Array2d<P3UInt8>(new(LandscapeExtensions.GridSize, LandscapeExtensions.GridSize), fill);
    }

    // Set up a test case. Returns a landscape in the cell directly south of the cell containing the passed record
    static Landscape Setup(Landscape land, ISkyrimMod mod)
    {
        var world = mod.Worldspaces.AddNew();
        var landCell = new Cell(mod) { Grid = new() { Point = new(0, 0) } };
        var southCell = new Cell(mod) { Grid = new() { Point = new(0, -1) } };
        world.AddCell(landCell);
        world.AddCell(southCell);

        landCell.Landscape = land;
        southCell.Landscape = new Landscape(mod);
        return southCell.Landscape;
    }

    // A landscape should not have height seams with its neighbors
    [Theory, MutagenModAutoData]
    public void LandscapeSeam(Fixture fixture)
    {
        fixture.RunWithFile(
            TestPlugin,
            GameRelease.SkyrimSE,
            errorRecord: TestLandBad,
            fixRecord: TestLandGood,
            LandscapeSeamAnalyzer.HeightMapSeam);
    }

    // A landscape should not have vertex color seams with its neighbors
    [Theory, MutagenModAutoData]
    public void ColorSeam(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var southLand = Setup(rec, mod);

                rec.VertexColors = CreateColorArray(new(255, 0, 0));
                southLand.VertexColors = CreateColorArray(new(255, 255, 0));
            },
            prepForFix: (rec, mod) =>
            {
                rec.VertexColors!.SetAllTo(new P3UInt8(255, 255, 0));
            },
            LandscapeSeamAnalyzer.VertexColorSeam);
    }

    // A landscape without vertex color data is treated as filled with (255, 255, 255)
    [Theory, MutagenModAutoData]
    public void ColorSeamEmptyNeighbor(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var southLand = Setup(rec, mod);
                southLand.VertexColors = null;
                rec.VertexColors = CreateColorArray(new(255, 0, 0));
            },
            prepForFix: (rec, mod) =>
            {
                rec.VertexColors!.SetAllTo(new P3UInt8(255, 255, 255));
            },
            LandscapeSeamAnalyzer.VertexColorSeam);
    }
}
