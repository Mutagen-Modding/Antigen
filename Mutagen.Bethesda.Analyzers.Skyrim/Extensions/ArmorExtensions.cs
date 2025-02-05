using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ArmorExtensions
{
    public static IEnumerable<BipedObjectFlag> GetSlots(this IArmorGetter armor)
    {
        if (armor.BodyTemplate is null) yield break;

        foreach (var bipedObjectFlag in Enum.GetValues<BipedObjectFlag>()) {
            if (armor.BodyTemplate.FirstPersonFlags.HasFlag(bipedObjectFlag)) yield return bipedObjectFlag;
        }
    }
}
