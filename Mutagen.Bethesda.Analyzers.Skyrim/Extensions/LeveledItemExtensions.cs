using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class LeveledItemExtensions
{
    public static IEnumerable<T> GetItems<T>(this ILeveledItemGetter leveledItem, ILinkCache linkCache)
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
}
