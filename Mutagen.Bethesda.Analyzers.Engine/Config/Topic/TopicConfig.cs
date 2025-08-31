using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Mutagen.Bethesda.Analyzers.Config.Topic;

public interface ITopicConfig : ISeverityLookup
{
    void Override(TopicId id, Severity severity);
}

public interface ISeverityLookup
{
    Severity LookupSeverity(TopicDefinition def);
}

public class TopicConfig(Dictionary<TopicId, Severity>? overrides = null) : ITopicConfig
{
    private readonly Dictionary<TopicId, Severity> _severityOverrides = overrides ?? [];

    public void Override(TopicId id, Severity severity)
    {
        _severityOverrides[id] = severity;
    }

    public Severity LookupSeverity(TopicDefinition def)
    {
        if (!_severityOverrides.TryGetValue(def.Id, out var severityOverride)) return def.Severity;
        return severityOverride;
    }
}
