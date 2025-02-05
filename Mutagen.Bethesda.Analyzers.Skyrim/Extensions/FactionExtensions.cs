using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class FactionExtensions
{
    public static bool IsVendor(this IFactionGetter faction)
    {
        return (faction.Flags & Faction.FactionFlag.Vendor) != 0;
    }

    public static bool IsCrimeFaction(this IFactionGetter faction)
    {
        return (faction.Flags & Faction.FactionFlag.TrackCrime) != 0;
    }
}
