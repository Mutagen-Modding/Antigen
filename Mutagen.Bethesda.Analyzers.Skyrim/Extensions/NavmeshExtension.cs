using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class NavmeshExtension
{
    public static bool TryGetTriangleArea(this INavigationMeshDataGetter navmeshData, int triangleIndex, out float area)
    {
        if (triangleIndex < 0 || triangleIndex >= navmeshData.Triangles.Count)
        {
            area = -1;
            return false;
        }

        var triangle = navmeshData.Triangles[triangleIndex];

        return TryGetTriangleArea(navmeshData, triangle, out area);
    }

    public static bool TryGetTriangleArea(this INavigationMeshDataGetter navmeshData, INavmeshTriangleGetter triangle, out float area)
    {
        var vertexX = navmeshData.Vertices[triangle.Vertices.X];
        var vertexY = navmeshData.Vertices[triangle.Vertices.Y];
        var vertexZ = navmeshData.Vertices[triangle.Vertices.Z];

        var edgeA = vertexY - vertexX;
        var edgeB = vertexZ - vertexX;

        // Area of triangle = 0.5 * |edgeA x edgeB|
        area = 0.5f * edgeA.Cross(edgeB).Length;
        return true;
    }

    public static bool TryGetTriangleNormal(this INavigationMeshDataGetter navmeshData, int triangleIndex, out P3Float normal)
    {
        if (triangleIndex < 0 || triangleIndex >= navmeshData.Triangles.Count)
        {
            normal = default;
            return false;
        }

        var triangle = navmeshData.Triangles[triangleIndex];

        return TryGetTriangleNormal(navmeshData, triangle, out normal);
    }

    public static bool TryGetTriangleNormal(this INavigationMeshDataGetter navmeshData, INavmeshTriangleGetter triangle, out P3Float normal)
    {
        var vertexX = navmeshData.Vertices[triangle.Vertices.X];
        var vertexY = navmeshData.Vertices[triangle.Vertices.Y];
        var vertexZ = navmeshData.Vertices[triangle.Vertices.Z];

        var edgeA = vertexY - vertexX;
        var edgeB = vertexZ - vertexX;
        normal = edgeA.Cross(edgeB).Normalize();
        return true;
    }
}
