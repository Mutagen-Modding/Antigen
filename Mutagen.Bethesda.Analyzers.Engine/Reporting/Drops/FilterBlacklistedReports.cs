using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Reporting.Drops;

public class FilterBlacklistedReports(IReportDropbox dropbox, IBlacklistedModsProvider blacklistedModsProvider) : IReportDropbox
{
    public void Dropoff(ReportContextParameters parameters, ModKey mod, IFormLinkIdentifier record, Topic topic)
    {
        if (blacklistedModsProvider.IsBlacklisted(mod))
        {
            return;
        }

        dropbox.Dropoff(parameters, mod, record, topic);
    }

    public void Dropoff(ReportContextParameters parameters, Topic topic)
    {
        dropbox.Dropoff(parameters, topic);
    }
}
