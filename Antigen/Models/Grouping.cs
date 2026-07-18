namespace Antigen.Models;

public sealed record Grouping<T>(string Name, Func<T, string?> Selector) : IGrouping;
