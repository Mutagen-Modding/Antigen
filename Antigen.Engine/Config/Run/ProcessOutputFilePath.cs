namespace Antigen.Config.Run;

public class ProcessOutputFilePath : IConfigReaderProcessor<IRunConfig>
{
    public bool Process(IRunConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // output_file = <path>
        if (instructionParts.Count != 1) return false;

        if (instructionParts[0] is not "output_file") return false;

        config.OverrideOutputFilePath(value);

        return true;
    }
}
