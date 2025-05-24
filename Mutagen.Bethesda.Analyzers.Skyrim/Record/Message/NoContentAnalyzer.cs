using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Message;

public class NoContentAnalyzer : IIsolatedRecordAnalyzer<IMessageGetter>
{
    public static readonly TopicDefinition NoContent = MutagenTopicBuilder.FromDiscussion(
            236,
            "No Content",
            Severity.Suggestion)
        .WithoutFormatting("Message has no content");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoContent];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IMessageGetter> param)
    {
        var message = param.Record;

        if ((message.Name is null || message.Name.String.IsNullOrWhitespace())
            && message.Description.String.IsNullOrWhitespace()
            && message.MenuButtons.Count == 0)
        {
            param.AddTopic(
                NoContent.Format());
        }
    }

    public IEnumerable<Func<IMessageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
        yield return x => x.Description;
        yield return x => x.MenuButtons;
    }
}
