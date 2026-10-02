using Autofac;
using Antigen.Drivers;
using Antigen.Drivers.Records;
using Antigen.Testing;
using Mutagen.Bethesda.Environments.DI;
using Shouldly;
using Xunit;

namespace Antigen.Skyrim.Tests;

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
