using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public class TooLongAnalyzer : IIsolatedRecordAnalyzer<IDialogTopicGetter>
{
    private const int DialogPromptLengthLimit = 80;
    public static readonly TopicDefinition<string, int> TopicPromptTooLong = MutagenTopicBuilder.FromDiscussion(
            277,
            "Topic Prompt Too Long",
            Severity.Suggestion)
        .WithFormatting<string, int>("Topic prompt '{0}' is {1} longer than the recommended limit " + DialogPromptLengthLimit);

    public IEnumerable<TopicDefinition> Topics { get; } = [TopicPromptTooLong];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogTopic = param.Record;

        if (dialogTopic.Name?.String is { Length: > DialogPromptLengthLimit })
        {
            param.AddTopic(
                TopicPromptTooLong.Format(dialogTopic.Name.String, dialogTopic.Name.String.Length - DialogPromptLengthLimit));
        }
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
    }
}
