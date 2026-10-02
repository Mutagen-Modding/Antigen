using Antigen.SDK.Topics;

namespace Antigen;

public static class MutagenTopicBuilder
{
    public const string Prefix = "A";

    public static TopicDefinition FromDiscussion(
        ushort id,
        string title,
        Severity severity)
    {
        return TopicDefinition.FromDiscussion(Prefix, id, title, severity, Constants.AnalyzersDiscussionUrl);
    }

    public static TopicDefinition DevelopmentTopic(
        string title,
        Severity severity)
    {
        var hash = title.GetHashCode();
        var ushortHash = (ushort) ((hash >> 16) ^ hash);

        return new TopicDefinition(
            new TopicId("Dev", ushortHash),
            title,
            severity);
    }
}
