using System.Collections;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers.Reporting.Drops;

public sealed class EditorIdEnricher : IReportDropbox
{
    private readonly IReportDropbox _reportDropbox;

    public EditorIdEnricher(
        IReportDropbox reportDropbox)
    {
        _reportDropbox = reportDropbox;
    }

    public void Dropoff(
        ReportContextParameters parameters,
        ModKey mod,
        IFormLinkIdentifier record,
        Topic topic)
    {
        _reportDropbox.Dropoff(parameters, mod, record, Enrich(parameters, topic));
    }

    public void Dropoff(
        ReportContextParameters parameters,
        Topic topic)
    {
        _reportDropbox.Dropoff(parameters, Enrich(parameters, topic));
    }

    private Topic Enrich(
        ReportContextParameters parameters,
        Topic topic)
    {
        return topic with
        {
            FormattedTopic = topic.FormattedTopic.Transform(parameters, Selector),
            MetaData = Enrich(parameters, topic.MetaData)
        };
    }

    private bool IsEnrichTarget(object obj)
    {
        switch (obj)
        {
            case IReadOnlyCollection<IFormLinkGetter> coll:
                return coll.Count > 0;
            case IDictionary dict:
                if (dict.Count == 0) return false;
                foreach (dynamic dictItem in dict)
                {
                    if (IsEnrichTarget(dictItem.Key)) return true;
                    if (IsEnrichTarget(dictItem.Value)) return true;
                    return false;
                }
                return false;
            case IFormLinkGetter:
            case IEnumerable<IFormLinkGetter>:
                return true;
            case IEnumerable enumerable:
                return enumerable.Cast<object>().Any(IsEnrichTarget);
            default:
                return false;
        }
    }

    private bool HasEnrichTargets((string Name, object Value)[] metaData)
    {
        return metaData.Select(x => x.Value)
            .Any(IsEnrichTarget);
    }

    private object EnrichItem(
        ReportContextParameters parameters,
        object item)
    {
        switch (item)
        {
            case IFormLinkGetter link:
                return LinkResolver(parameters, link);
            case IDictionary dictionary:
                return dictionary.Keys
                    .Cast<object>()
                    .ToDictionary(key =>
                    {
                        if (key is IFormLinkGetter keyLink)
                        {
                            return LinkResolver(parameters, keyLink);
                        }
                        return EnrichItem(parameters, key) ?? key;
                    }, key =>
                    {
                        var val = dictionary[key];
                        if (val is IFormLinkGetter valueLink)
                        {
                            return LinkResolver(parameters, valueLink);
                        }
                        return EnrichItem(parameters, dictionary[key]!);
                    });
            case IEnumerable<IFormLinkGetter> enumerable:
                return enumerable
                    .Select(e => LinkResolver(parameters, e))
                    .ToArray();
            case IEnumerable enumerable:
                var array = enumerable as object[] ?? enumerable.Cast<object>().ToArray();
                return array.Any(IsEnrichTarget)
                    ? array
                        .Select(x => x switch
                        {
                            IFormLinkGetter link => LinkResolver(parameters, link),
                            _ => EnrichItem(parameters, x)
                        })
                        .ToArray()
                    : item;
            default:
                return item;
        }
    }

    private (string Name, object Value)[] Enrich(
        ReportContextParameters parameters,
        (string Name, object Value)[] metaData)
    {
        if (!HasEnrichTargets(metaData)) return metaData;
        return metaData.Select(item =>
        {
            return (item.Name, EnrichItem(parameters, item.Value));
        }).ToArray();
    }

    private object LinkResolver(
        ReportContextParameters parameters,
        IFormLinkGetter link)
    {
        if (parameters.LinkCache.TryResolveIdentifier(link.FormKey, link.Type, out var edid))
        {
            return new MajorRecordIdentifier()
            {
                FormKey = link.FormKey,
                EditorID = edid
            };
        }

        return link;
    }

    private object? Selector(
        ReportContextParameters parameters,
        object? item)
    {
        if (item is IFormLinkGetter link)
        {
            return LinkResolver(parameters, link);
        }
        return item;
    }
}
