using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

public class InvalidConditionReferenceAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition<string?> InvalidConditionReference = MutagenTopicBuilder.FromDiscussion(
            213,
            "Condition Runs on Null Reference",
            Severity.Error)
        .WithFormatting<string?>("Condition {0} runs on reference, but reference is null");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidConditionReference];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        var data = context.Condition.Data;
        if (data is not { RunOnType: Condition.RunOnType.Reference, Reference.IsNull: true }) return;

        context.Param.AddTopic(InvalidConditionReference.Format(data.Function.ToString()));
    }
}
