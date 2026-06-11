using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Landscape;

public class LandscapeSeamAnalyzer : IContextualRecordAnalyzer<ILandscapeGetter>
{
    public enum Direction
    {
        North,
        East,
        South,
        West
    };

    public static readonly TopicDefinition<Direction> LandscapeSeam = MutagenTopicBuilder.DevelopmentTopic(
            "Landscape seam",
            Severity.Error)
        .WithFormatting<Direction>("Landscape has seam in direction {0}");

    public IEnumerable<TopicDefinition> Topics => [LandscapeSeam];

    static P2Int NeighbourCoords(P2Int origin, Direction direction)
    {
        return origin + direction switch
        {
            Direction.North => new P2Int(0, 1),
            Direction.East => new P2Int(1, 0),
            Direction.South => new P2Int(0, -1),
            Direction.West => new P2Int(-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
    }
    static Direction Opposite(Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
    }

    static IEnumerable<float> GetEdge(float[,] data, Direction direction)
    {
        return direction switch
        {
            Direction.North => data.GetRow(LandscapeExtensions.GridSize - 1),
            Direction.East => data.GetColumn(LandscapeExtensions.GridSize - 1),
            Direction.South => data.GetRow(0),
            Direction.West => data.GetColumn(0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
    }

    static bool HasSeam(IEnumerable<float> a, IEnumerable<float> b)
    {
        foreach (var (a1, b1) in a.Zip(b))
        {
            if (!a1.EqualsWithin(b1))
                return true;
        }
        return false;
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILandscapeGetter> param)
    {
        var landscape = param.Record;
        if (landscape.VertexHeightMap == null)
            return;

        var cell = landscape.GetCell(param.LinkCache);
        var worldspace = cell?.GetWorldspace(param.LinkCache);
        if (cell?.Grid == null || worldspace == null) return;

        var heights = landscape.VertexHeightMap.Decode();

        void CheckNeigbour(Direction dir)
        {
            var neighbour = worldspace.GetCell(NeighbourCoords(cell.Grid.Point, dir), param.LinkCache)
                ?.GetLandscape(param.LinkCache);
            if (neighbour?.VertexHeightMap == null) return;

            var neighbourHeights = neighbour.VertexHeightMap.Decode();

            var edgeSelf = GetEdge(heights, dir);
            var edgeOther = GetEdge(neighbourHeights, Opposite(dir));

            if (HasSeam(edgeSelf, edgeOther))
                param.AddTopic(LandscapeSeam.Format(dir));
        }
        CheckNeigbour(Direction.North);
        CheckNeigbour(Direction.East);
        CheckNeigbour(Direction.South);
        CheckNeigbour(Direction.West);
    }

    public IEnumerable<Func<ILandscapeGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VertexHeightMap;
    }
}
