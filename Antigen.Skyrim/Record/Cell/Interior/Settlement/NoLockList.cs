using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Cell.Interior.Settlement;

public class NoLockListAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition NoLockList = MutagenTopicBuilder.FromDiscussion(
            295,
            "No Lock List",
            Severity.Suggestion)
        .WithoutFormatting("Cell has no lock list");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoLockList];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        // Public cells should not have a lock list
        if (cell.IsPublic()) return;

        // Skip non-settlement cells
        if (!cell.IsSettlementCell(param.LinkCache)) return;

        if (cell.LockList.IsNull)
        {
            param.AddTopic(
                NoLockList.Format());
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.LockList;
    }
}
