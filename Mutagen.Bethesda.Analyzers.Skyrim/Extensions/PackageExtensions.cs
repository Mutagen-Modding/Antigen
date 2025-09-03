using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class PackageExtensions
{
    public static string? GetPackageDataName(this IPackageGetter package, sbyte key, ILinkCache linkCache)
    {
        // Check template
        var template = package.PackageTemplate.TryResolve(linkCache);
        if (template is not null && template.Data.TryGetValue(key, out var templateData))
        {
            return templateData.Name;
        }

        // Check package itself
        if (package.Data.TryGetValue(key, out var data))
        {
            return data.Name;
        }

        return null;
    }

    public static IEnumerable<IPackageBranchGetter> GetDataUsageInProcedure(this IReadOnlyList<IPackageBranchGetter> branches, byte dataIndex)
    {
        foreach (var branch in branches)
        {
            if (!branch.BranchType.Equals("Procedure", StringComparison.OrdinalIgnoreCase)) continue;

            if (branch.DataInputIndices.Contains(dataIndex))
            {
                yield return branch;
            }
        }
    }

    public record ProcedureTreeNode
    {
        public required IPackageBranchGetter Branch { get; init; }
        public List<ProcedureTreeNode> Children { get; init; } = [];
        public ProcedureTreeNode? Parent { get; init; }

        public ProcedureTreeNode? FindNode(IPackageBranchGetter branch)
        {
            if (Branch.Equals(branch)) return this;

            return Children.Select(child => child.FindNode(branch)).FirstOrDefault();
        }
    }

    public static ProcedureTreeNode BuildProcedureTree(this IEnumerator<IPackageBranchGetter> branches, ProcedureTreeNode? parent = null)
    {
        var branch = branches.Current;

        var node = new ProcedureTreeNode
        {
            Branch = branch,
            Parent = parent,
        };

        if (!branches.MoveNext() || branch.Root is null)
        {
            return node;
        }

        for (var i = 0; i < branch.Root.BranchCount; i++)
        {
            node.Children.Add(BuildProcedureTree(branches, node));
        }

        return node;
    }

    public static ProcedureTreeNode BuildProcedureTree(this IReadOnlyList<IPackageBranchGetter> branches)
    {
        using var enumerator = branches.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.BuildProcedureTree();
    }
}
