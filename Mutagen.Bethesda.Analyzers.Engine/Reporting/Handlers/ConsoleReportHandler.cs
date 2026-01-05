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
    private readonly IRunConfigLookup _runConfig;

    public ConsoleReportHandler(IWorkDropoff workDropoff, IRunConfigLookup runConfig)
    {
        _workDropoff = workDropoff;
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
