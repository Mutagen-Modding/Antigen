using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Reporting.Handlers;

public class AnalyzerResult
{
    public required Topic Topic { get; init; }
    public required IFormLinkIdentifier? Record { get; init; }
    public required ModKey? ModKey { get; init; }
}
