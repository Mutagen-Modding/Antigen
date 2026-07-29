using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Autofac;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Meta;
using Module = Autofac.Module;

namespace Antigen.Modules;

public class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<LoggingModule>();

        // Register base services
        builder.RegisterType<FileSystem>()
            .As<IFileSystem>()
            .SingleInstance();

        builder.RegisterModule<MutagenModule>();

        builder.RegisterModule<SkyrimModule>();

        builder.RegisterModule<SkyrimAnalyzerModule>();

        builder.Register(context =>
        {
            var gameReleaseContext = context.Resolve<IGameReleaseContext>();
            return GameConstants.Get(gameReleaseContext.Release);
        });

        RegisterMarkedTypes(builder, typeof(App).Assembly);
    }

    private void RegisterMarkedTypes(ContainerBuilder builder, Assembly assembly)
    {
        builder.RegisterAssemblyTypes(assembly)
            .AssignableTo<ISingleton>()
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterAssemblyTypes(assembly)
            .AssignableTo<ITransient>()
            .AsSelf()
            .AsImplementedInterfaces();
    }
}
