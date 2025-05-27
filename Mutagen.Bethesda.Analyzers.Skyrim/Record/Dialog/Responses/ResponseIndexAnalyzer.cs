using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class ResponseIndexAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<byte> InvalidResponseIndex = MutagenTopicBuilder.FromDiscussion(
            204,
            "Invalid Response Index",
            Severity.Error)
        .WithFormatting<byte>("Response has an invalid index {0}");

    public IEnumerable<TopicDefinition> Topics => [InvalidResponseIndex];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var responses = param.Record;
        if (responses.Responses.Count == 0) return;

        for (var i = 1; i <= responses.Responses.Count; i++)
        {
            var response = responses.Responses[i - 1];

            if (response.ResponseNumber == 0)
            {
                param.AddTopic(InvalidResponseIndex.Format(response.ResponseNumber));
            }
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Responses;
    }
}
