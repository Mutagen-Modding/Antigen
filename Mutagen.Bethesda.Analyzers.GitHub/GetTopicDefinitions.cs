using System.Text.RegularExpressions;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Mutagen.Bethesda.Analyzers.GitHub;

public partial class GetTopicDefinitions(IAnalyzer[] analyzers)
{
    [GeneratedRegex("""
        MutagenTopicBuilder\.DevelopmentTopic\([\s\r\n]*"(.+)",[\s\r\n]*Severity\.(\w+)\)[\s\r\n]*.+\("(.+)"
        """)]
    public static partial Regex TopicRegex { get; }

    public IEnumerable<TopicDefinition> FromDirectory(string directoryPath)
    {
        var files = Directory.EnumerateFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            foreach (var topic in FromFile(file))
            {
                yield return topic;
            }
        }
    }

    public IEnumerable<TopicDefinition> FromFile(string filePath)
    {
        var text = File.ReadAllText(filePath);
        foreach (var match in TopicRegex.Matches(text).AsEnumerable())
        {
            if (match.Success)
            {
                var topicName = match.Groups[1].Value;
                var severityStr = match.Groups[2].Value;
                var topicDescription = match.Groups[3].Value;
                var severity = Enum.TryParse<Severity>(severityStr, out var s) ? s : throw new InvalidOperationException();
                yield return MutagenTopicBuilder.DevelopmentTopic(topicName, severity)
                    .WithoutFormatting(topicDescription);
            }
        }
    }

    public IEnumerable<TopicDefinition> FromRegistered()
    {
        return analyzers
            .SelectMany(a => a.Topics)
            .Where(x => x.InformationUri is null);
    }
}
