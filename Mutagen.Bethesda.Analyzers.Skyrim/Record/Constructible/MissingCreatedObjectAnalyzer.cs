using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Constructible;

public class MissingCreatedObjectAnalyzer : IIsolatedRecordAnalyzer<IConstructibleObjectGetter>
{
    public static readonly TopicDefinition MissingCreatedObject = MutagenTopicBuilder.FromDiscussion(
            304,
            "Missing Created Object",
            Severity.Warning)
        .WithoutFormatting("Constructible doesn't create any object");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingCreatedObject];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IConstructibleObjectGetter> param)
    {
        if (param.Record.CreatedObject.IsNull)
        {
            param.AddTopic(MissingCreatedObject.Format());
        }
    }

    public IEnumerable<Func<IConstructibleObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.CreatedObject;
    }
}
