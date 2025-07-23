using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Navmesh;

public class NavmeshTriangleAnalyzer : IIsolatedRecordAnalyzer<INavigationMeshGetter>
{
    public static readonly TopicDefinition<int, int> TriangleNormal = MutagenTopicBuilder.FromDiscussion(
            400,
            "Linked triangles rotated in opposite directions",
            Severity.Warning)
        .WithFormatting<int, int>("Linked triangles {0} and {1} are rotated in opposite directions");

    public static readonly TopicDefinition<int, float> TriangleTooSmall = MutagenTopicBuilder.FromDiscussion(
            401,
            "Triangle is too small",
            Severity.Warning)
        .WithFormatting<int, float>("Triangle {0} has an area of {1}, which is too small");

    public IEnumerable<TopicDefinition> Topics => [TriangleNormal, TriangleTooSmall];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<INavigationMeshGetter> param)
    {
        var navmesh = param.Record;
        if (navmesh.Data is null) return;

        var alreadyCheckedTriangles = new HashSet<int>();
        for (var triangleIndex = 0; triangleIndex < navmesh.Data.Triangles.Count; triangleIndex++)
        {
            // Check neighbors
            var triangle = navmesh.Data.Triangles[triangleIndex];
            if (!navmesh.Data.TryGetTriangleNormal(triangle, out var normal))
            {
                if (!triangle.Flags.HasFlag(NavmeshTriangle.Flag.EdgeLink_0_1))
                {
                    CheckNeighboringTriangle(triangle.EdgeLink_0_1);
                }

                if (!triangle.Flags.HasFlag(NavmeshTriangle.Flag.EdgeLink_1_2))
                {
                    CheckNeighboringTriangle(triangle.EdgeLink_1_2);
                }

                if (!triangle.Flags.HasFlag(NavmeshTriangle.Flag.EdgeLink_2_0))
                {
                    CheckNeighboringTriangle(triangle.EdgeLink_2_0);
                }

                void CheckNeighboringTriangle(short neighboringTriangleIndex)
                {
                    if (alreadyCheckedTriangles.Contains(neighboringTriangleIndex)) return;
                    if (float.IsNaN(normal.X)) return;
                    if (!navmesh.Data.TryGetTriangleNormal(neighboringTriangleIndex, out var neighboringNormal)) return;
                    if (float.IsNaN(neighboringNormal.X)) return;

                    var dot = normal.Dot(neighboringNormal);
                    if (dot > 0) return;
                    // if (Math.Abs(dot + 1) > 0.001) return;

                    param.AddTopic(
                        TriangleNormal.Format(triangleIndex, neighboringTriangleIndex));
                }
            }

            // Check triangle area
            if (navmesh.Data.TryGetTriangleArea(triangle, out var area) && area < 0.01f)
            {
                param.AddTopic(
                    TriangleTooSmall.Format(triangleIndex, area));
            }

            alreadyCheckedTriangles.Add(triangleIndex);
        }
    }

    public IEnumerable<Func<INavigationMeshGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Data?.Triangles;
        yield return x => x.Data?.Vertices;
    }
}
