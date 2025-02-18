using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class IHaveVirtualMachineAdapterExtensions
{
    public static IScriptEntryGetter? GetScript(this IHaveVirtualMachineAdapterGetter npc, string name)
    {
        return npc.VirtualMachineAdapter?.Scripts.FirstOrDefault(script => string.Equals(script.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasScript(this IHaveVirtualMachineAdapterGetter npc, string name)
    {
        return npc.GetScript(name) is not null;
    }
}
