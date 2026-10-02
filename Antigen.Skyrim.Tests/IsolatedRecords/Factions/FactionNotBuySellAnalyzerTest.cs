using Antigen.Skyrim.Extensions;
using Antigen.Skyrim.Record.Faction;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.IsolatedRecords.Factions;

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
