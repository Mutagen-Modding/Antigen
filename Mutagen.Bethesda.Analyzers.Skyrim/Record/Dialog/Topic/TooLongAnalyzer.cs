using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public class TooLongAnalyzer : IIsolatedRecordAnalyzer<IDialogTopicGetter>
{
    private const int DialogPromptLengthLimit = 80;
    public static readonly TopicDefinition<string, Language, int> TopicPromptTooLong = MutagenTopicBuilder.FromDiscussion(
            277,
            "Topic Prompt Too Long",
            Severity.Suggestion)
        .WithFormatting<string, Language, int>("Topic prompt '{0}' in {1} is {2} longer than the recommended limit " + DialogPromptLengthLimit);

    public IEnumerable<TopicDefinition> Topics { get; } = [TopicPromptTooLong];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogTopic = param.Record;
        if (dialogTopic.Name is null) return;

        foreach (var (language, name) in dialogTopic.Name)
        {
            if (name.Length > DialogPromptLengthLimit)
            {
                param.AddTopic(
                    TopicPromptTooLong.Format(name, language, name.Length - DialogPromptLengthLimit));
            }
        }
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
    }
}
