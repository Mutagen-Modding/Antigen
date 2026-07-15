using Mutagen.Bethesda.Plugins;

namespace Antigen.Extensions;

public static class ModTypeExtension
{
    public const string MasterFileExtension = ".esm";
    public const string PluginFileExtension = ".esp";
    public const string LightPluginFileExtension = ".esl";

    public static ModType FromFileExtension(string name)
    {
        return name switch
        {
            MasterFileExtension => ModType.Master,
            LightPluginFileExtension => ModType.Light,
            PluginFileExtension => ModType.Plugin,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }

    extension(ModType modType)
    {
        public string ToFileExtension()
        {
            return modType switch
            {
                ModType.Master => MasterFileExtension,
                ModType.Light => LightPluginFileExtension,
                ModType.Plugin => PluginFileExtension,
                _ => throw new ArgumentOutOfRangeException(nameof(modType), modType, null)
            };
        }
    }
}