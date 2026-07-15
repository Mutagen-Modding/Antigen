namespace Antigen.Models.Analyzer;

public record struct StatusUpdate(AnalyzerStatus Status, string? Message = null);