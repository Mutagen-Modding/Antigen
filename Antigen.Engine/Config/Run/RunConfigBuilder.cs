using System.IO.Abstractions;
using Noggog;

namespace Antigen.Config.Run;

public class RunConfigBuilder(
    IFileSystem fileSystem,
    ConfigDirectoryProvider configDirectoryProvider,
    ConfigReader<IRunConfig> reader)
{
    public const string AnalyzerFileName = ".runconfig";

    public IRunConfigLookup Build()
    {
        var config = new RunConfig();

        foreach (var configDirectory in configDirectoryProvider.ConfigDirectories)
        {
            LoadIn(Path.Combine(configDirectory.Path, AnalyzerFileName), config);
        }

        return config;
    }

    private void LoadIn(FilePath path, RunConfig config)
    {
        if (!path.CheckExists(fileSystem)) return;

        reader.ReadInto(path, config);
    }
}
