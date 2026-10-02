using Noggog.StructuredStrings;

namespace Antigen.SDK.Topics;

public static class TopicPrinting
{
    public static string ToShortString(this TopicDefinition topic)
    {
        return $"[{topic.Id}][{topic.Severity.ToShortString()}] {topic.Title}";
    }

    public static void Append(this TopicDefinition topic, StructuredStringBuilder sb)
    {
        sb.AppendLine(topic.ToShortString());
        using (sb.IncreaseDepth())
        {
            if (topic.InformationUri is not null)
            {
                sb.AppendLine(topic.InformationUri.ToString());
            }
        }
    }
}
