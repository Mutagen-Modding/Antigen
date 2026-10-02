namespace Antigen.SDK.Topics;

public partial record TopicDefinition
{
    public TopicDefinition WithoutFormatting(string messageFormat)
    {
        return new TopicDefinition(
            id: Id,
            title: Title,
            messageFormat: messageFormat,
            severity: Severity,
            informationUri: InformationUri);
    }

    public TopicDefinition<T1> WithFormatting<T1>(string messageFormat)
    {
        return new TopicDefinition<T1>(
            id: Id,
            title: Title,
            messageFormat: messageFormat,
            severity: Severity,
            informationUri: InformationUri);
    }

    public TopicDefinition<T1, T2> WithFormatting<T1, T2>(string messageFormat)
    {
        return new TopicDefinition<T1, T2>(
            id: Id,
            title: Title,
            messageFormat: messageFormat,
            severity: Severity,
            informationUri: InformationUri);
    }

    public TopicDefinition<T1, T2, T3> WithFormatting<T1, T2, T3>(string messageFormat)
    {
        return new TopicDefinition<T1, T2, T3>(
            id: Id,
            title: Title,
            messageFormat: messageFormat,
            severity: Severity,
            informationUri: InformationUri);
    }

    public TopicDefinition<T1, T2, T3, T4> WithFormatting<T1, T2, T3, T4>(string messageFormat)
    {
        return new TopicDefinition<T1, T2, T3, T4>(
            id: Id,
            title: Title,
            messageFormat: messageFormat,
            severity: Severity,
            informationUri: InformationUri);
    }
}
