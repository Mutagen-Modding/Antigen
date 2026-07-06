using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Caches;
using Mutagen.Bethesda.Plugins;
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

    public static readonly TopicDefinition<Quadrant, Direction, IFormLinkGetter<ILandscapeTextureGetter>> TextureSeam = MutagenTopicBuilder.FromDiscussion(
            633,
            "Landscape texture seam",
            Severity.Warning)
        .WithFormatting<Quadrant, Direction, IFormLinkGetter<ILandscapeTextureGetter>>("Landscape quadrant {0} has texture seam in direction {1} with texture {2}");

    static readonly IReadOnlyArray2d<P3UInt8> DefaultVertexColors = new Array2d<P3UInt8>(new P2Int(LandscapeExtensions.GridSize, LandscapeExtensions.GridSize), new P3UInt8(255, 255, 255));
    // Minimum opacity difference to raise DefaultVertexColors. Somewhat arbritrary based on floating point errors + min difference for perception
    static readonly float AlphaOpacityEpsilon = 1.0f / 8.0f;

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

    public readonly struct Difference<T>
    {
        public readonly int Index { get; init; }
        public readonly T Self { get; init;  }
        public readonly T Other { get; init; }

        public override string ToString()
        {
            return $"At {Index}: ({Self}) vs ({Other})";
        }

        public static IEnumerable<Difference<T>> GetDifferences(IEnumerable<T> self, IEnumerable<T> other, Func<T, T, bool> diffPredicate)
        {
            return self.Zip(other)
                .Select((pair, index) => new Difference<T>() { Index = index, Self = pair.First, Other = pair.Second })
                .Where(d => diffPredicate(d.Self, d.Other));
        }
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILandscapeGetter> param)
    {
        var landscape = param.Record;

        var cell = landscape.GetCell(param.LinkCache);
        var worldspace = cell?.GetWorldspace(param.LinkCache);
        if (cell?.Grid == null || worldspace == null) return;

        var usageCache = param.ResolveCache<ILinkUsageCache>();
        var exteriorCache = param.ResolveCache<IExteriorCellCache>();

        ILandscapeGetter? GetLandscape(P2Int point)
        {
            return exteriorCache.GetExterior(worldspace, point).TryResolve(param.LinkCache)?.GetLandscape(param.LinkCache);
        }

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
                var neighbour = GetLandscape(cell.Grid.Point + ToOffset(dir));
                if (neighbour == null) return;
                var neighbourData = getData(neighbour);
                if (neighbourData == null) return;

                var edgeSelf = GetEdge(data, dir);
                var edgeOther = GetEdge(neighbourData, Opposite(dir));

                var diff = Difference<T>.GetDifferences(edgeSelf, edgeOther, (a, b) => !a.Equals(b));
                if (diff.Any())
                    param.AddTopic(topic.Format(dir), ("Differences", diff));
            }
            CheckNeigbour(Direction.North);
            CheckNeigbour(Direction.East);
            CheckNeigbour(Direction.South);
            CheckNeigbour(Direction.West);
        }

        CheckSeams(HeightMapSeam, l => l.VertexHeightMap?.Decode());
        CheckSeams(VertexColorSeam, l => l.VertexColors ?? DefaultVertexColors);

        void CheckTextures(LandscapeExtensions.QuadrantData selfQuadrant, LandscapeExtensions.QuadrantData otherQuadrant, Direction selfToOther)
        {
            foreach (var texture in selfQuadrant.GetTextures().And(otherQuadrant.GetTextures()).Distinct())
            {
                var edgeSelf = GetEdge(selfQuadrant.GetLayer(texture).Opacity, selfToOther);
                var edgeOther = GetEdge(otherQuadrant.GetLayer(texture).Opacity, Opposite(selfToOther));

                // We need an epsilon here since opacities are stored as floats
                var diff = Difference<float>.GetDifferences(edgeSelf, edgeOther, (a, b) => !a.EqualsWithin(b, AlphaOpacityEpsilon));

                var zipped = edgeSelf.Zip(edgeOther);
                if (diff.Any())
                    param.AddTopic(TextureSeam.Format(selfQuadrant.Quadrant, selfToOther, texture), ("Differences", diff));
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

        var north = GetLandscape(cell.Grid.Point + ToOffset(Direction.North));
        if (north != null)
        {
            CheckTextures(tl, north.Layers.DecodeQuadrant(Quadrant.BottomLeft), Direction.North);
            CheckTextures(tr, north.Layers.DecodeQuadrant(Quadrant.BottomRight), Direction.North);
        }
        var east = GetLandscape(cell.Grid.Point + ToOffset(Direction.North));
        if (east != null)
        {
            CheckTextures(tr, east.Layers.DecodeQuadrant(Quadrant.TopLeft), Direction.East);
            CheckTextures(br, east.Layers.DecodeQuadrant(Quadrant.BottomLeft), Direction.East);
        }
        var south = GetLandscape(cell.Grid.Point + ToOffset(Direction.North));
        if (south != null)
        {
            CheckTextures(bl, south.Layers.DecodeQuadrant(Quadrant.TopLeft), Direction.South);
            CheckTextures(br, south.Layers.DecodeQuadrant(Quadrant.TopRight), Direction.South);
        }
        var west = GetLandscape(cell.Grid.Point + ToOffset(Direction.North));
        if (west != null)
        {
            CheckTextures(tl, west.Layers.DecodeQuadrant(Quadrant.TopRight), Direction.West);
            CheckTextures(bl, west.Layers.DecodeQuadrant(Quadrant.BottomRight), Direction.West);
        }
    }

    public IEnumerable<Func<ILandscapeGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VertexHeightMap;
    }
}
