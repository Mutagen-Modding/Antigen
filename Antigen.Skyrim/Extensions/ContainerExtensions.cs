using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Extensions;

public static class ContainerExtensions
{
    extension(IContainerGetter container)
    {
        public IEnumerable<IItemGetter> SampleItems(ILinkCache linkCache, int level, bool useChanceNone, Random random)
        {
            if (container.Items is null) return [];

            return container.Items
                .SelectMany(entry =>
                {
                    return entry.Item.Item.TryResolve(linkCache) switch
                    {
                        ILeveledItemGetter leveledItem => leveledItem.SampleItems(linkCache, level, useChanceNone, random),
                        {} item => Enumerable.Repeat(item, entry.Item.Count),
                        _ => []
                    };
                })
                .WhereNotNull();
        }

        public long GetAverageGoldValue(ILinkCache linkCache, int level)
        {
            if (container.Items is null) return 0;

            return container.Items
                .Select(entry =>
                {
                    return entry.Item.Item.TryResolve(linkCache) switch
                    {
                        ILeveledItemGetter leveledItem => leveledItem.GetAverageGoldValue(linkCache, level),
                        {} item => ((item as IWeightValueGetter)?.Value ?? 0) * entry.Item.Count,
                        _ => 0
                    };
                })
                .Sum();
        }
    }
}
