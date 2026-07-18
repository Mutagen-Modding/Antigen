using DynamicData.Binding;

namespace Antigen.Models;

/// <summary>
/// Represents a group node in the hierarchical tree structure.
/// </summary>
public sealed class GroupNode(string key)
{
    public string Key { get; } = key;
    public ObservableCollectionExtended<object> Children { get; } = [];
}