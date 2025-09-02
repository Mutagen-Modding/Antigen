using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Mutagen.Bethesda.Analyzers.Config.Topic;

public interface IMinimumSeverityConfiguration
{
    Severity MinimumSeverity { get; }
}

public class MinimumSeverityConfiguration(Severity minimumSeverity) : IMinimumSeverityConfiguration
{
    public Severity MinimumSeverity { get; } = minimumSeverity;
}
