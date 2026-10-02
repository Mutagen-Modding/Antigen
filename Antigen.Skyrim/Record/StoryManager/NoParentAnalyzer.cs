using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.StoryManager;

public class NoParentAnalyzer : IIsolatedRecordAnalyzer<IAStoryManagerNodeGetter>
{
    public static readonly TopicDefinition NoParent = MutagenTopicBuilder.FromDiscussion(
            419,
            "No Parent",
            Severity.Error)
        .WithoutFormatting("Node has no parent");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoParent];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IAStoryManagerNodeGetter> param)
    {
        var branchNode = param.Record;
        if (branchNode.Parent.IsNull && branchNode.FormKey != FormKeys.SkyrimSE.Skyrim.AStoryManagerNode.Root.FormKey)
        {
            param.AddTopic(
                NoParent.Format());
        }
    }

    public IEnumerable<Func<IAStoryManagerNodeGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Parent;
    }
}
