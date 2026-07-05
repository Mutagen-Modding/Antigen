using System.Runtime.CompilerServices;
using System.Text;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class LandscapeExtensions
{
    public static readonly int GridSize = 33;
    public static readonly int QuadSize = 17;
    public static readonly float HeightMult = 8;
    public static readonly float TriangleWidth = CellExtensions.CellLength / (GridSize - 1);
    public static readonly float ObjScale = 1.0f / 128.0f;
    public static readonly IFormLinkGetter<ILandscapeTextureGetter> DefaultTexture = FormKeys.SkyrimSE.Skyrim.LandscapeTexture.LDirt02;
    public static readonly IReadOnlyArray2d<float> DefaultAlphaOpacity = new Array2d<float>(QuadSize, QuadSize, 0);

    /// <summary>
    /// Decode a landscape's heightmap
    /// </summary>
    /// <param name="heightMap"></param>
    /// <returns>Height data as a row-major array</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Array2d<float> Decode(this ILandscapeVertexHeightMapGetter heightMap)
    {
        if (heightMap.HeightMap.Width != GridSize || heightMap.HeightMap.Height != GridSize)
            throw new ArgumentOutOfRangeException(nameof(heightMap), $"Expected heightmap to be {GridSize}x{GridSize}");

        // Based on UESP https://en.uesp.net/wiki/Skyrim_Mod:Mod_File_Format/LAND
        var result = new Array2d<float>(new P2Int(GridSize, GridSize), 0);

        for (int col = 0; col < GridSize; col++)
        {
            for (int row = 0; row < GridSize; row++)
            {
                var prev = (row, col) switch
                {
                    (0, 0) => heightMap.Offset * HeightMult,
                    (0, _) => result[row, col-1],
                    (_, _) => result[row-1, col],
                };

                var delta = (sbyte)heightMap.HeightMap[row, col] * HeightMult;

                result[row, col] = prev + delta;
            }
        }

        return result;
    }

    public class QuadrantData
    {
        public readonly struct Layer
        {
            public required readonly IFormLinkGetter<ILandscapeTextureGetter> Texture { get; init; }
            public required readonly IReadOnlyArray2d<float> Opacity { get; init; }
        };

        static P2Int ToCoords(ushort encoded)
        {
            // Slight deviation from xEdit: transpose coordinates to match that of color and height data
            // which slightly simplifies the analyser
            return new(encoded % QuadSize, encoded / QuadSize);
        }

        public QuadrantData(Quadrant quadrant, IEnumerable<IBaseLayerGetter> layers)
        {
            Quadrant = quadrant;
            var baseTexture = DefaultTexture;
            var layerData = new List<Layer>();

            foreach (var layer in layers)
            {
                if (layer.Header == null)
                    throw new ArgumentException("Layer header should not be null");
                var texture = layer.Header.Texture.IsNull ? DefaultTexture : layer.Header.Texture;

                if (layer is IAlphaLayerGetter alpha)
                {
                    if (alpha.AlphaLayerData == null)
                        continue;

                    var data = new Array2d<float>(QuadSize, QuadSize, 0);
                    foreach (var point in alpha.AlphaLayerData)
                    {
                        data[ToCoords(point.Position)] = point.Opacity;
                    }
                    layerData.Add(new() { Texture = texture, Opacity = data });
                }
                else
                {
                    baseTexture = texture;
                }
            }

            var baseAlpha = new Array2d<float>(QuadSize, QuadSize, 0);
            foreach (var point in baseAlpha)
            {
                baseAlpha[point.Key] = 1.0f - layerData.Sum(l => l.Opacity[point.Key]);
            }
            layerData.Add(new() { Texture = baseTexture, Opacity = baseAlpha });
            Layers = layerData;
        }

        public readonly IReadOnlyList<Layer> Layers;
        public readonly Quadrant Quadrant;


        public IEnumerable<IFormLinkGetter<ILandscapeTextureGetter>> GetTextures()
        {
            return Layers.Select(l => l.Texture);
        }

        public Layer GetLayer(IFormLinkGetter<ILandscapeTextureGetter> texture)
        {
            return Layers.FirstOrDefault(
                l => l.Texture.Equals(texture),
                new() { Texture = DefaultTexture, Opacity = DefaultAlphaOpacity });
        }
    }

    public static QuadrantData DecodeQuadrant(this IEnumerable<IBaseLayerGetter> layers, Quadrant quadrant)
    {
        return new QuadrantData(quadrant, layers.Where(l => l.Header?.Quadrant == quadrant));
    }

    /// <summary>
    /// Mostly for testing
    /// </summary>
    /// <param name="data"></param>
    /// <returns>OBJ formatted landscape mesh, using Godot's coordinate system</returns>
    public static string ToObj(float[,] data)
    {
        var sb = new StringBuilder();

        static int Index(int col, int row)
        {
            return (col * GridSize) + row + 1;
        }

        // Write vertex data
        for (int col = 0; col < GridSize; col++)
        {
            for (int row = 0; row < GridSize; row++)
            {
                var x = col * TriangleWidth * ObjScale;
                var z = row * TriangleWidth * ObjScale;
                var y = data[col, row] * ObjScale;

                sb.AppendLine($"v {x} {y} {z}");
                sb.AppendLine($"vt {x / 33.0} {z / 33.0}");
            }
        }

        // Write index data
        for (int col = 0; col < GridSize - 1; col++)
        {
            for (int row = 0; row < GridSize - 1; row++)
            {
                var tl = Index(col, row);
                var tr = Index(col, row + 1);
                var bl = Index(col + 1, row);
                var br = Index(col + 1, row + 1);

                sb.AppendLine($"f {tl}/{tl} {tr}/{tr} {bl}/{bl}");
                sb.AppendLine($"f {tr}/{tr} {br}/{br} {bl}/{bl}");
            }
        }

        return sb.ToString();
    }

    public static ICellGetter? GetCell(this ILandscapeGetter landscape, ILinkCache linkCache)
    {
        if (!linkCache.TryResolveSimpleContext(landscape, out var context)) return null;

        return context.Parent?.Record as ICellGetter;
    }
}
