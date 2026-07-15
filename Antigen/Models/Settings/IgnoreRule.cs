namespace Antigen.Models.Settings;

public sealed record IgnoreRule(
    IgnoreType Type,
    string Identifier
);