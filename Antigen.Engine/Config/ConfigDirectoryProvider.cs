using Mutagen.Bethesda.Environments.DI;
using Noggog;
using Noggog.IO;

namespace Antigen.Config;

public class ConfigDirectoryProvider(
    IDataDirectoryProvider dataDirectoryProvider,
    ICurrentDirectoryProvider currentDirectoryProvider)
{
    public IEnumerable<DirectoryPath> ConfigDirectories
    {
        get
        {
            yield return currentDirectoryProvider.CurrentDirectory;

            DirectoryPath? dataDirectory;
            try
            {
                dataDirectory = dataDirectoryProvider.Path;
            }
            catch (Exception)
            {
                dataDirectory = null;
            }

            if (dataDirectory.HasValue)
            {
                yield return dataDirectory.Value;
            }
        }
    }
}
