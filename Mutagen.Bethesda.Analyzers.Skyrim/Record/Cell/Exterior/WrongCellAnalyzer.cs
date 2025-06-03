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
        if (((Bethesda.Skyrim.Cell.MajorFlag)cell.SkyrimMajorRecordFlags).HasFlag(Bethesda.Skyrim.Cell.MajorFlag.Persistent)) return;

        foreach (var placed in cell.Temporary.Concat(cell.Persistent))
        {
            if (placed.Placement is null) continue;

            var position = placed.Placement.Position;

            const int cellLength = 4096;
            var cellX = position.X / cellLength;
            var cellY = position.Y / cellLength;

            var expectedCell = new P2Int(
                cellX < 0 ? (int)Math.Floor(cellX) : (int)cellX,
                cellY < 0 ? (int)Math.Floor(cellY) : (int)cellY);

            if (actualCell != expectedCell)
            {
                // If the cell is exactly at the border of two cell (so something like 4096.) then both cells are allowed
                if (Math.Abs(cellX % 1) < 0.00001)
                {
                    expectedCell.X += (cellX < 0) ? -1 : 1;
                    if (actualCell == expectedCell)
                    {
                        continue;
                    }

                    if (Math.Abs(cellY % 1) < 0.00001)
                    {
                        expectedCell.Y += (cellY < 0) ? -1 : 1;
                        if (actualCell == expectedCell)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if (Math.Abs(cellY % 1) < 0.00001)
                    {
                        expectedCell.Y += (cellY < 0) ? -1 : 1;
                        if (actualCell == expectedCell)
                        {
                            continue;
                        }
                    }
                }

                // Revert expected cell to the original calculation
                expectedCell = new P2Int(
                    cellX < 0 ? (int)Math.Floor(cellX) : (int)cellX,
                    cellY < 0 ? (int)Math.Floor(cellY) : (int)cellY);

                param.AddTopic(
                    WrongCell.Format(
                        placed,
                        actualCell,
                        expectedCell));
            }
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
