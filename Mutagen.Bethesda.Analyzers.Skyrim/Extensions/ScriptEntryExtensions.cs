using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ScriptEntryExtensions
{
    public static T? GetProperty<T>(this IScriptEntryGetter scriptEntry, string name)
        where T : IScriptPropertyGetter
    {
        return scriptEntry.Properties
            .OfType<T>()
            .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasProperty<T>(this IScriptEntryGetter scriptEntry, string name)
        where T : IScriptPropertyGetter
    {
        return scriptEntry.GetProperty<T>(name) is not null;
    }
}
