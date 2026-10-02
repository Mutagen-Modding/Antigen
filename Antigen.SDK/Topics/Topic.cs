namespace Antigen.SDK.Topics;

public record Topic
{
    public static Topic Create(
        IFormattedTopicDefinition formattedTopicDefinition,
        Type? analyzerType,
        (string Name, object Value)[] metaData)
    {
        return new Topic
        {
            FormattedTopic = formattedTopicDefinition,
            Severity = formattedTopicDefinition.TopicDefinition.Severity,
            MetaData = metaData,
            AnalyzerType = analyzerType
        };
    }

    public TopicDefinition TopicDefinition => FormattedTopic.TopicDefinition;
    public required IFormattedTopicDefinition FormattedTopic { get; init; }
    public required Severity Severity { get; init; }
    public required Type? AnalyzerType { get; init; }
    public required (string Name, object Value)[] MetaData { get; init; }
}
