using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public class MissingPromptAnalyzer : IContextualRecordAnalyzer<IDialogTopicGetter>
{
    public static readonly TopicDefinition NoPrompt = MutagenTopicBuilder.FromDiscussion(
            499,
            "No Prompt",
            Severity.Warning)
        .WithoutFormatting("Topic has no prompt on the topic or on all responses and will show up as ... in game");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoPrompt];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var topic = param.Record;

        if (topic.Name is not null) return;
        if (topic.Responses.Count == 0) return;
        if (topic.SubtypeName != "CUST" && topic.SubtypeName != "PFGT") return;

        var responsesWithoutPrompt = topic.Responses.Where(r => r.Prompt is null).ToArray();
        if (responsesWithoutPrompt.Length == 0) return;

        var usageCache = param.ResolveCache<ILinkUsageCache>();
        var linkingBranches = usageCache.GetUsagesOf<IDialogBranchGetter>(topic).UsageLinks
            .Select(b => b.TryResolve(param.LinkCache))
            .WhereNotNull();

        if (linkingBranches.All(b => b.Flags is not null && !b.Flags.Value.HasFlag(DialogBranch.Flag.TopLevel))) return;

        var linkingResponses = usageCache.GetUsagesOf<IDialogResponsesGetter>(topic).UsageLinks
            .Select(b => b.TryResolve(param.LinkCache))
            .WhereNotNull();

        if (linkingResponses.All(r =>
            {
                // Either the response is an invisible continue
                if (r.Flags is not null && r.Flags.Flags.HasFlag(DialogResponses.Flag.InvisibleContinue)) return true;

                // Or it's the walkaway response and the walkaway is invisible
                if (r.Flags is not null && r.Flags.Flags.HasFlag(DialogResponses.Flag.WalkAwayInvisibleInMenu) && r.WalkAwayTopic.FormKey == topic.FormKey) return true;

                // Or it's not part of the links
                return r.LinkTo.All(l => l.FormKey != topic.FormKey);
            })) return;

        param.AddTopic(
            NoPrompt.Format(),
            ("Responses without prompt", responsesWithoutPrompt));
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
        yield return x => x.Responses;
        yield return x => x.Subtype;
    }
}
