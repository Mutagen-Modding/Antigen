using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Testing;

public class TestContextualRecordAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition HasHeight = MutagenTopicBuilder.DevelopmentTopic(
            "Has Height",
            Severity.Warning)
        .WithoutFormatting("Test analyzer is angry the NPC has a height");

    public IEnumerable<TopicDefinition> Topics => [HasHeight];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        if (param.Record.Height > 0)
        {
            param.AddTopic(HasHeight.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Height;
    }
}
