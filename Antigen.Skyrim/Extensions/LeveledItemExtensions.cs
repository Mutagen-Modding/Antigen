using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Extensions;

public static class LeveledItemExtensions
{
    extension(ILeveledItemGetter leveledItem)
    {
        public IEnumerable<T> GetItems<T>(ILinkCache linkCache)
            where T : class, IMajorRecordQueryableGetter
        {
            var items = new List<T>();

            GetNested(leveledItem);

            return items;

            void GetNested(ILeveledItemGetter l)
            {
                if (l.Entries is null) return;

                foreach (var item in l.Entries)
                {
                    if (item.Data is null) continue;

                    if (linkCache.TryResolve<T>(item.Data.Reference.FormKey, out var t))
                    {
                        items.Add(t);
                    }
                    else if (linkCache.TryResolve<ILeveledItemGetter>(item.Data.Reference.FormKey, out var leveled))
                    {
                        GetNested(leveled);
                    }
                }
            }
        }

        /// <summary>
        /// Sample items from the leveled list based on the provided level.
        /// </summary>
        /// <param name="linkCache">The link cache used to resolve form links.</param>
        /// <param name="level">The level at which to sample items from the leveled list.</param>
        /// <param name="useChanceNone">If true, the chance none value will be considered when sampling items.</param>
        /// <param name="random">A random number generator used for sampling.</param>
        /// <returns>An enumerable of item getters representing the sampled items from the leveled list.</returns>
        public IEnumerable<IItemGetter> SampleItems(ILinkCache linkCache, int level, bool useChanceNone, Random random)
        {
            if (leveledItem.Entries is null) yield break;

            // If the leveled list has a chance none value and the random number generator produces a value less than that chance, no items will be returned.
            if (useChanceNone && leveledItem.ChanceNone > 0 && random.NextDouble() < leveledItem.ChanceNone.Value)
            {
                yield break;
            }

            // Use All causes the list to always return all of its contents.
            var useAll = leveledItem.Flags.HasFlag(LeveledItem.Flag.UseAll);

            if (useAll)
            {
                foreach (var entry in leveledItem.Entries)
                {
                    if (entry.Data is null) continue;
                    if (!entry.Data.Reference.TryResolve(linkCache, out var item)) continue;

                    if (item is ILeveledItemGetter nestedLeveledItem)
                    {
                        // If this list is in another leveled list with a count greater than zero, this check box determines if each item in the count is the same, or is recalculated.
                        var recalculateNested = nestedLeveledItem.Flags.HasFlag(LeveledItem.Flag.CalculateForEachItemInCount);

                        if (recalculateNested)
                        {
                            // If the nested leveled item has the CalculateForEachItemInCount flag, we need to sample items for each count in the parent entry.
                            for (var i = 0; i < entry.Data.Count; i++)
                            {
                                foreach (var nestedItem in nestedLeveledItem.SampleItems(linkCache, level, useChanceNone, random))
                                {
                                    yield return nestedItem;
                                }
                            }
                        }
                        else
                        {
                            // If the nested leveled item does not have the CalculateForEachItemInCount flag, we only sample items once and then return the same items for each count in the parent entry.
                            foreach (var nestedItem in nestedLeveledItem.SampleItems(linkCache, level, useChanceNone, random))
                            {
                                for (var i = 0; i < entry.Data.Count; i++)
                                {
                                    yield return nestedItem;
                                }
                            }
                        }
                    }
                    else
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                // If checked, the level list will use any item at or below the player's level in the list.
                // If not checked, it uses the closest item or items to the player's level, but not exceeding it.
                var calculateFromAllLevelsLessThanOrEqualPlayer = leveledItem.Flags.HasFlag(LeveledItem.Flag.CalculateFromAllLevelsLessThanOrEqualPlayer);

                if (calculateFromAllLevelsLessThanOrEqualPlayer)
                {
                    var itemPool = leveledItem.Entries
                        .Select(e => e.Data)
                        .WhereNotNull()
                        .Where(e => e.Level <= level)
                        .Select(GetEntryItems)
                        .WhereNotNull()
                        .ToArray();

                    if (itemPool.Length > 0)
                    {
                        var randomIndex = random.Next(0, itemPool.Length);
                        foreach (var item in itemPool[randomIndex])
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    // Find the closest level to the player's level without exceeding it.
                    var closestItem = leveledItem.Entries
                        .Where(e => e.Data is not null && e.Data.Level <= level)
                        .OrderByDescending(e => e.Data!.Level)
                        .Select(e => e.Data)
                        .WhereNotNull()
                        .Select(GetEntryItems)
                        .WhereNotNull()
                        .FirstOrDefault();

                    if (closestItem is not null)
                    {
                        foreach (var item in closestItem)
                        {
                            yield return item;
                        }
                    }
                }

                IEnumerable<IItemGetter>? GetEntryItems(ILeveledItemEntryDataGetter e)
                {
                    if (!e.Reference.TryResolve(linkCache, out var item)) return null;

                    if (item is ILeveledItemGetter nestedLeveledItem)
                    {
                        var items = nestedLeveledItem.SampleItems(linkCache, level, useChanceNone, random).ToArray();
                        return Enumerable.Repeat(items, e.Count).SelectMany(x => x);
                    }

                    return Enumerable.Repeat(item, e.Count);
                }
            }
        }

        /// <summary>
        /// Sample items from the leveled list based on the provided level.
        /// </summary>
        /// <param name="linkCache">The link cache used to resolve form links.</param>
        /// <param name="level">The level at which to sample items from the leveled list.</param>
        /// <returns>An enumerable of item getters representing the sampled items from the leveled list.</returns>
        public long GetAverageGoldValue(ILinkCache linkCache, int level)
        {
            if (leveledItem.Entries is null) return 0;

            // If a global is used for chance none
            if (!leveledItem.Global.IsNull) return 0;

            // Use the chance none value to reduce the average gold value of the leveled list.
            var fraction = 1 - leveledItem.ChanceNone.Value;

            // Use All causes the list to always return all of its contents.
            var useAll = leveledItem.Flags.HasFlag(LeveledItem.Flag.UseAll);
            if (useAll) return (long) (fraction * leveledItem.Entries.Sum(GetEntryValue));

            // If checked, the level list will use any item at or below the player's level in the list.
            // If not checked, it uses the closest item or items to the player's level, but not exceeding it.
            var calculateFromAllLevelsLessThanOrEqualPlayer = leveledItem.Flags.HasFlag(LeveledItem.Flag.CalculateFromAllLevelsLessThanOrEqualPlayer);
            if (calculateFromAllLevelsLessThanOrEqualPlayer)
            {
                var itemPool = leveledItem.Entries
                    .Where(e => e.Data?.Level <= level)
                    .Select(GetEntryValue)
                    .ToArray();

                if (itemPool.Length == 0) return 0;

                return (long) (fraction * itemPool.Average().Round());
            }

            // Find the closest level to the player's level without exceeding it.
            return (long) (fraction * leveledItem.Entries
                .Where(e => e.Data is not null && e.Data.Level <= level)
                .OrderByDescending(e => e.Data!.Level)
                .Select(GetEntryValue)
                .FirstOrDefault());

            long GetEntryValue(ILeveledItemEntryGetter? e)
            {
                if (e?.Data is null || !e.Data.Reference.TryResolve(linkCache, out var item)) return 0;

                var value = item switch
                {
                    ILeveledItemGetter nestedLeveledItem => nestedLeveledItem.GetAverageGoldValue(linkCache, level),
                    IWeightValueGetter weightValueItem => weightValueItem.Value,
                    _ => 0
                };

                return (long) (fraction * value * e.Data.Count);
            }
        }
    }
}
