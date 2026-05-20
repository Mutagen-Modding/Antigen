using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior.Settlement;

public class NoMerchantChestLocRefTypeAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedObjectGetter> NoMerchantChestLocRefType = MutagenTopicBuilder.FromDiscussion(
            296,
            "No Merchant Chest Location Reference Type",
            Severity.Suggestion)
        .WithFormatting<IPlacedObjectGetter>("{0} is a merchant chest and should have MerchantContainerRefType location reference type");

    public static readonly TopicDefinition<IPlacedObjectGetter> InvalidMerchantChestLocRefType = MutagenTopicBuilder.FromDiscussion(
            355,
            "Invalid Merchant Chest Location Reference Type",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter>("{0} is not a merchant chest and should not have MerchantContainerRefType location reference type");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoMerchantChestLocRefType, InvalidMerchantChestLocRefType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        // Skip non-settlement cells
        if (!cell.IsSettlementCell(param.LinkCache)) return;

        foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
        {
            if (placedObject.IsDeleted) continue;

            var isMerchantChest = param.ResolveCache<ILinkUsageCache>()
                .GetUsagesOf<IFactionGetter>(placedObject).UsageLinks
                .Select(f => f.Resolve(param.LinkCache))
                .Any(f => f.MerchantContainer.Equals(placedObject));
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
