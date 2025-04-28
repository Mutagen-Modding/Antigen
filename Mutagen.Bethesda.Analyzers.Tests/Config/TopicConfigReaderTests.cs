using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Config;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Config;

[method: SuppressMessage("ReSharper", "ContextualLoggerProblem", Justification = "Passed in")]
public class TopicConfigReader(
    ILogger<ConfigReader<ITopicConfig>> logger,
    ProcessSeverity p1)
{
    public readonly ConfigReader<ITopicConfig> Reader = new(logger, [p1]);
}

public class TopicConfigReaderTests
{
    private readonly TopicDefinition _topicDefinition = new(new TopicId("A", 123), "", Severity.None);

    [Theory]
    [AnalyzerInlineData("diagnostic.A123.severity = Warning")]
    [AnalyzerInlineData("diagnostic.A123.severity = Warning # A Comment")]
    public void TestSeverity(
        string line,
        TopicConfig config,
        TopicConfigReader sut)
    {
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.LookupSeverity(_topicDefinition).ShouldBe(Severity.Warning);
    }

    [Theory]
    [AnalyzerInlineData("diagnostic.A123.severity = Warning And Gibberish")]
    [AnalyzerInlineData("other.A123.severity = Warning")]
    [AnalyzerInlineData("diagnostic.1A123.severity = Warning")]
    [AnalyzerInlineData("diagnostic.A123")]
    [AnalyzerInlineData("diagnostic.A123.other = Warning")]
    [AnalyzerInlineData("diagnostic.A123.severity = Other")]
    [AnalyzerInlineData("diagnostic.A123.severity")]
    [AnalyzerInlineData("")]
    [AnalyzerInlineData("#diagnostic.A123.severity = Warning")]
    public void AbnormalLinesDontOverride(
        string line,
        TopicConfig config,
        TopicConfigReader sut)
    {
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.LookupSeverity(_topicDefinition).ShouldBe(Severity.None);
    }
}
