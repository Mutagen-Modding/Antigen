using System.IO.Abstractions;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mutagen.Bethesda.Analyzers.Cli.Modules;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments.DI;
using Noggog;
using Noggog.WorkEngine;
using NSubstitute;

namespace Mutagen.Bethesda.Analyzers.Testing;

public class TestModule : Module
{
    private readonly IFileSystem? _fileSystem;

    public TestModule(IFileSystem? fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public TestModule()
    {
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<RunAnalyzerModule>();
        builder.RegisterInstance(_fileSystem.GetOrDefault())
            .As<IFileSystem>();
        builder.RegisterGeneric(typeof(NullLogger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
        builder.RegisterInstance(new TestDropoff())
            .AsSelf()
            .AsImplementedInterfaces();
        builder.RegisterInstance(new GameReleaseInjection(GameRelease.SkyrimSE))
            .AsImplementedInterfaces();
        var minSev = Substitute.For<IMinimumSeverityConfiguration>();
        minSev.MinimumSeverity.Returns(Severity.Suggestion);
        builder.RegisterInstance(minSev).As<IMinimumSeverityConfiguration>();
        builder.RegisterType<InlineWorkDropoff>().As<IWorkDropoff>();
    }
}
