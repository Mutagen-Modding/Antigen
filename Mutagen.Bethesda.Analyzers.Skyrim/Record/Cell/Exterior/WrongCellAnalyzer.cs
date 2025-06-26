using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Exterior;

public class WrongCellAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedGetter, P2Int, P2Int> WrongCell = MutagenTopicBuilder.FromDiscussion(
            383,
            "Wrong Cell",
            Severity.Error)
        .WithFormatting<IPlacedGetter, P2Int, P2Int>("Placement {0} is placed in cell ({1}), but should be placed in cell ({2})");

    public IEnumerable<TopicDefinition> Topics { get; } = [WrongCell];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        // Skip non-exterior cells
        if (!cell.IsExteriorCell()) return;
        if (cell.Grid is not { Point: var actualCell }) return;
        if (cell.MajorFlags.HasFlag(Bethesda.Skyrim.Cell.MajorFlag.Persistent)) return;

        foreach (var placed in cell.Temporary.Concat(cell.Persistent))
        {
            if (placed.Placement is null) continue;

            var expectedCell = placed.Placement.GetCellCoordinates();
            if (expectedCell == actualCell) continue;

            param.AddTopic(
                WrongCell.Format(
                    placed,
                    actualCell,
                    expectedCell));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Location;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
