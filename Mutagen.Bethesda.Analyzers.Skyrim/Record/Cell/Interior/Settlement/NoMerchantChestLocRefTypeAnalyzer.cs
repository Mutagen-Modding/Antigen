using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior.Settlement;

public class NoMerchantChestLocRefTypeAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedObjectGetter> NoMerchantChestLocRefType = MutagenTopicBuilder.DevelopmentTopic(
            "No Bed Location Reference Type",
            Severity.Suggestion)
        .WithFormatting<IPlacedObjectGetter>("{0} is a merchant chest and should have MerchantContainerRefType location reference type");

    public static readonly TopicDefinition<IPlacedObjectGetter> InvalidMerchantChestLocRefType = MutagenTopicBuilder.DevelopmentTopic(
            "Invalid Merchant Chest Location Reference Type",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter>("{0} is not a merchant chest and should not have MerchantContainerRefType location reference type");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoMerchantChestLocRefType, InvalidMerchantChestLocRefType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        var merchantChests = param.LinkCache.PriorityOrder.WinningOverrides<IFactionGetter>()
            .Where(faction => faction.Flags.HasFlag(Bethesda.Skyrim.Faction.FactionFlag.Vendor))
            .Select(faction => faction.MerchantContainer.FormKey)
            .ToHashSet();

        // Skip non-settlement cells
        if (!cell.IsSettlementCell(param.LinkCache)) return;

        foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
        {
            if (placedObject.IsDeleted) continue;
            // todo: replace with reference cache
            var isMerchantChest = merchantChests.Contains(placedObject.FormKey);
            var hasLocRefType = placedObject.HasLocationRefType(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.MerchantContainerRefType);

            if (isMerchantChest && !hasLocRefType)
            {
                param.AddTopic(
                    NoMerchantChestLocRefType.Format(placedObject));
            }
            else if (!isMerchantChest && hasLocRefType)
            {
                param.AddTopic(
                    InvalidMerchantChestLocRefType.Format(placedObject));
            }
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Location;
    }
}
