using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
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

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILandscapeGetter> param)
    {
        var landscape = param.Record;
        if (landscape.VertexHeightMap == null)
            return;

        var cell = landscape.GetCell(param.LinkCache);
        var worldspace = cell?.GetWorldspace(param.LinkCache);
        if (cell?.Grid == null || worldspace == null) return;

        // For testing. Cell with recognisable shape when unlit
        if (cell.EditorID != "Riverwood02") return;
        //if (cell.Grid.Point != new P2Int(4, -14)) return;
        //if (worldspace.EditorID != "Tamriel") return;

        var heights = landscape.VertexHeightMap.Decode();

        void CheckNeigbour(Direction dir)
        {
            var neighbour = worldspace.GetCell(NeighbourCoords(cell.Grid.Point, dir), param.LinkCache)
                ?.GetLandscape(param.LinkCache);
            if (neighbour?.VertexHeightMap == null) return;

            var neighbourData = neighbour.VertexHeightMap.Decode();
            File.WriteAllText($"C:\\Modding\\Godot\\Riverwood{dir}.obj", LandscapeExtensions.ToObj(neighbourData));
        }
        CheckNeigbour(Direction.North);
        CheckNeigbour(Direction.East);
        CheckNeigbour(Direction.South);
        CheckNeigbour(Direction.West);

        var obj = LandscapeExtensions.ToObj(heights);
        File.WriteAllText("C:\\Modding\\Godot\\Riverwood.obj", obj);

        throw new NotImplementedException();
    }

    public IEnumerable<Func<ILandscapeGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VertexHeightMap;
    }
}
