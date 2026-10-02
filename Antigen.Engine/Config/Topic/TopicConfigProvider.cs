using Antigen.SDK.Topics;

namespace Antigen.Config.Topic;

public class TopicConfigProvider : ISeverityLookup
{
    private readonly Lazy<ITopicConfig> _config;
    public ITopicConfig Config => _config.Value;

    public TopicConfigProvider(TopicConfigBuilder builder)
    {
        _config = new Lazy<ITopicConfig>(builder.Build);
    }

    public Severity LookupSeverity(TopicDefinition def)
    {
        return Config.LookupSeverity(def);
    }
}
