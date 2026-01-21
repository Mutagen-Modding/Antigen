using System.IO.Abstractions;
using System.Text;
using Mutagen.Bethesda.Analyzers.Cli;
using Mutagen.Bethesda.Analyzers.Cli.Args;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Noggog;
using Noggog.Testing.Extensions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Mutagen.Bethesda.Analyzers.Tests.Integration;

public class RunAnalyzersCommandTests
{
    private readonly ITestOutputHelper _output;

    public RunAnalyzersCommandTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory, AnalyzerAutoData]
    public async Task RunAnalyzersCommand_ExecutesSuccessfully(
        IFileSystem fileSystem,
        DirectoryPath dataDirectory)
    {
        // Arrange - Create a minimal mod in memory
        var mod = new SkyrimMod(ModKey.FromNameAndExtension("TestMod.esp"), SkyrimRelease.SkyrimSE);
        var npc = mod.Npcs.AddNew();
        npc.Name = "Test NPC";
        npc.Height = 1.0f;

        // Write the mod to the in-memory file system
        var modPath = Path.Combine(dataDirectory.Path, mod.ModKey.FileName);
        fileSystem.Directory.CreateDirectory(dataDirectory.Path);

        mod.BeginWrite
            .ToPath(modPath)
            .WithNoLoadOrder()
            .WithFileSystem(fileSystem)
            .Write();

        // Create command pointing to our in-memory data folder
        var command = new RunAnalyzersCommand
        {
            GameRelease = GameRelease.SkyrimSE,
            DataFolder = dataDirectory.Path,
            LoadOrder = mod.ModKey.FileName,
            MinimumSeverity = SDK.Topics.Severity.Suggestion,
            PrintTopics = false
        };

        // Capture console output
        var consoleOutput = new StringBuilder();
        var originalOut = Console.Out;
        try
        {
            using (var stringWriter = new StringWriter(consoleOutput))
            {
                Console.SetOut(stringWriter);

                // Act - Execute the command
                var result = await RunAnalyzers.Run(command);

                // Assert - Should complete successfully
                result.ShouldBe(0);
            }

            var output = consoleOutput.ToString();
            _output.WriteLine("=== Console Output ===");
            _output.WriteLine(output);
            _output.WriteLine("=== End Console Output ===");

            output.ShouldContain(RunAnalyzers.RunText);
        }
        finally
        {
            // Restore original console output
            Console.SetOut(originalOut);
        }
    }
}
