using Autofac;
using Antigen.Config;
using Antigen.Engines;
using Noggog.Autofac;

namespace Antigen.Autofac;

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
