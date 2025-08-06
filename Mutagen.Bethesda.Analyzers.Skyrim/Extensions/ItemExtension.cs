using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ItemExtension
{
    public static T? FindItem<T>(this IItemGetter item, ILinkCache linkCache, Func<T, bool> predicate)
        where T : class, IItemGetter
    {
        switch (item)
        {
            case ILeveledItemGetter leveledItem:
            {
                if (leveledItem.Entries is null || leveledItem.Entries.Count == 0) return null;

                foreach (var entry in leveledItem.Entries)
                {
                    var entryItem = entry.Data?.Reference.TryResolve(linkCache);
                    if (entryItem is null) continue;

                    var foundItem = FindItem(entryItem, linkCache, predicate);
                    if (foundItem is not null)
                    {
                        return foundItem;
                    }
                }

                return null;
            }
            default:
            {
                if (item is T t && predicate(t))
                {
                    return t;
                }

                return null;
            }
        }
    }
}
