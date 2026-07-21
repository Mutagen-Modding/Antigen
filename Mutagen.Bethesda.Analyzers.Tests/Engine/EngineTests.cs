using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.Testing;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.Testing.Extensions;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Engine;

public class EngineTests
{
    [Theory, MutagenModAutoData]
    public async Task IsolatedEngineCallsRecordAnalyzers(
        IFileSystem fileSystem,
        SkyrimMod mod,
        Npc npc,
        DirectoryPath existingDataDir)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new TestModule(fileSystem));
        builder.RegisterType<TestIsolatedRecordAnalyzer>().AsImplementedInterfaces();
        var container = builder.Build();
        var sut = container.Resolve<IsolatedEngine>();
        var dropoff = container.Resolve<TestDropoff>();

        var modPath = Path.Combine(existingDataDir, mod.ModKey.FileName);

        npc.Height = 5;
        mod.BeginWrite
            .ToPath(modPath)
            .WithNoLoadOrder()
            .WithFileSystem(fileSystem)
            .Write();

        await sut.RunOn(modPath, dropoff, CancellationToken.None);

        dropoff.Reports.Select(x => x.TopicDefinition.Id)
            .ShouldBe(new[]
            {
                TestIsolatedRecordAnalyzer.WasRun.Id,
                TestIsolatedRecordAnalyzer.HasHeight.Id,
            }, ignoreOrder: true);
    }

    [Theory, MutagenModAutoData]
    public async Task IsolatedEngineSkipsDeletedRecords(
        IFileSystem fileSystem,
        SkyrimMod mod,
        Npc npc,
        DirectoryPath existingDataDir)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new TestModule(fileSystem));
        builder.RegisterType<TestIsolatedRecordAnalyzer>().AsImplementedInterfaces();
        var container = builder.Build();
        var sut = container.Resolve<IsolatedEngine>();
        var dropoff = container.Resolve<TestDropoff>();

        var modPath = Path.Combine(existingDataDir, mod.ModKey.FileName);

        npc.IsDeleted = true;

        mod.BeginWrite
            .ToPath(modPath)
            .WithNoLoadOrder()
            .WithFileSystem(fileSystem)
            .Write();

        await sut.RunOn(modPath, dropoff, CancellationToken.None);

        dropoff.Reports.ShouldBeEmpty();
    }

    [Theory, MutagenModAutoData]
    public async Task ContextualEngineCallsIsolatedRecordAnalyzers(
        IFileSystem fileSystem,
        SkyrimMod mod,
        Npc npc,
        DirectoryPath existingDataDir)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new TestModule(fileSystem));
        builder.RegisterType<TestIsolatedRecordAnalyzer>().AsImplementedInterfaces();
        builder
            .RegisterInstance(new LoadOrder<IModListingGetter<IModGetter>>([
                new ModListing<IModGetter>(mod)
            ]))
            .AsImplementedInterfaces();
        builder
            .RegisterInstance(new DataDirectoryInjection(existingDataDir))
            .AsImplementedInterfaces();
        builder
            .RegisterInstance(mod.ToImmutableLinkCache())
            .AsImplementedInterfaces();
        var container = builder.Build();
        var sut = container.Resolve<ContextualAnalyzerEngine>();
        var dropoff = container.Resolve<TestDropoff>();

        var modPath = Path.Combine(existingDataDir, mod.ModKey.FileName);

        npc.Height = 5;
        mod.BeginWrite
            .ToPath(modPath)
            .WithNoLoadOrder()
            .WithFileSystem(fileSystem)
            .Write();

        await sut.Run(CancellationToken.None);

        dropoff.Reports.Select(x => x.TopicDefinition.Id)
            .ShouldBe(new[]
            {
                TestIsolatedRecordAnalyzer.WasRun.Id,
                TestIsolatedRecordAnalyzer.HasHeight.Id,
            }, ignoreOrder: true);
    }

    [Theory, MutagenModAutoData]
    public async Task ContextualEngineSkipsDeletedIsolatedRecordAnalyzers(
        IFileSystem fileSystem,
        SkyrimMod mod,
        Npc npc,
        DirectoryPath existingDataDir)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new TestModule(fileSystem));
        builder.RegisterType<TestIsolatedRecordAnalyzer>().AsImplementedInterfaces();
        builder
            .RegisterInstance(new LoadOrder<IModListingGetter<IModGetter>>([
                new ModListing<IModGetter>(mod)
            ]))
            .AsImplementedInterfaces();
        builder
            .RegisterInstance(new DataDirectoryInjection(existingDataDir))
            .AsImplementedInterfaces();
        builder
            .RegisterInstance(mod.ToImmutableLinkCache())
            .AsImplementedInterfaces();
        var container = builder.Build();
        var sut = container.Resolve<ContextualAnalyzerEngine>();
        var dropoff = container.Resolve<TestDropoff>();

        var modPath = Path.Combine(existingDataDir, mod.ModKey.FileName);

        npc.IsDeleted = true;

        mod.BeginWrite
            .ToPath(modPath)
            .WithNoLoadOrder()
            .WithFileSystem(fileSystem)
            .Write();

        await sut.Run(CancellationToken.None);

        dropoff.Reports.ShouldBeEmpty();
    }

    [Theory, MutagenModAutoData]
    public async Task ContextualEngineCallsContextualRecordAnalyzers(
        IFileSystem fileSystem,
        SkyrimMod mod,
        Npc npc,
        DirectoryPath existingDataDir)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new TestModule(fileSystem));
        builder.RegisterType<TestContextualRecordAnalyzer>().AsImplementedInterfaces();
        builder
            .RegisterInstance(new LoadOrder<IModListingGetter<IModGetter>>([
                new ModListing<IModGetter>(mod)
            ]))
            .AsImplementedInterfaces();
        builder
            .RegisterInstance(new DataDirectoryInjection(existingDataDir))
            .AsImplementedInterfaces();
        builder
            .RegisterInstance(mod.ToImmutableLinkCache())
            .AsImplementedInterfaces();
        var container = builder.Build();
        var sut = container.Resolve<ContextualAnalyzerEngine>();
        var dropoff = container.Resolve<TestDropoff>();

        var modPath = Path.Combine(existingDataDir, mod.ModKey.FileName);

        npc.Height = 5;
        mod.BeginWrite
            .ToPath(modPath)
            .WithNoLoadOrder()
            .WithFileSystem(fileSystem)
            .Write();

        await sut.Run(CancellationToken.None);

        dropoff.Reports.Select(x => x.TopicDefinition.Id)
            .ShouldBe(new[] { TestContextualRecordAnalyzer.HasHeight.Id }, ignoreOrder: true);
    }
}
