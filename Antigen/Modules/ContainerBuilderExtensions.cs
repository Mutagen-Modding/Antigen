using Autofac;

namespace Antigen.Modules;

public enum RegistrationStyle
{
    Singleton,
    Transient
}

public static class ContainerBuilderExtensions
{
    public static void RegisterFolder<TPrototype>(this ContainerBuilder builder, RegistrationStyle style)
    {
        var registration = builder.RegisterAssemblyTypes(typeof(TPrototype).Assembly)
            .InNamespaceOf<TPrototype>()
            .AsImplementedInterfaces()
            .AsSelf();

        if (style == RegistrationStyle.Singleton)
        {
            registration.SingleInstance();
        }
    }
}
