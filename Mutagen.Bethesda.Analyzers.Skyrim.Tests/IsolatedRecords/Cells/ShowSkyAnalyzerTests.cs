using Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Cells;

public class ShowSkyAnalyzerTests
{
    [Theory, MutagenModAutoData]
    public void ShowSkyWithoutRegion(
        IsolatedRecordTestFixture<ShowSkyAnalyzer, Cell, ICellGetter> fixture)
    {
        fixture.Run(
            prepForError: cell =>
            {
                cell.EditorID = "TestCell";
                cell.Flags = Cell.Flag.IsInteriorCell | Cell.Flag.ShowSky;

            },
            prepForFix: static cell =>
            {
                cell.EditorID = "TestCell";
                cell.Flags = Cell.Flag.IsInteriorCell | Cell.Flag.ShowSky;
                cell.Regions = [FormKeys.SkyrimSE.Skyrim.Region.WeatherMountains];
            },
            ShowSkyAnalyzer.ShowSkyWithoutRegion);
    }
}

