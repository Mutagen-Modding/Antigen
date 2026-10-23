using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Extensions;

public static class ItemExtensions
{
    extension(IEnumerable<IItemGetter> items)
    {
        public long GetTotalGoldValue()
        {
            return items
                .OfType<IWeightValueGetter>()
                .Sum(item => item.Value);
        }
    }
}
