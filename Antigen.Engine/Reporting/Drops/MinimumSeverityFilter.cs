using Antigen.Config.Topic;
using Antigen.SDK.Drops;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Reporting.Drops;

public class MinimumSeverityFilter : IReportDropbox
{
    private readonly IMinimumSeverityConfiguration _minimum;
    private readonly IReportDropbox _reportDropbox;

    public MinimumSeverityFilter(
        IMinimumSeverityConfiguration minimum,
        IReportDropbox reportDropbox)
    {
        _minimum = minimum;
        _reportDropbox = reportDropbox;
    }

    public void Dropoff(
        ReportContextParameters parameters,
        ModKey mod,
        IFormLinkIdentifier record,
        Topic topic)
    {
        if (topic.Severity < _minimum.MinimumSeverity) return;
        _reportDropbox.Dropoff(parameters, mod, record, topic);
    }

    public void Dropoff(
        ReportContextParameters parameters,
        Topic topic)
    {
        if (topic.Severity < _minimum.MinimumSeverity) return;
        _reportDropbox.Dropoff(parameters, topic);
    }
}
