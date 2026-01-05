using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Reporting.Handlers;

public class ConsoleReportHandler : IReportHandler
{
    private readonly IWorkDropoff _workDropoff;
    private readonly ILinkCache _linkCache;
    private readonly IRunConfigLookup _runConfig;

    public ConsoleReportHandler(
        IWorkDropoff workDropoff,
        ILinkCache linkCache,
        IRunConfigLookup runConfig)
    {
        _workDropoff = workDropoff;
        _linkCache = linkCache;
        _runConfig = runConfig;
    }

    public void Dropoff(
        ReportContextParameters parameters,
        ModKey sourceMod,
        IFormLinkIdentifier majorRecord,
        Topic topic)
    {
        _workDropoff.Enqueue(() =>
        {
            IMajorRecordGetter? parent = null;
            _linkCache.TryResolveIdentifier(majorRecord, out var editorId);
            if (_linkCache.TryResolveSimpleContext(majorRecord, out var parentContext) && parentContext?.Parent?.Record is IMajorRecordGetter parentRecord)
            {
                parent = parentRecord;
            }

            string parentText;
            if (parent is null)
            {
                parentText = string.Empty;
            }
            else
            {
                _linkCache.TryResolveIdentifier(majorRecord, out var parentEditorId);
                parentText = $" (Parent: {parent.FormKey.ToString()} {parentEditorId})";
            }

            Console.WriteLine($"""
                {topic.TopicDefinition}
                   {sourceMod.ToString()} -> {majorRecord.FormKey.ToString()} {majorRecord.EditorID}
                   {topic.FormattedTopic.FormattedMessage}
                """);

            PrintMetadata(topic);
        });
    }

    public void Dropoff(
        ReportContextParameters parameters,
        Topic topic)
    {
        _workDropoff.Enqueue(() =>
        {
            Console.WriteLine($"""
                {topic.TopicDefinition}
                   {topic.FormattedTopic.FormattedMessage}
                """);
        });
    }

    private void PrintMetadata(Topic topic)
    {
        if (!_runConfig.PrintMetadata) return;

        foreach (var meta in topic.MetaData)
        {
            Console.WriteLine($"{ReportUtility.GetStringValue(meta.Name)}: {ReportUtility.GetStringValue(meta.Value)}");
        }
    }
}
