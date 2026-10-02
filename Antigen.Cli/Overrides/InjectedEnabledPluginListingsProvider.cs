using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Order.DI;

namespace Antigen.Cli.Overrides;

public class InjectedEnabledPluginListingsProvider(IEnumerable<ModKey> modKeys) : IEnabledPluginListingsProvider
{
    public IEnumerable<ILoadOrderListingGetter> Get()
    {
        foreach (var modKey in modKeys)
        {
            yield return new LoadOrderListing(modKey, true);
        }
    }
}
