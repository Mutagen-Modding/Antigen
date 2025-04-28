using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Config;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Noggog.Testing.Extensions;
using Noggog.Testing.IO;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Config;

[method: SuppressMessage("ReSharper", "ContextualLoggerProblem", Justification = "Passed in")]
public class RunConfigReader(
    ILogger<ConfigReader<IRunConfig>> logger,
    ProcessDataDirectoryPath p1,
    ProcessLoadOrderSetToMods p2,
    ProcessOutputFilePath p3)
{
    public readonly ConfigReader<IRunConfig> Reader = new(logger, [p1, p2, p3]);
}

public class RunConfigReaderTests
{
    [Theory, AnalyzerAutoData]
    public void TestDataDirectoryForwardSlash(
        RunConfig config,
        RunConfigReader sut)
    {
        var line = $"environment.data_directory = {PathingUtil.DrivePrefix}some/path";
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.DataDirectoryPath.ShouldNotBeNull();
        config.DataDirectoryPath!.Value.Path.ShouldBe(Path.Combine(PathingUtil.DrivePrefix, "some", "path"));
    }

    [Theory, AnalyzerAutoData]
    public void TestDataDirectoryBackwardSlash(
        RunConfig config,
        RunConfigReader sut)
    {
        var line = @$"environment.data_directory = {PathingUtil.DrivePrefix}some\path";
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.DataDirectoryPath.ShouldNotBeNull();
        config.DataDirectoryPath!.Value.Path.ShouldBe(Path.Combine(PathingUtil.DrivePrefix, "some", "path"));
    }

    [Theory]
    [AnalyzerInlineData("environment.load_order.set_to_mods = Skyrim.esm, Update.esm")]
    public void TestSetToMods(
        string line,
        RunConfig config,
        RunConfigReader sut)
    {
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.LoadOrderSetToMods!.ShouldEqual([FormKeys.SkyrimSE.Skyrim.ModKey, Update.ModKey]);
    }

    [Theory, AnalyzerAutoData]
    public void TestOutputFilePathForwardSlash(
        RunConfig config,
        RunConfigReader sut)
    {
        var line = $"output_file = {PathingUtil.DrivePrefix}some/path";
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.OutputFilePath.ShouldNotBeNull();
        config.OutputFilePath!.Value.Path.ShouldBe(Path.Combine(PathingUtil.DrivePrefix, "some", "path"));
    }

    [Theory, AnalyzerAutoData]
    public void TestOutputFilePathBackwardSlash(
        RunConfig config,
        RunConfigReader sut)
    {
        var line = $@"output_file = {PathingUtil.DrivePrefix}some\path";
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.OutputFilePath.ShouldNotBeNull();
        config.OutputFilePath!.Value.Path.ShouldBe(Path.Combine(PathingUtil.DrivePrefix, "some", "path"));
    }
}
