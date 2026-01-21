using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ArmorExtensions
{
    public static bool IsSkin(this IArmorGetter armor)
    {
        return armor.EditorID is not null && armor.EditorID.Contains("Skin", StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<BipedObjectFlag> GetSlots(this IArmorGetter armor)
    {
        if (armor.BodyTemplate is null) return [];

        return armor.BodyTemplate.GetSlots();
    }

    public static IEnumerable<BipedObjectFlag> GetSlots(this IBodyTemplateGetter bodyTemplate)
    {
        foreach (var bipedObjectFlag in Enum.GetValues<BipedObjectFlag>())
        {
            if (bodyTemplate.FirstPersonFlags.HasFlag(bipedObjectFlag)) yield return bipedObjectFlag;
        }
    }
}
