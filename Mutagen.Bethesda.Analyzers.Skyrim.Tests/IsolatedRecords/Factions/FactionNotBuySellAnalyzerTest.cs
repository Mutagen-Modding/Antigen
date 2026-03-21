using Mutagen.Bethesda.Analyzers.Skyrim.Extensions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Faction;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Factions;

public class FactionNotBuySellAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void DetectsFactionsWithIncorrectNotBuySellProperty(
        IsolatedRecordTestFixture<FactionNotBuySellAnalyzer, Faction, IFactionGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.Flags |= Faction.FactionFlag.Vendor;
                rec.VendorBuySellList = FormKeys.SkyrimSE.Skyrim.FormList.VendorItemsMisc.AsNullable();
                rec.VendorValues = new VendorValues();
                rec.VendorValues.NotSellBuy = false;
            },
            prepForFix: rec => {
                rec.Flags |= Faction.FactionFlag.Vendor;
                rec.VendorBuySellList = FormKeys.SkyrimSE.Skyrim.FormList.VendorItemsMisc.AsNullable();
                rec.VendorValues = new VendorValues();
                rec.VendorValues.NotSellBuy = true;
            },
            new[]
            {
                FactionNotBuySellAnalyzer.FactionNotBuySellList
            });
    }

}
