using Antigen.SDK.Topics;

namespace Antigen.Config.Topic;

public interface IMinimumSeverityConfiguration
{
    Severity MinimumSeverity { get; }
}

public class MinimumSeverityConfiguration(Severity minimumSeverity) : IMinimumSeverityConfiguration
{
    public Severity MinimumSeverity { get; } = minimumSeverity;
}
