using System.IO.Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Cli.Args;
using Mutagen.Bethesda.Analyzers.Cli.Modules;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Cli;

public static class RunAnalyzers
{
    public static async Task<int> Run(RunAnalyzersCommand command)
    {
        var container = GetContainer(command);

        var engine = container.Resolve<GameEnvironmentAnalyzerEngine>();
        var consumer = container.Resolve<IWorkConsumer>();

        PrintTopics(command, engine);

        consumer.Start();
        await engine.Run(CancellationToken.None);

        return 0;
    }

    private static void PrintTopics(RunAnalyzersCommand command, GameEnvironmentAnalyzerEngine engine)
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

        builder.RegisterModule<RunAnalyzerModule>();
        builder.RegisterModule(new AnalyzerCommandModule(command));

        builder.RegisterModule<SkyrimAnalyzerModule>();

        return builder.Build();
    }
}
