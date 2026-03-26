using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Faction;

public class FactionNotBuySellAnalyzer : IIsolatedRecordAnalyzer<IFactionGetter>
{
    //build FormKey HashSet for VendorBuyLists that should have NotSellBuy set to true
    private static readonly HashSet<FormKey> VendorBuySellLists =
    [
        FormKeys.SkyrimSE.Skyrim.FormList.VendorItemsMisc.FormKey,
        FormKeys.SkyrimSE.Skyrim.FormList.VendorItemsMiscLucan.FormKey,
        FormKeys.SkyrimSE.Dragonborn.FormList.DLC2DremoraVendorExclusion.FormKey
    ];

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

        if (!VendorBuySellLists.Contains(faction.VendorBuySellList.FormKey)) return;

        if (!faction.VendorValues.NotSellBuy)
        {
            param.AddTopic(FactionNotBuySellList.Format());
        }
    }

    public IEnumerable<Func<IFactionGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VendorBuySellList;
    }
}
