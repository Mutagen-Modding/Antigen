using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Faction;

public class FactionNotBuySellAnalyzer : IIsolatedRecordAnalyzer<IFactionGetter>
{
    public static readonly TopicDefinition FactionNotBuySellList = MutagenTopicBuilder.FromDiscussion(
            549,
            "NotBuySell set to wrong value",
            Severity.Error)
        .WithoutFormatting("When using this VendorBuySellList the Property NotBuySell needs to be set to true");

    public IEnumerable<TopicDefinition> Topics { get; } = [FactionNotBuySellList];
    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IFactionGetter> param)
    {
        var faction = param.Record;

        if (!faction.IsVendor()) return;

        if (faction.VendorValues is null) return;

        if (faction.VendorBuySellList.FormKey == FormKey.Null) return;

        //build FormKey Array for VendorBuyLists that should have NotSellBuy set to true
        FormKey[] vendorBuySellLists =
        {
            FormKeys.SkyrimSE.Skyrim.FormList.VendorItemsMisc.FormKey,
            FormKeys.SkyrimSE.Skyrim.FormList.VendorItemsMiscLucan.FormKey,
            FormKeys.SkyrimSE.Dragonborn.FormList.DLC2DremoraVendorExclusion.FormKey
        };

        bool relevantFaction = false;
        foreach (FormKey formKey in vendorBuySellLists)
        {
            if (faction.VendorBuySellList.FormKey == formKey)
            {
                relevantFaction = true;
            }
        }

        if (!relevantFaction) return;

        if (faction.VendorValues.NotSellBuy == false)
        {
            param.AddTopic(FactionNotBuySellList.Format());
        }
    }

    public IEnumerable<Func<IFactionGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VendorBuySellList;
    }
}
