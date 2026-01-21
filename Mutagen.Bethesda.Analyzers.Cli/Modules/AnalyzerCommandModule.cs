using System.IO.Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Cli.Args;
using Mutagen.Bethesda.Analyzers.Cli.Overrides;
using Mutagen.Bethesda.Analyzers.Config;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Plugins.Order.DI;
using Mutagen.Bethesda.Strings;
using Noggog;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Cli.Modules;

public class AnalyzerCommandModule(RunAnalyzersCommand command) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(command).AsImplementedInterfaces();
        builder.RegisterInstance(new GameReleaseInjection(command.GameRelease)).AsImplementedInterfaces();
        builder.RegisterType<WorkDropoff>().AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterType<WorkConsumer>().AsImplementedInterfaces().AsSelf()
            .SingleInstance();
        builder.RegisterInstance(new NumWorkThreadsConstant(command.NumThreads)).AsImplementedInterfaces();
        builder.RegisterInstance(GameConstants.Get(command.GameRelease)).As<GameConstants>();

        var runConfig = new RunConfig();
        if (command.RunConfigPath is not null)
        {
            var services = new ServiceCollection();
            services.AddLogging(x => x.AddConsole());

            var tempBuilder = new ContainerBuilder();
            tempBuilder.Populate(services);
            var fileSystem = new FileSystem();
            tempBuilder.RegisterInstance(fileSystem).As<IFileSystem>();
            tempBuilder.RegisterModule<MainModule>();
            tempBuilder.RegisterModule<ConfigModule>();
            tempBuilder.RegisterInstance(new GameReleaseInjection(command.GameRelease))
                .AsImplementedInterfaces();

            var tempContainer = tempBuilder.Build();
            var runConfigReader = tempContainer.Resolve<ConfigReader<IRunConfig>>();
            if (fileSystem.File.Exists(command.RunConfigPath))
            {
                runConfigReader.ReadInto(new FilePath(command.RunConfigPath), runConfig);
            }
        }

        if (command.PrintMetadata.HasValue) runConfig.PrintMetadata = command.PrintMetadata.Value;
        builder.RegisterModule(new RunConfigModule(runConfig));

        if (command.OutputFilePath is not null)
        {
            builder.RegisterType<CsvReportHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new CsvInputs(command.OutputFilePath)).AsSelf().AsImplementedInterfaces();
        }

        if (command.DataFolder is not null)
        {
            var dataDirectoryProvider = new DataDirectoryInjection(command.DataFolder);
            builder.RegisterInstance(dataDirectoryProvider).As<IDataDirectoryProvider>();
        }

        if (command.LoadOrder is not null)
        {
            var loadOrder = command.LoadOrder.Split(',')
                .Select(x => x.Trim())
                .Select(x => ModKey.FromFileName(x));

            builder.RegisterInstance(new InjectedEnabledPluginListingsProvider(loadOrder)).As<IEnabledPluginListingsProvider>();
            builder.RegisterType<NullPluginListingsPathProvider>().As<IPluginListingsPathProvider>();
            builder.RegisterType<NullCreationClubListingsPathProvider>().As<ICreationClubListingsPathProvider>();
        }

        if (command.BlacklistedMods is not null)
        {
            var blacklistedMods = command.BlacklistedMods?.Split(',')
                .Select(x => x.Trim())
                .Select(x => ModKey.FromFileName(x)) ?? [];

            builder.RegisterInstance(new InjectedBlacklistedModsProvider(blacklistedMods)).As<IBlacklistedModsProvider>();
        }
    }
}
