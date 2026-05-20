using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
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

        if (!param.LinkCache.TryResolveSimpleContext(responses, out var responsesContext)) return;
        if (responsesContext.Parent?.Record is not IDialogTopicGetter topic) return;
        if (topic.SubtypeName != "IDAT") return;

        if (responses.VirtualMachineAdapter is not null)
        {
            param.AddTopic(
                ScriptInSharedDialogue.Format());
        }

        bool isUsed = param.ResolveCache<ILinkUsageCache>()
            .GetUsagesOf<IDialogResponsesGetter>(responses).UsageLinks
            .Select(r => r.Resolve(param.LinkCache))
            .Any(r => r.ResponseData.Equals(responses));

        if (!isUsed)
        {
            param.AddTopic(
                UnusedSharedDialogue.Format());
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
    }
}
