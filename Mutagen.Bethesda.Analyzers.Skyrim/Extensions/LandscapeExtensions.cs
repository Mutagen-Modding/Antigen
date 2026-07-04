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

    public class TextureData
    {
        public QuadrantData GetQuadrant(Quadrant quadrant)
        {
            return quadrant switch
            {
                Quadrant.BottomLeft => BottomLeft,
                Quadrant.BottomRight => BottomRight,
                Quadrant.TopLeft => TopLeft,
                Quadrant.TopRight => TopRight,
                _ => throw new ArgumentOutOfRangeException(nameof(quadrant)),
            };
        }

        public TextureData(IEnumerable<IBaseLayerGetter> layers)
        {
            TopLeft = new QuadrantData(layers.Where(l => l.Header?.Quadrant == Quadrant.TopLeft));
            TopRight = new QuadrantData(layers.Where(l => l.Header?.Quadrant == Quadrant.TopRight));
            BottomLeft = new QuadrantData(layers.Where(l => l.Header?.Quadrant == Quadrant.BottomLeft));
            BottomRight = new QuadrantData(layers.Where(l => l.Header?.Quadrant == Quadrant.BottomRight));
            // Texture paint is stored as a base layer and zero or more alpha layers for each quadrant
            // Alpha layers are layered on top of the base layer and are defined as a key-value map of position->alpha
            // Alpha layer values for a position sum to <= 1, if sum is less then 1 then the remainder is the base layer
            // If a layer's texture is null, it is treated as LDirt02
        }

        public QuadrantData TopLeft { get; }
        public QuadrantData TopRight { get; }
        public QuadrantData BottomLeft { get; }
        public QuadrantData BottomRight { get; }
    }

    public class QuadrantData
    {
        P2Int ToCoords(ushort encoded)
        {
            return new(encoded / QuadSize, encoded % QuadSize);
        }

        public QuadrantData(IEnumerable<IBaseLayerGetter> layers)
        {
            _baseTexture = DefaultTexture;
            foreach (var layer in layers)
            {
                if (layer.Header == null)
                    throw new ArgumentException("Layer header should not be null");

                if (layer is IAlphaLayerGetter alpha)
                {
                    if (alpha.AlphaLayerData == null)
                        continue;

                    var texture = layer.Header.Texture.IsNull ? DefaultTexture : layer.Header.Texture;
                    _alphaLayers[texture.FormKey] = alpha.AlphaLayerData
                        .ToDictionary(p => ToCoords(p.Position), p => p.Opacity);
                }
                else
                {
                    _baseTexture = layer.Header.Texture;
                }
            }
        }

        IFormLinkGetter<ILandscapeTextureGetter> _baseTexture;
        Dictionary<FormKey, Dictionary<P2Int, float>> _alphaLayers = [];


        public IEnumerable<IFormLinkGetter<ILandscapeTextureGetter>> GetTextures()
        {
            yield return _baseTexture;
            foreach (var layer in _alphaLayers.Keys)
                yield return layer.ToLink<ILandscapeTextureGetter>();
        }

        public float GetOpacity(IFormLinkGetter<ILandscapeTextureGetter> texture, P2Int position)
        {
            if (texture.Equals(_baseTexture))
            {
                var sum = _alphaLayers.Values.Select(l => l.GetOrDefault(position)).Sum();
                return 1.0f - sum;
            }
            else
            {
                return _alphaLayers.GetOrDefault(texture.FormKey)?.GetOrDefault(position) ?? 0;
            }
        }
    }

    public static TextureData Decode(this IEnumerable<IBaseLayerGetter> layers)
    {
        return new TextureData(layers);
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
