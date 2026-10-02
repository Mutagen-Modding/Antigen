using Autofac;
using Antigen.Drivers;
using Antigen.Engines;
using Mutagen.Bethesda.Autofac;
using Noggog.Autofac;
using Noggog.Autofac.Modules;

namespace Antigen.Autofac;

public class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<CSharpExtModule>();
        builder.RegisterModule<MutagenModule>();
        builder.RegisterModule<ReflectionDriverModule>();
        builder.RegisterModule<HandlerModule>();
        builder.RegisterAssemblyTypes(typeof(IsolatedEngine).Assembly)
            .InNamespacesOf(
                typeof(ContextualAnalyzerEngine))
            .AsImplementedInterfaces()
            .AsSelf()
            .InstancePerLifetimeScope();
        builder.RegisterType<ContextualDriver>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(InjectionDriverProvider<>))
            .As(typeof(IDriverProvider<>))
            .InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(FilteredAnalyzerProvider<>))
            .As(typeof(IAnalyzerProvider<>))
            .InstancePerLifetimeScope();
    }
}
