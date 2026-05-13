using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ConstructibleExtensions
{
    public static IEnumerable<IConstructibleObjectGetter> GetTemperRecipes(
        this IConstructibleGetter item,
        IFormLink<IKeywordGetter> temperKeyword,
        ILinkUsageCache usageCache,
        ILinkCache linkCache)
    {
        return usageCache
            .GetUsagesOf<IConstructibleObjectGetter>(item).UsageLinks
            .Select(c => c.Resolve(linkCache))
            .Where(c => c.WorkbenchKeyword.Equals(temperKeyword))
            .Where(c => c.CreatedObject.Equals(item));
    }
}
