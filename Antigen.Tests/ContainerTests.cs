using Autofac;
using Antigen.Engines;
using Antigen.Testing;
using Xunit;

namespace Antigen.Tests;

public class ContainerTests
{
    [Fact]
    public void ResolvesIsolatedEngine()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<TestModule>();
        var container = builder.Build();
        container.Resolve<IsolatedEngine>();
    }

    [Fact]
    public void ResolvesContextualEngine()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<TestModule>();
        var container = builder.Build();

        container.Resolve<ContextualAnalyzerEngine>();
    }
}
