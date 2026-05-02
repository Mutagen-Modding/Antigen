using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Util
{
    public static class TemperRecipeAnalyzerUtil
    {
        public static IEnumerable<IConstructibleObjectGetter> GetTemperRecipes(
            IFormLink<IKeywordGetter> temperKeyword,
            ILinkUsageCache usageCache,
            ILinkCache linkCache,
            IConstructibleGetter item)
        {
            return usageCache
                .GetUsagesOf<IConstructibleObjectGetter>(item).UsageLinks
                .Select(c => c.Resolve(linkCache))
                .Where(c => c.WorkbenchKeyword.Equals(temperKeyword))
                .Where(c => c.CreatedObject.Equals(item));
        }
    }
}
