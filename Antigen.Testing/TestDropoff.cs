using Antigen.SDK.Drops;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Testing;

public class TestDropoff : IReportDropbox
{
    private readonly List<Topic> _reports = new();
    public IReadOnlyList<Topic> Reports => _reports;

    public void ClearReports()
    {
        _reports.Clear();
    }

    public void Dropoff(ReportContextParameters parameters, ModKey mod, IFormLinkIdentifier record, Topic topic)
    {
        _reports.Add(topic);
    }

    public void Dropoff(ReportContextParameters parameters, Topic topic)
    {
        _reports.Add(topic);
    }
}
