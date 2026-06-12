using System.Text;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class LandscapeExtensions
{
    public static readonly int GridSize = 33;
    public static readonly float HeightMult = 8;
    public static readonly float TriangleWidth = CellExtensions.CellLength / (GridSize - 1);
    public static readonly float ObjScale = 1.0f / 128.0f;

    /// <summary>
    /// Decode a landscape's heightmap
    /// </summary>
    /// <param name="heightMap"></param>
    /// <returns>Height data as a row-major array</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static float[,] Decode(this ILandscapeVertexHeightMapGetter heightMap)
    {
        if (heightMap.HeightMap.Width != GridSize || heightMap.HeightMap.Height != GridSize)
            throw new ArgumentOutOfRangeException(nameof(heightMap), $"Expected heightmap to be {GridSize}x{GridSize}");

        // Based on UESP https://en.uesp.net/wiki/Skyrim_Mod:Mod_File_Format/LAND
        var result = new float[GridSize, GridSize];

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
