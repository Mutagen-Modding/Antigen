using Antigen.SDK.Drops;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Reporting.Handlers;

public interface IReportHandler
{
    void Dropoff(
        ReportContextParameters parameters,
        ModKey sourceMod,
        IFormLinkIdentifier majorRecord,
        Topic topic);

    void Dropoff(
        ReportContextParameters parameters,
        Topic topic);
}
