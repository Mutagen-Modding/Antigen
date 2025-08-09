using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Container;

public class MerchantContainerAnalyzer : IContextualRecordAnalyzer<IContainerGetter>
{
    public static readonly TopicDefinition MerchantContainerWithoutVendorGold = MutagenTopicBuilder.FromDiscussion(
            455,
            "Merchant Container without Vendor Gold",
            Severity.Error)
        .WithoutFormatting("Merchant Container has no vendor gold");

    public static readonly TopicDefinition MerchantContainerWithoutPerkMasterTrader = MutagenTopicBuilder.FromDiscussion(
            456,
            "Merchant Container without Master Trader Perk Gold",
            Severity.Error)
        .WithoutFormatting("Merchant Container has no gold for the Master Trader perk");

    public IEnumerable<TopicDefinition> Topics { get; } = [MerchantContainerWithoutVendorGold, MerchantContainerWithoutPerkMasterTrader];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IContainerGetter> param)
    {
        var container = param.Record;
        if (container.IsDeleted) return;

        if (container.EditorID is not {} editorID) return;
        if (!editorID.Contains("Merchant")) return;

        if (container.Items is null)
        {
            param.AddTopic(
                MerchantContainerWithoutVendorGold.Format());
            param.AddTopic(
                MerchantContainerWithoutPerkMasterTrader.Format());
            return;
        }

        var items = container.Items
            .Select(i => i.Item.Item.TryResolve(param.LinkCache))
            .WhereNotNull()
            .ToArray();

        var vendorGold = items.FirstOrDefault(i => i.EditorID is {} id && id.Contains("VendorGold"));
        var gold = items.FirstOrDefault(i => i.FormKey == FormKeys.SkyrimSE.Skyrim.MiscItem.Gold001.FormKey);
        if (vendorGold is null && gold is null)
        {
            param.AddTopic(
                MerchantContainerWithoutVendorGold.Format());
        }

        var perkMasterTrader = items.FirstOrDefault(i => i.FormKey == FormKeys.SkyrimSE.Skyrim.LeveledItem.PerkMasterTraderGold.FormKey);
        if (perkMasterTrader is null)
        {
            param.AddTopic(
                MerchantContainerWithoutPerkMasterTrader.Format());
        }
    }

    public IEnumerable<Func<IContainerGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.Items;
    }
}
