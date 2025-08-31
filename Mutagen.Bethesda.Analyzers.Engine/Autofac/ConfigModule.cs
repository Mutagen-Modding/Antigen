using Autofac;
using Mutagen.Bethesda.Analyzers.Config;
using Mutagen.Bethesda.Analyzers.Engines;
using Noggog.Autofac;

namespace Mutagen.Bethesda.Analyzers.Autofac;

public class ConfigModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(ConfigReader<>))
            .As(typeof(ConfigReader<>));

        builder.RegisterAssemblyTypes(typeof(IsolatedEngine).Assembly)
            .InNamespacesOf(
                typeof(ConfigReader<>))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
    }
}
