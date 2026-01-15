using System.IO.Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Cli.Args;
using Mutagen.Bethesda.Analyzers.Cli.Modules;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.WorkEngine;
using IContainer = Autofac.IContainer;

namespace Mutagen.Bethesda.Analyzers.Cli;

public static class RunAnalyzers
{
    public static async Task<int> Run(RunAnalyzersCommand command)
    {
        var container = GetContainer(command);

        var engine = container.Resolve<ContextualAnalyzerEngine>();
        var consumer = container.Resolve<IWorkConsumer>();

        PrintTopics(command, engine);

        consumer.Start();
        await engine.Run(CancellationToken.None);

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
        builder.RegisterInstance(new FileSystem()).As<IFileSystem>();

        IGameEnvironment gameEnvironment;
        if (command.LoadOrder is null)
        {
            gameEnvironment = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);
        }
        else
        {
            var loadOrder = command.LoadOrder.Split(',')
                .Select(x => x.Trim())
                .Select(x => ModKey.FromFileName(x));

            gameEnvironment = GameEnvironmentBuilder.Create(GameRelease.SkyrimSE)
                .WithLoadOrder(loadOrder.ToArray())
                .Build();
        }

        builder
            .RegisterInstance(gameEnvironment.LoadOrder)
            .As<ILoadOrderGetter<IModListingGetter<IModGetter>>>();

        builder
            .RegisterInstance(gameEnvironment.LinkCache)
            .AsImplementedInterfaces();

        var workDropoff = new WorkDropoff();
        builder
            .RegisterInstance(workDropoff)
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(new WorkConsumer(0, workDropoff, workDropoff))
            .AsImplementedInterfaces();

        builder.RegisterInstance(new NumWorkThreadsUnopinionated())
            .AsImplementedInterfaces();

        builder.RegisterModule<RunAnalyzerModule>();
        builder.RegisterModule(new AnalyzerCommandModule(command));

        DynamicAnalyzerModuleLoader.LoadAnalyzerModule(builder, command.GameRelease);

        return builder.Build();
    }
}
