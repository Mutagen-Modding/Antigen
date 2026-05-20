using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class LocationExtensions
{
    public static bool IsLocationAppliedToInterior(this ILocationGetter location, ILinkCache linkCache, ILinkUsageCache usageCache)
    {
        return usageCache.GetUsagesOf<ICellGetter>(location).UsageLinks
            .Select(c => c.Resolve(linkCache))
            .Where(c => c.Location.Equals(location))
            .Any(c => c.IsInteriorCell());
    }

    private static readonly HashSet<IFormLinkGetter<IKeywordGetter>> SettlementKeywords =
    [
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHabitation,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHabitationHasInn,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeFarm,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeCity,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHouse,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeSettlement,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypePlayerHouse,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeInn,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeStore
    ];

    public static bool IsSettlementLocation(this ILocationGetter location)
    {
        if (location.Keywords is null) return false;

        return location.Keywords.Any(k => SettlementKeywords.Contains(k));
    }

    private static readonly HashSet<IFormLinkGetter<IKeywordGetter>> SettlementHouseKeywords =
    [
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHouse,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypePlayerHouse,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeInn,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeStore
    ];

    public static bool IsSettlementHouseLocation(this ILocationGetter location)
    {
        if (location.Keywords is null) return false;

        return location.Keywords.Any(k => SettlementHouseKeywords.Contains(k));
    }

    private static readonly HashSet<IFormLinkGetter<IKeywordGetter>> SettlementHouseNotPlayerHomeKeywords =
    [
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHouse,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeInn,
        FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeStore
    ];

    public static bool IsSettlementHouseLocationNotPlayerHome(this ILocationGetter location)
    {
        if (location.Keywords is null) return false;

        if (location.Keywords.Any(k => k.FormKey == FormKeys.SkyrimSE.Skyrim.Keyword.LocTypePlayerHouse.FormKey))
        {
            return false;
        }

        return location.Keywords.Any(k => SettlementHouseNotPlayerHomeKeywords.Contains(k));
    }

    public static bool IsInnLocation(this ILocationGetter location)
    {
        if (location.Keywords is null) return false;

        return location.Keywords.Any(k => k.FormKey == FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeInn.FormKey);
    }

    public static IEnumerable<ILocationGetter> GetParentLocations(this ILocationGetter location, ILinkCache linkCache, bool includeSelf = false)
    {
        if (includeSelf)
        {
            yield return location;
        }

        var parentLocation = location.ParentLocation.TryResolve(linkCache);
        while (parentLocation is not null)
        {
            yield return parentLocation;
            parentLocation = parentLocation.ParentLocation.TryResolve(linkCache);
        }
    }
}
