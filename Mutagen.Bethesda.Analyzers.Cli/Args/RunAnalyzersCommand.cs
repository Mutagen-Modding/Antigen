using CommandLine;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Cli.Args;

[Verb("run-analyzers", HelpText = "Run analyzers on a game installation")]
public class RunAnalyzersCommand : IMinimumSeverityConfiguration
{
    [Option('g', "GameRelease", Required = true, HelpText = "Game Release to target")]
    public GameRelease GameRelease { get; set; }

    [Option("PrintTopics", Required = false, HelpText = "Whether to print the topics being run")]
    public bool PrintTopics { get; set; } = false;

    [Option('s', "Severity", HelpText = "Minimum severity required in order to report")]
    public Severity MinimumSeverity { get; set; } = Severity.Suggestion;

    [Option("RunConfigPath", HelpText = "Optional path to a run config file")]
    public string? RunConfigPath { get; set; } = null;

    [Option('o', "OutputFilePath", HelpText = "Optional output file path to save the report")]
    public string? OutputFilePath { get; set; } = null;

    [Option("DataFolder", HelpText = "Optional directory path to use a custom data folder for the analysis")]
    public string? DataFolder { get; set; } = null;

    [Option("LoadOrder", HelpText = "Optional list of mod file names to set a custom load order, separated by commas")]
    public string? LoadOrder { get; set; } = null;

    [Option("BlacklistedMods", HelpText = "Optional list of mod file names excluded from analyzing, separated by commas")]
    public string? BlacklistedMods { get; set; } = null;

    [Option('t', "NumThreads", HelpText = "Number of threads to use")]
    public int? NumThreads { get; set; }

    [Option("PrintMetadata", HelpText = "Disable printing metadata")]
    public bool? PrintMetadata { get; set; }
}
