using Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Exterior;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Cells.Exterior;

public class EncounterZoneAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void DetectsExteriorCellWithEncounterZone(
        IsolatedRecordTestFixture<EncounterZoneAnalyzer, Cell, ICellGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.EncounterZone = new FormLinkNullable<IEncounterZoneGetter>(new FormKey(new ModKey("Test", ModType.Plugin), 7));
            },
            prepForFix: rec =>
            {
                //nothing to set up, Cell by default is Exterior without Encounter Zone
            },
            new[]
            {
                EncounterZoneAnalyzer.HasEncounterZone
            }
            );
    }
}
