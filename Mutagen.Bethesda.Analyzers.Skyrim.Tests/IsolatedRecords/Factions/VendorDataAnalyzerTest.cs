using Mutagen.Bethesda.Analyzers.Skyrim.Record.Faction;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Factions;

public class VendorDataAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void TestRemoveLocationData(
        IsolatedRecordTestFixture<VendorDataAnalyzer, Faction, IFactionGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.VendorLocation = new();
            },
            prepForFix: rec =>
            {
                rec.VendorLocation = null;
            },
            VendorDataAnalyzer.LocationDataWithoutVendor);
    }

    [Theory, MutagenModAutoData]
    public void TestAddVendorFlag(
        IsolatedRecordTestFixture<VendorDataAnalyzer, Faction, IFactionGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.VendorLocation = new();
            },
            prepForFix: rec =>
            {
                rec.Flags |= Faction.FactionFlag.Vendor;
            },
            VendorDataAnalyzer.LocationDataWithoutVendor);
    }
}
