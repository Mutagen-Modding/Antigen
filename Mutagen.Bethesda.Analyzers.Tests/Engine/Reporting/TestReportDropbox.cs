using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Tests.Engine.Reporting;

public class TestReportDropbox : IReportDropbox
{
    public List<(ReportContextParameters Parameters, Topic Topics)> Dropoffs = new();

    public void Dropoff(ReportContextParameters parameters, ModKey mod, IFormLinkIdentifier record, Topic topic)
    {
        Dropoffs.Add((parameters, topic));
    }

    public void Dropoff(ReportContextParameters parameters, Topic topic)
    {
        Dropoffs.Add((parameters, topic));
    }
}
