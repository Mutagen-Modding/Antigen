using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class FurnitureExtensions
{
    public static bool IsBed(this IFurnitureGetter furniture)
    {
        // TODO: potentially replace with check on nif file to see if the animation type used on the furniture entry node is sleep instead
        return furniture.EditorID is not null && furniture.EditorID.Contains("bed", StringComparison.OrdinalIgnoreCase);
    }
}
