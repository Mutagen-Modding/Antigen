using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class PublicCellAnalyzer : IIsolatedRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition HasLockList = MutagenTopicBuilder.FromDiscussion(
            263,
            "Public Cell has Lock List",
            Severity.Warning)
        .WithoutFormatting("Public cell has lock list");

    public IEnumerable<TopicDefinition> Topics { get; } = [HasLockList];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsExteriorCell() || !cell.IsPublic() || cell.LockList.IsNull) return;

        param.AddTopic(
            HasLockList.Format()
        );
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.LockList;
        yield return x => x.Music;
    }
}
