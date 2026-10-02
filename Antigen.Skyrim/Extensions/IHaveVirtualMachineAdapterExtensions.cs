using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Extensions;

public static class IHaveVirtualMachineAdapterExtensions
{
    public static IScriptEntryGetter? GetScript(this IHaveVirtualMachineAdapterGetter adapterContainer, string name)
    {
        return adapterContainer.VirtualMachineAdapter?.Scripts.FirstOrDefault(script => string.Equals(script.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasScript(this IHaveVirtualMachineAdapterGetter npc, string name)
    {
        return npc.GetScript(name) is not null;
    }
}
