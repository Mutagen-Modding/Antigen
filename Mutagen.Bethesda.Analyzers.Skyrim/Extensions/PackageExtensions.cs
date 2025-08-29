using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class PackageExtensions
{
    public static string? GetPackageDataName(this IPackageGetter package, sbyte key, ILinkCache linkCache)
    {
        var template = package.PackageTemplate.TryResolve(linkCache);
        if (template is not null && template.Data.TryGetValue(key, out var templateData))
        {
            return templateData.Name;
        }

        return null;
    }
}
