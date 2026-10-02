using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Reporting.Handlers;

public class AnalyzerResult
{
    public required Topic Topic { get; init; }
    public required IFormLinkIdentifier? Record { get; init; }
    public required ModKey? ModKey { get; init; }
}
