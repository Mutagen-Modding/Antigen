using Autofac;
using Antigen.SDK;
using Antigen.Skyrim.Caches;
using Noggog.Autofac;
using Module = Autofac.Module;

namespace Antigen.Skyrim;

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
