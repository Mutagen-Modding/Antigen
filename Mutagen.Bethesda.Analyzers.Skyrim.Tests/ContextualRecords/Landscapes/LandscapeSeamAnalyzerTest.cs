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
    // Landscape that is flush on its edge with TestLandNorth
    public static readonly FormKey TestHeightGood = new(TestPlugin.ModKey, 0x000D63);
    public static readonly FormKey TestColorGood = new(TestPlugin.ModKey, 0x001341);
    public static readonly FormKey TestTextureGood = new(TestPlugin.ModKey, 0x0018FC);
    // Landscape that has seams on its edge with TestLandNorth
    public static readonly FormKey TestHeightBad = new(TestPlugin.ModKey, 0x000D8F);
    public static readonly FormKey TestColorBad = new(TestPlugin.ModKey, 0x001357);
    public static readonly FormKey TestTextureBad = new(TestPlugin.ModKey, 0x00191E);

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
            errorRecord: TestHeightBad,
            fixRecord: TestHeightGood,
            LandscapeSeamAnalyzer.HeightMapSeam);
    }

    // A landscape should not have vertex color seams with its neighbors
    [Theory, MutagenModAutoData]
    public void ColorSeam(Fixture fixture)
    {
        fixture.RunWithFile(
            TestPlugin,
            GameRelease.SkyrimSE,
            errorRecord: TestColorBad,
            fixRecord: TestColorGood,
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

    // A quadrant's textures should match its neighbors
    [Theory, MutagenModAutoData]
    public void TextureSeam(Fixture fixture)
    {
        fixture.RunWithFile(
            TestPlugin,
            GameRelease.SkyrimSE,
            errorRecord: TestTextureBad,
            fixRecord: TestTextureGood,
            // Analyser will trigger once for each texture that differs
            LandscapeSeamAnalyzer.TextureSeam,
            LandscapeSeamAnalyzer.TextureSeam);
    }

    // A landscape should not be considered if its cell is not near a border region
    [Theory, MutagenModAutoData]
    public void NotInBorderRegion(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {

            },
            prepForFix: (rec, mod) =>
            {

            },
            LandscapeSeamAnalyzer.VertexColorSeam);
    }

    // A null texture should be treated as LDirt02
    [Theory, MutagenModAutoData]
    public void DefaultTexture(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {

            },
            prepForFix: (rec, mod) =>
            {

            },
            LandscapeSeamAnalyzer.VertexColorSeam);
    }
}
