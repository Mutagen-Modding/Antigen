using System.Text.RegularExpressions;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public partial class InconsistentTimeframeAnalyzer : IContextualRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition<int, int> InconsistentHourTopic = MutagenTopicBuilder.FromDiscussion(
            249,
            "Inconsistent Timeframe",
            Severity.Suggestion)
        .WithFormatting<int, int>("Starting hour {0} doesn't match starting hour in the EditorID {1}");

    public static readonly TopicDefinition<int, int> InconsistentDurationTopic = MutagenTopicBuilder.FromDiscussion(
            322,
            "Inconsistent Timeframe",
            Severity.Suggestion)
        .WithFormatting<int, int>("Duration {0} doesn't match duration in the EditorID {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [InconsistentHourTopic, InconsistentDurationTopic];


    [GeneratedRegex(@"(\d+)(?:_(\d+))?x(\d+)(?:_(\d+))?")]
    private static partial Regex TimeframeRegex();

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;

        if (!TryGetEditorIDTimeframe(package, out var hour, out var minutes, out var duration, out var durationMinutes)) return;
        if (package is { ScheduleHour: -1, ScheduleDurationInMinutes: 0 } && hour == 0 && duration % 24 == 0) return;

        if (package.ScheduleHour != (hour % 24) && (minutes == -1 || package.ScheduleMinute != minutes))
        {
            // Try another way by interpreting minutes as hour and omit hour from the consideration
            if (package.ScheduleHour != (minutes % 24))
            {
                param.AddTopic(
                    InconsistentHourTopic.Format(package.ScheduleHour, hour));
            }
        }

        if (package.ScheduleDurationInMinutes / 60 != duration && (durationMinutes != -1 || package.ScheduleDurationInMinutes % 60 != durationMinutes))
        {
            param.AddTopic(
                InconsistentDurationTopic.Format(package.ScheduleDurationInMinutes / 60, duration));
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.ScheduleHour;
        yield return x => x.ScheduleMinute;
        yield return x => x.ScheduleDurationInMinutes;
    }

    private static bool TryGetEditorIDTimeframe(IPackageGetter package, out int hour, out int minutes, out int duration, out int durationMinutes)
    {
        if (package.EditorID is not {} edid)
        {
            hour = 0;
            minutes = -1;
            duration = 0;
            durationMinutes = -1;
            return false;
        }

        var match = TimeframeRegex().Match(edid);
        if (!match.Success || match.Groups.Count < 5)
        {
            hour = 0;
            minutes = -1;
            duration = 0;
            durationMinutes = -1;
            return false;
        }

        hour = Convert.ToInt32(match.Groups[1].Value);
        minutes = match.Groups[2].Value.IsNullOrEmpty() ? -1 : Convert.ToInt32(match.Groups[2].Value);
        duration = Convert.ToInt32(match.Groups[3].Value);
        durationMinutes = match.Groups[4].Value.IsNullOrEmpty() ? -1 : Convert.ToInt32(match.Groups[4].Value);
        return true;
    }
}
