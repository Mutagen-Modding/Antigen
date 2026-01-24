using Autofac;
using Mutagen.Bethesda.Analyzers.SDK;
using Mutagen.Bethesda.Analyzers.Skyrim.Caches;
using Noggog.Autofac;
using Module = Autofac.Module;

namespace Mutagen.Bethesda.Analyzers.Skyrim;

public class SkyrimCacheModule : Module, ICacheModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(VoiceTypeAssetLookupProvider).Assembly)
            .InNamespacesOf(typeof(VoiceTypeAssetLookupProvider))
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}
