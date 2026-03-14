using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public class OncePerDayFlagAnalyzer : IIsolatedRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition OncePerDayWithStartTimeOrDuration = MutagenTopicBuilder.FromDiscussion(
            532,
            "Once Per Day Flag with Start Time or Duration",
            Severity.Warning)
        .WithoutFormatting("Package has Once Per Day flag but also has a start time or duration - the flag only works for packages without start time or duration");

    public IEnumerable<TopicDefinition> Topics { get; } = [OncePerDayWithStartTimeOrDuration];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;

        // Check if Once Per Day flag is set
        if (!package.Flags.HasFlag(Bethesda.Skyrim.Package.Flag.OncePerDay)) return;

        // Check if package has a start time (hour or minute is not default)
        var hasStartTime = package.ScheduleHour != -1 || package.ScheduleMinute != -1;

        // Check if package has a duration
        var hasDuration = package.ScheduleDurationInMinutes != 0;

        if (hasStartTime || hasDuration)
        {
            param.AddTopic(
                OncePerDayWithStartTimeOrDuration.Format());
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.ScheduleHour;
        yield return x => x.ScheduleMinute;
        yield return x => x.ScheduleDurationInMinutes;
    }
}
