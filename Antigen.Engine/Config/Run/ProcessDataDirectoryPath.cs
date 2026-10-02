namespace Antigen.Config.Run;

public class ProcessDataDirectoryPath : IConfigReaderProcessor<IRunConfig>
{
    public bool Process(IRunConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // environment.data_directory = <path>
        if (instructionParts.Count != 2) return false;

        if (instructionParts[0] is not "environment") return false;
        if (instructionParts[1] is not "data_directory") return false;

        config.OverrideDataDirectory(value);

        return true;
    }
}
