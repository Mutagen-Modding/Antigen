using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.Testing;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.Testing.Extensions;
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
            .ShouldEqual(TestIsolatedRecordAnalyzer.HasHeight.Id);
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
        var env = new TestGameEnvironment()
        {
            GameRelease = GameRelease.SkyrimSE,
            LinkCache = mod.ToImmutableLinkCache(),
            DataFolderPath = existingDataDir,
            CreationClubListingsFilePath = null,
            LoadOrderFilePath = "",
            LoadOrder = new LoadOrder<IModListingGetter<IModGetter>>([
                new ModListing<IModGetter>(mod)
            ]),
            AssetProvider = null!,
        };
        builder.RegisterInstance(new TestGameEnvironmentProvider(env)).AsImplementedInterfaces();
        builder.RegisterInstance(env).AsImplementedInterfaces();
        var container = builder.Build();
        var sut = container.Resolve<ContextualEngine>();
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
            .ShouldEqual(TestIsolatedRecordAnalyzer.HasHeight.Id);
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
        var env = new TestGameEnvironment()
        {
            GameRelease = GameRelease.SkyrimSE,
            LinkCache = mod.ToImmutableLinkCache(),
            DataFolderPath = existingDataDir,
            CreationClubListingsFilePath = null,
            LoadOrderFilePath = "",
            LoadOrder = new LoadOrder<IModListingGetter<IModGetter>>([
                new ModListing<IModGetter>(mod)
            ]),
            AssetProvider = null!,
        };
        builder.RegisterInstance(new TestGameEnvironmentProvider(env)).AsImplementedInterfaces();
        builder.RegisterInstance(env).AsImplementedInterfaces();
        var container = builder.Build();
        var sut = container.Resolve<ContextualEngine>();
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
            .ShouldEqual(TestContextualRecordAnalyzer.HasHeight.Id);
    }
}
