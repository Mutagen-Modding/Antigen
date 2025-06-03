using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class InvalidPlacementAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedGetter, string, float> PlacementOutOfBounds = MutagenTopicBuilder.FromDiscussion(
            391,
            "Placement Out Of Bounds",
            Severity.Warning)
        .WithFormatting<IPlacedGetter, string, float>("Placed {0} in interior cell is placed out of bounds in {1} axis: {2}");

    public IEnumerable<TopicDefinition> Topics { get; } = [PlacementOutOfBounds];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        if (cell.IsDeleted) return;

        if (cell.IsExteriorCell()) return;

        foreach (var placed in cell.Temporary.Concat(cell.Persistent))
        {
            if (placed.Placement is not {} placement) return;

            if (placement.Position.X is <= -30000 or >= 30000)
            {
                param.AddTopic(
                    PlacementOutOfBounds.Format(placed, "X", placement.Position.X));
            }
            else if (placement.Position.Y is <= -30000 or >= 30000)
            {
                param.AddTopic(
                    PlacementOutOfBounds.Format(placed, "Y", placement.Position.Y));
            }
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
