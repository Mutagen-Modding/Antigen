using Microsoft.Extensions.Logging;
using Noggog;

namespace Antigen.Config;

public class ConfigReader<TConfig>(
    ILogger<ConfigReader<TConfig>> logger,
    IConfigReaderProcessor<TConfig>[] processors)
    : IConfigReader<TConfig>
    where TConfig : class
{
    public const string SettingEqualString = " = ";

    public void ReadInto(FilePath path, TConfig config)
    {
        foreach (var line in File.ReadLines(path))
        {
            var span = line.AsSpan();
            ReadInto(span, config);
        }
    }

    public void ReadInto(ReadOnlySpan<char> line, TConfig config)
    {
        // Remove comments
        var index = line.IndexOf("#");
        if (index != -1)
        {
            line = line.Slice(0, index);
        }

        // Remove empty lines
        if (line.IsWhiteSpace()) return;

        // Separate instruction and its assigned value
        Span<Range> ranges = stackalloc Range[2];
        try
        {
            var split = line.Split(ranges, SettingEqualString);

            if (split != 2)
            {
                logger.LogError("Malformed line: {Line}", line.ToString());
                return;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Malformed line: {Line}", line.ToString());
            return;
        }

        var instruction = line[ranges[0]];
        var instructionParts = instruction.Split('.');
        var instructionPartStrings = new List<string>();
        foreach (var (subStr, _) in instructionParts)
        {
            instructionPartStrings.Add(subStr.ToString());
        }

        var value = line[ranges[1]].Trim().ToString();

        // Pass result into processors
        foreach (var processor in processors)
        {
            if (processor.Process(config, instructionPartStrings, value)) return;
        }
    }
}
