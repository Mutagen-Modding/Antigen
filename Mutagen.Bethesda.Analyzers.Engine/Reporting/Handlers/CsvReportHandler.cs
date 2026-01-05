using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Reporting.Handlers;

public record CsvInputs(string OutputFilePath);

public class CsvReportHandler : IReportHandler
{
    private readonly IWorkDropoff _workDropoff;
    private readonly CsvInputs _inputs;
    private readonly Lock _lock = new();

    public CsvReportHandler(
        CsvInputs inputs,
        IWorkDropoff workDropoff)
    {
        _inputs = inputs;
        _workDropoff = workDropoff;
    }

    public void Dropoff(
        ReportContextParameters parameters,
        ModKey sourceMod,
        IFormLinkIdentifier majorRecord,
        Topic topic)
    {
        _workDropoff.Enqueue(() =>
        {
            Append(BuildLine(topic, sourceMod, majorRecord));
        });
    }

    public void Dropoff(
        ReportContextParameters parameters,
        Topic topic)
    {
        _workDropoff.Enqueue(() =>
        {
            Append(BuildLine(topic, null, null));
        });
    }

    private static string BuildLine(Topic topic, ModKey? sourceMod, IMajorRecordIdentifierGetter? majorRecord)
    {
        var baseLine = $"""
            "{topic.TopicDefinition.Id}","{topic.TopicDefinition.Severity}","{topic.TopicDefinition.Title}","{sourceMod?.ToString()}","{majorRecord?.FormKey.ToString()}","{majorRecord?.EditorID}","{topic.FormattedTopic.FormattedMessage}"
            """;

        if (topic.MetaData.Length > 0)
        {
            baseLine += ",\"" + string.Join("\n", topic.MetaData.Select(x => x.Name + ": " + ReportUtility.GetStringValue(x.Value))) + "\"";
        }

        return baseLine;
    }

    private void Append(string line)
    {
        lock (_lock)
        {
            using var writer = new StreamWriter(_inputs.OutputFilePath, true);
            writer.WriteLine(line);
        }
    }
}
