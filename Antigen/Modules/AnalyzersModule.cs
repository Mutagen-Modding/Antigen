using Autofac;
using Antigen.Autofac;
using Antigen.Services;
using Noggog.Autofac;

namespace Antigen.Modules;

public class AnalyzersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<MainModule>();

        builder.RegisterAssemblyTypes(typeof(AnalyzerRunner).Assembly)
            .InNamespacesOf(
                typeof(AnalyzerRunner))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
    }
}
