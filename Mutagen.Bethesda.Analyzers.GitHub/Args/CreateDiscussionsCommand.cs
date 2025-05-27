using CommandLine;

namespace Mutagen.Bethesda.Analyzers.GitHub.Args;

[Verb("create-discussions", HelpText = "Create GitHub discussions for the topics")]
public class CreateDiscussionsCommand
{
    [Option('g', "GameRelease", Required = true, HelpText = "Game release to create discussions for")]
    public GameRelease GameRelease { get; set; }

    [Option('t', "Token", Required = true, HelpText = "GitHub access token")]
    public string Token { get; set; } = null!;

    [Option('d', "Directory", Required = false, HelpText = "Directory to find the topics from")]
    public string? Directory { get; set; } = null;
}
