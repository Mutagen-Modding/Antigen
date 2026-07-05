using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Caches;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
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

    public static readonly TopicDefinition<Direction> HeightMapSeam = MutagenTopicBuilder.FromDiscussion(
            606,
            "Landscape height map seam",
            Severity.Error)
        .WithFormatting<Direction>("Landscape heightmap has seam in direction {0}");

    public static readonly TopicDefinition<Direction> VertexColorSeam = MutagenTopicBuilder.FromDiscussion(
            609,
            "Landscape vertex color seam",
            Severity.Warning)
        .WithFormatting<Direction>("Landscape vertex colors have seam in direction {0}");

    public static readonly TopicDefinition<Quadrant, Direction, IFormLinkGetter<ILandscapeTextureGetter>> TextureSeam = MutagenTopicBuilder.DevelopmentTopic(
            "Landscape texture seam",
            Severity.Warning)
        .WithFormatting<Quadrant, Direction, IFormLinkGetter<ILandscapeTextureGetter>>("Landscape quadrant {0} has seam in direction {1} with texture {2}");

    static readonly IReadOnlyArray2d<P3UInt8> DefaultVertexColors = new Array2d<P3UInt8>(new P2Int(LandscapeExtensions.GridSize, LandscapeExtensions.GridSize), new P3UInt8(255, 255, 255));

    public IEnumerable<TopicDefinition> Topics => [HeightMapSeam, VertexColorSeam, TextureSeam];

    static P2Int ToOffset(Direction direction)
    {
        return direction switch
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

    static IEnumerable<T> GetEdge<T>(IReadOnlyArray2d<T> data, Direction direction)
    {
        return direction switch
        {
            Direction.North => data.GetRow(data.Height - 1),
            Direction.East => data.GetColumn(data.Width - 1),
            Direction.South => data.GetRow(0),
            Direction.West => data.GetColumn(0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
    }

    static bool HasSeam<T>(IEnumerable<T> a, IEnumerable<T> b)
        where T : IEquatable<T>
    {
        foreach (var (a1, b1) in a.Zip(b))
        {
            // Don't need a large epsilon here. While the heightmap is a float, all vanilla landscape uses integer offsets
            if (!a1.Equals(b1))
                return true;
        }
        return false;
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILandscapeGetter> param)
    {
        var landscape = param.Record;

        var cell = landscape.GetCell(param.LinkCache);
        var worldspace = cell?.GetWorldspace(param.LinkCache);
        if (cell?.Grid == null || worldspace == null) return;

        var usageCache = param.ResolveCache<ILinkUsageCache>();
        var exteriorCache = param.ResolveCache<IExteriorCellCache>();

        if (!cell.IsNearBorderRegion(param.LinkCache, usageCache, exteriorCache))
            return;


        void CheckSeams<T>(TopicDefinition<Direction> topic, Func<ILandscapeGetter, IReadOnlyArray2d<T>?> getData)
            where T : IEquatable<T>
        {
            var data = getData(landscape);
            if (data == null)
                return;

            void CheckNeigbour(Direction dir)
            {
                var neighbour = exteriorCache.GetExterior(worldspace, cell.Grid.Point + ToOffset(dir)).TryResolve(param.LinkCache)
                    ?.GetLandscape(param.LinkCache);
                if (neighbour == null) return;
                var neighbourData = getData(neighbour);
                if (neighbourData == null) return;

                var edgeSelf = GetEdge(data, dir);
                var edgeOther = GetEdge(neighbourData, Opposite(dir));

                // TODO: Add context about sizes and positions of seams. Only include differing points
                if (HasSeam(edgeSelf, edgeOther))
                    param.AddTopic(topic.Format(dir), ("Edge", edgeSelf.Zip(edgeOther)));
            }
            CheckNeigbour(Direction.North);
            CheckNeigbour(Direction.East);
            CheckNeigbour(Direction.South);
            CheckNeigbour(Direction.West);
        }

        CheckSeams(HeightMapSeam, l => l.VertexHeightMap?.Decode());
        CheckSeams(VertexColorSeam, l => l.VertexColors ?? DefaultVertexColors);

        void CheckTextures(LandscapeExtensions.QuadrantData selfQuadrant, LandscapeExtensions.QuadrantData otherQuadrant, Direction firstToSecond)
        {
            foreach (var texture in selfQuadrant.GetTextures().And(otherQuadrant.GetTextures()).Distinct())
            {
                var edgeSelf = GetEdge(selfQuadrant.GetLayer(texture).Opacity, firstToSecond);
                var edgeOther = GetEdge(otherQuadrant.GetLayer(texture).Opacity, Opposite(firstToSecond));

                // We need an epsilon here since opacities are stored as floats
                var zipped = edgeSelf.Zip(edgeOther);
                // TODO: Add context about sizes and positions of seams. Only include differing points
                if (!zipped.All(p => p.First.EqualsWithin(p.Second, AlphaOpacityEpsilon)))
                    param.AddTopic(TextureSeam.Format(selfQuadrant.Quadrant, firstToSecond, texture), ("Edge", zipped));
            }
        }

        // Landscape textures ar broken into four quadrants per cell
        // This analysers checks are described as:
        // Where `[bt][lr]` defines a quadrant, and `[NESW]` defines a neighbouring cell
        //     | Nbl | Nbr |
        // Wtr | tl  | tr  | Etl
        //     +-----+-----+
        // Wbr | bl  | br  | Ebl
        //     | Stl | Str |


        var tl = landscape.Layers.DecodeQuadrant(Quadrant.TopLeft);
        var tr = landscape.Layers.DecodeQuadrant(Quadrant.TopRight);
        var bl = landscape.Layers.DecodeQuadrant(Quadrant.BottomLeft);
        var br = landscape.Layers.DecodeQuadrant(Quadrant.BottomRight);

        CheckTextures(tl, tr, Direction.East);
        CheckTextures(tl, bl, Direction.South);
        CheckTextures(bl, br, Direction.East);
        CheckTextures(tr, br, Direction.South);

        var north = exteriorCache.GetExterior(worldspace, cell.Grid.Point + ToOffset(Direction.North)).TryResolve(param.LinkCache)?.GetLandscape(param.LinkCache);
        if (north?.Layers != null)
        {
            CheckTextures(tl, north.Layers.DecodeQuadrant(Quadrant.BottomLeft), Direction.North);
            CheckTextures(tr, north.Layers.DecodeQuadrant(Quadrant.BottomRight), Direction.North);
        }
        // TODO: East, south, west
    }

    public IEnumerable<Func<ILandscapeGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VertexHeightMap;
    }
}
