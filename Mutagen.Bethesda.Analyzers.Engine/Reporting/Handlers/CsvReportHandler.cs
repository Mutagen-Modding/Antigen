using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Reporting.Handlers;

public record CsvInputs(string OutputFilePath);

public class CsvReportHandler : IReportHandler
{
    private readonly IWorkDropoff _workDropoff;
    private readonly CsvInputs _inputs;
    private readonly ILinkCache _linkCache;
    private readonly Lock _lock = new();

    public CsvReportHandler(
        CsvInputs inputs,
        ILinkCache linkCache,
        IWorkDropoff workDropoff)
    {
        _inputs = inputs;
        _linkCache = linkCache;
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

    private string BuildLine(Topic topic, ModKey? sourceMod, IFormLinkIdentifier? majorRecord)
    {
        string? editorId = null;
        IMajorRecordGetter? parent = null;
        if (majorRecord is not null)
        {
            _linkCache.TryResolveIdentifier(majorRecord, out editorId);
            if (_linkCache.TryResolveSimpleContext(majorRecord, out var parentContext) && parentContext.Parent?.Record is IMajorRecordGetter parentRecord)
            {
                parent = parentRecord;
            }
        }

        var baseLine = $"""
            "{topic.TopicDefinition.Id}","{topic.TopicDefinition.Severity}","{topic.TopicDefinition.Title}","{sourceMod?.ToString()}","{majorRecord?.FormKey.ToString()}","{editorId}","{parent?.FormKey} {parent?.EditorID}","{topic.FormattedTopic.FormattedMessage}"
            """;

        if (topic.MetaData.Length > 0)
        {
            baseLine += ",\"" + string.Join("\n", topic.MetaData.Select(x => x.Name + ": " + ReportUtility.GetStringValue(x.Value))) + "\"";
        }

        return baseLine;
    }

    private static string BuildLineFilePath(Topic topic, string filePath)
    {
        var baseLine = $"""
            "{topic.TopicDefinition.Id}","{topic.TopicDefinition.Severity}","{topic.TopicDefinition.Title}","","{filePath}","","{topic.FormattedTopic.FormattedMessage}"
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
