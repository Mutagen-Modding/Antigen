using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class PlacementExtension
{
    public static P2Int GetCellCoordinates(this IPlacementGetter placement)
    {
        const int cellLength = 4096;
        var position = placement.Position;

        return new P2Int(ToInt(position.X), ToInt(position.Y));

        int ToInt(float pos) => (int)Math.Floor(pos / cellLength);
    }
}
