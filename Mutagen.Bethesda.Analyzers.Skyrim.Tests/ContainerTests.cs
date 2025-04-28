using Autofac;
using Mutagen.Bethesda.Analyzers.Drivers;
using Mutagen.Bethesda.Analyzers.Drivers.Records;
using Mutagen.Bethesda.Analyzers.Testing;
using Mutagen.Bethesda.Environments.DI;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests;

public class ContainerTests
{
    [Fact]
    public void ResolvesMajorRecordDriver()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<TestModule>();
        builder.RegisterInstance(new GameReleaseInjection(GameRelease.SkyrimSE)).AsImplementedInterfaces();
        var container = builder.Build();

        var drivers = container.Resolve<IIsolatedDriver[]>();
        drivers
            .Any(x => x.GetType().IsGenericType && typeof(ByGenericTypeRecordIsolatedDriver<>).IsAssignableFrom(x.GetType().GetGenericTypeDefinition()))
            .ShouldBeTrue();

        var contextualDrivers = container.Resolve<IContextualDriver[]>();
        contextualDrivers
            .Any(x => x.GetType().IsGenericType && typeof(ByGenericTypeRecordContextualDriver<>).IsAssignableFrom(x.GetType().GetGenericTypeDefinition()))
            .ShouldBeTrue();
    }
}
