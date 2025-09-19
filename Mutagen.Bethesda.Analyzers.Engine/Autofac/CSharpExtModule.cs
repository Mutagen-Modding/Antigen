using Autofac;
using Noggog.Autofac;
using Noggog.DotNetCli.DI;
using Noggog.IO;
using Noggog.Processes.DI;
using Noggog.Reactive;
using Noggog.Time;

namespace Mutagen.Bethesda.Analyzers.Autofac;

public class CSharpExtModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ToDo
        // Import official noggog module when it doesn't include WorkEngine anymore
        builder.RegisterAssemblyTypes(typeof(IDeepCopyDirectory).Assembly)
            .InNamespacesOf(
                typeof(IDeleteEntireDirectory),
                typeof(INowProvider),
                typeof(IProcessRunner),
                typeof(IQueryNugetListing),
                typeof(IWatchFile))
            .Except<TempFile>()
            .Except<TempFolder>()
            .NotInjection()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}
