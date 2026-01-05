using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Reporting.Drops;

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
