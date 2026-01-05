using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Testing;

public class TestAnalyzer : IContextualAnalyzer
{
    public IEnumerable<TopicDefinition> Topics { get; }

    public TestAnalyzer(params TopicDefinition[] topics)
    {
        Topics = topics;
    }

    public void Analyze(ContextualAnalyzerParams param)
    {
        foreach (var topic in Topics)
        {
            param.AddTopic(
                ModKey.Null,
                new FormLinkInformation(FormKey.Null, typeof(IMajorRecordGetter)),
                topic.Format());
        }
    }
}
