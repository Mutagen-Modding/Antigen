using System.IO.Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Cli.Args;
using Mutagen.Bethesda.Analyzers.Cli.Modules;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.SDK;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Order.DI;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.WorkEngine;
using IContainer = Autofac.IContainer;

namespace Mutagen.Bethesda.Analyzers.Cli;

public static class RunAnalyzers
{
    public static string RunText = "Running analyzers...";

    private record Bootstrap(ContextualAnalyzerEngine Engine, IWorkConsumer WorkConsumer);

    public static async Task<int> Run(RunAnalyzersCommand command)
    {
        var container = GetContainer(command);

        var bootstrap = container.Resolve<Bootstrap>();

        bootstrap.WorkConsumer.Start();

        PrintTopics(command, bootstrap.Engine);

        IntroConstants.PrintTextSplash();
        Console.WriteLine(RunText);
        await bootstrap.Engine.Run(CancellationToken.None);

        return 0;
    }

    private static void PrintTopics(RunAnalyzersCommand command, ContextualAnalyzerEngine engine)
    {
        if (!command.PrintTopics) return;

        Console.WriteLine("Topics:");
        var sb = new StructuredStringBuilder();
        foreach (var topic in engine.Drivers
                     .SelectMany(d => d.Analyzers)
                     .SelectMany(a => a.Topics)
                     .Distinct(x => x.Id))
        {
            topic.Append(sb);
        }

        foreach (var line in sb)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    private static IContainer GetContainer(RunAnalyzersCommand command)
    {
        var services = new ServiceCollection();
        services.AddLogging(x => x.AddConsole());

        var builder = new ContainerBuilder();
        builder.Populate(services);
        builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();

        builder
            .Register(x => x.Resolve<IGameEnvironmentProvider>().Construct())
            .SingleInstance()
            .AsImplementedInterfaces();

        builder
            .Register(x => x.Resolve<IGameEnvironment>().LoadOrder)
            .SingleInstance()
            .AsImplementedInterfaces();

        builder
            .Register(x => x.Resolve<IGameEnvironment>().LinkCache)
            .SingleInstance()
            .AsImplementedInterfaces();

        builder.RegisterModule<RunAnalyzerModule>();
        builder.RegisterModule(new AnalyzerCommandModule(command));

        DynamicModuleLoader.LoadGameModule<IAnalyzerModule>(builder, command.GameRelease);
        DynamicModuleLoader.LoadGameModule<ICacheModule>(builder, command.GameRelease);

        builder.RegisterType<Bootstrap>().AsSelf().SingleInstance();

        return builder.Build();
    }
}
