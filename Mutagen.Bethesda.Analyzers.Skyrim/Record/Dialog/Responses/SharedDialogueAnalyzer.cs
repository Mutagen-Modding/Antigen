using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class SharedDialogueAnalyzer : IContextualRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition ScriptInSharedDialogue = MutagenTopicBuilder.FromDiscussion(
            203,
            "Script In Shared Dialogue",
            Severity.Warning)
        .WithoutFormatting("Shared dialogue cannot not have a script attached");

    public static readonly TopicDefinition UnusedSharedDialogue = MutagenTopicBuilder.FromDiscussion(
            272,
            "Unused Shared Dialogue",
            Severity.Suggestion)
        .WithoutFormatting("Shared dialogue is not used in any topic");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnusedSharedDialogue, ScriptInSharedDialogue];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var responses = param.Record;

        var responsesContext = param.LinkCache.ResolveSimpleContext(responses);
        if (responsesContext.Parent?.Record is not IDialogTopicGetter topic) return;
        if (topic.SubtypeName != "IDAT") return;

        if (responses.VirtualMachineAdapter is not null)
        {
            param.AddTopic(
                ScriptInSharedDialogue.Format());
        }

        // TODO: add when there is a reference cache - this is too slow
        // var isNotUsed = param.LinkCache.PriorityOrder
        //     .SelectMany(x => x.EnumerateMajorRecords<IDialogResponsesGetter>())
        //     .Where(r => !r.ResponseData.IsNull)
        //     .All(r => r.ResponseData.FormKey != responses.FormKey);
        //
        // if (isNotUsed)
        // {
        //     param.AddTopic(
        //         UnusedSharedDialogue.Format());
        // }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
    }
}
