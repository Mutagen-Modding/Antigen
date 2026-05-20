using Mutagen.Bethesda.Analyzers.Skyrim.Record.Location;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Locations;

using Fixture = ContextualRecordTestFixture<RefTypeSettlementHouseAnalyzer, Location, ILocationGetter>;

public class RefTypeSettlementHouseAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void NoHouseContainer(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHouse];

                var cell = fixture.Create<Cell>();
                cell.Flags |= Cell.Flag.IsInteriorCell;
                mod.Cells.AddInteriorCell(cell);
                cell.Location.SetTo(rec);
            },
            prepForFix: (rec, mod) =>
            {
                rec.LocationRefTypeReferencesAdded =[new()
                {
                    LocationRefType = FormKeys.SkyrimSE.Skyrim.LocationReferenceType.HouseContainerRefType
                }];
            },
            RefTypeSettlementHouseAnalyzer.NoHouseContainerRefType);
    }
}
