using Antigen.Resources.Converter;
using Antigen.Services;
using Autofac;
using Autofac.Builder;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Modules;

public abstract class GameSpecificModule<TMod, TModGetter> : Module
    where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
    where TModGetter : class, IContextGetterMod<TMod, TModGetter>
{

    protected abstract GameRelease GameRelease { get; }

    protected abstract IReg<IModInfoProvider<TModGetter>> ModInfoProvider { get; }
    protected abstract IReg<IFormattedTopicConverters> FormattedTopicConverters { get; }
    protected abstract IReg<IAnalyzerResultInfoFactory> AnalyzerResultInfoFactory { get; }
    protected abstract IReg<IAnalyzerFilter> AnalyzerFilter { get; }

    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterInstance(new GameReleaseInjection(GameRelease))
            .SingleInstance()
            .AsImplementedInterfaces();

        Register(builder, ModInfoProvider)
            .As<IModInfoProvider>()
            .As<IModInfoProvider<TModGetter>>();

        Register(builder, FormattedTopicConverters)
            .As<IFormattedTopicConverters>();

        Register(builder, AnalyzerResultInfoFactory)
            .As<IAnalyzerResultInfoFactory>();

        Register(builder, AnalyzerFilter)
            .As<IAnalyzerFilter>();
    }

    private static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        Register<T>(ContainerBuilder builder, IReg<T> x)
        where T : notnull
    {
        return builder.RegisterType(x.GetType().GenericTypeArguments[2]);
    }
    protected static IReg<T> Register<T>()
    {
        return new Reg<T>();
    }

    protected interface IReg<out T>;
    private sealed class Reg<T> : IReg<T>;
}