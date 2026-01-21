using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.SDK.Drops;

public interface IReportDropbox
{
    void Dropoff(
        ReportContextParameters parameters,
        ModKey mod,
        IFormLinkIdentifier record,
        Topic topic);

    void Dropoff(
        ReportContextParameters parameters,
        Topic topic);
}
