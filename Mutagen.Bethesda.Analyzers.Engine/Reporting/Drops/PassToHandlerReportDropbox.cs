using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Reporting.Drops;

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
