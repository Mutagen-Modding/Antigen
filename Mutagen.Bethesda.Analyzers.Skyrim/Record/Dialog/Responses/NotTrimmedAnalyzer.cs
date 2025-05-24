using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class NotTrimmedAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<string> PromptNotTrimmed = MutagenTopicBuilder.FromDiscussion(
            270,
            "Prompt Not Trimmed",
            Severity.Suggestion)
        .WithFormatting<string>("Prompt '{0}' is not trimmed");

    public static readonly TopicDefinition<string> ResponseNotTrimmed = MutagenTopicBuilder.FromDiscussion(
            339,
            "Response Not Trimmed",
            Severity.Suggestion)
        .WithFormatting<string>("Response '{0}' is not trimmed");

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptNotTrimmed, ResponseNotTrimmed];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Prompt?.String is not null && NotTrimmed(dialogResponses.Prompt.String))
        {
            param.AddTopic(
                PromptNotTrimmed.Format(dialogResponses.Prompt.String));
        }

        // Check responses
        foreach (var response in dialogResponses.Responses.Select(dialogResponse => dialogResponse.Text.String)
                     .WhereNotNull()
                     .Where(text => !text.IsNullOrWhitespace())
                     .Where(NotTrimmed)
                )
        {
            param.AddTopic(
                ResponseNotTrimmed.Format(response));
        }

        static bool NotTrimmed(string text) => text.StartsWith(' ') || text.EndsWith(' ');
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Prompt;
        yield return x => x.Responses;
    }
}
