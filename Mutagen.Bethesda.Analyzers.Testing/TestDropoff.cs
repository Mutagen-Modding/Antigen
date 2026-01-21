using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Testing;

public class TestDropoff : IReportDropbox
{
    private readonly List<Topic> _reports = new();
    public IReadOnlyList<Topic> Reports => _reports;

    public void Dropoff(ReportContextParameters parameters, ModKey mod, IFormLinkIdentifier record, Topic topic)
    {
        _reports.Add(topic);
    }

    public void Dropoff(ReportContextParameters parameters, Topic topic)
    {
        _reports.Add(topic);
    }
}
