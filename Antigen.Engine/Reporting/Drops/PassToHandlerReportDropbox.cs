using Antigen.Reporting.Handlers;
using Antigen.SDK.Drops;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Reporting.Drops;

public class PassToHandlerReportDropbox : IReportDropbox
{
    private readonly IReportHandler[] _handlers;

    public PassToHandlerReportDropbox(IReportHandler[] handlers)
    {
        _handlers = handlers;
    }

    public void Dropoff(
        ReportContextParameters parameters,
        ModKey mod,
        IFormLinkIdentifier record,
        Topic topic)
    {
        foreach (var handler in _handlers)
        {
            handler.Dropoff(parameters, mod, record, topic);
        }
    }

    public void Dropoff(ReportContextParameters parameters, Topic topic)
    {
        foreach (var handler in _handlers)
        {
            handler.Dropoff(parameters, topic);
        }
    }
}
