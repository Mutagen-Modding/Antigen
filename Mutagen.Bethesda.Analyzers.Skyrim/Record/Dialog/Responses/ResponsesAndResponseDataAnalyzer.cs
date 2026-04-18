using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class ResponsesAndResponseDataAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition ResponsesAndResponseData = MutagenTopicBuilder.FromDiscussion(
        560,
        "Dialog Responses Contains Shared ResponseData and Responses",
        Severity.Warning
    );

    public IEnumerable<TopicDefinition> Topics { get; } = [ResponsesAndResponseData];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        if (param.Record.Responses.Count > 0 && !param.Record.ResponseData.IsNull)
        {
            param.AddTopic(ResponsesAndResponseData.Format());
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Responses;
        yield return x => x.ResponseData;
    }
}
