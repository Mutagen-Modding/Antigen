using Antigen.Skyrim.Record.Cell.Exterior;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.IsolatedRecords.Cells.Exterior;

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
                rec.EncounterZone.SetToNull();
            },
            new[]
            {
                EncounterZoneAnalyzer.HasEncounterZone
            }
            );
    }
}
