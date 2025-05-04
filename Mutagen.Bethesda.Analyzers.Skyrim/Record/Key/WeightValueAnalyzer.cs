using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Key;

public class WeightValueAnalyzer : IIsolatedRecordAnalyzer<IKeyGetter>
{
    public static readonly TopicDefinition<float> WeightNotZero = MutagenTopicBuilder.DevelopmentTopic(
            "Weight Not Zero",
            Severity.Suggestion)
        .WithFormatting<float>("Key weight {0} is not zero");

    public static readonly TopicDefinition<uint> ValueNotZero = MutagenTopicBuilder.DevelopmentTopic(
            "Value Not Zero",
            Severity.Suggestion)
        .WithFormatting<uint>("Key value {0} is not zero");

    public IEnumerable<TopicDefinition> Topics { get; } = [WeightNotZero, ValueNotZero];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IKeyGetter> param)
    {
        var key = param.Record;
        if (key.IsDeleted) return;

        if (key is not { Weight: 0 })
        {
            param.AddTopic(
                WeightNotZero.Format(key.Weight));
        }

        if (key.Value != 0)
        {
            param.AddTopic(
                ValueNotZero.Format(key.Value));
        }
    }

    public IEnumerable<Func<IKeyGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Weight;
        yield return x => x.Value;
    }
}
