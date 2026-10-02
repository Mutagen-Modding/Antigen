using Microsoft.Extensions.Logging;
using Antigen.SDK.Topics;

namespace Antigen.Config.Topic;

public class ProcessSeverity(ILogger<ProcessSeverity> logger) : IConfigReaderProcessor<ITopicConfig>
{
    public bool Process(ITopicConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // diagnostic.<TopicId>.severity = <Severity>
        if (instructionParts.Count != 3) return false;

        if (instructionParts[0] is not "diagnostic") return false;

        TopicId id;
        try
        {
            id = TopicId.Parse(instructionParts[1]);
        }
        catch (ArgumentException e)
        {
            logger.LogError(e, "Error parsing TopicId");
            return false;
        }

        if (instructionParts[2] is not "severity") return false;

        if (!Enum.TryParse<Severity>(value, out var severity))
        {
            logger.LogError("Error parsing Severity");
            return false;
        }

        config.Override(id, severity);
        return true;
    }
}
