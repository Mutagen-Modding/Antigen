using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public sealed class RedundantPackageAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static TopicDefinition<IPackageGetter> RedundantPackage = MutagenTopicBuilder.FromDiscussion(
            474,
            "Npc Uses Redundant Package",
            Severity.Warning)
        .WithFormatting<IPackageGetter>("Npc has package {0} which never plays as higher priority packages take priority at all times");

    public IEnumerable<TopicDefinition> Topics { get; } = [RedundantPackage];

    private readonly struct Time
    {
        public int Hour { get; init; }
        public int Minute { get; init; }
    }

    private readonly struct PackageTimeSpan
    {
        public IPackageGetter Package { get; init; }
        public Time Start { get; init; }
        public Time End { get; init; }
        public bool ExtendsToNextDay { get; init; }

        public bool IsSubsumedBy(PackageTimeSpan other)
        {
            if (ExtendsToNextDay)
            {
                if (other.ExtendsToNextDay)
                {
                    return Start.Hour >= other.Start.Hour && Start.Minute >= other.Start.Minute &&
                           End.Hour <= other.End.Hour && End.Minute <= other.End.Minute;
                }

                return false;
            }

            if (other.ExtendsToNextDay)
            {
                return Start.Hour >= other.Start.Hour && Start.Minute >= other.Start.Minute;
            }

            return Start.Hour >= other.Start.Hour && Start.Minute >= other.Start.Minute &&
                   End.Hour <= other.End.Hour && End.Minute <= other.End.Minute;
        }
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (npc.IsDeleted) return;

        var timespans = new List<PackageTimeSpan>();
        foreach (var packageLink in npc.Packages)
        {
            var package = packageLink.TryResolve(param.LinkCache);
            if (package is null) continue;

            var hours = package.ScheduleHour == -1 ? Enumerable.Range(0, 24).ToArray() : [package.ScheduleHour];
            var minutes = package.ScheduleMinute == -1 ? package.ScheduleHour == -1 ? Enumerable.Range(0, 60).ToArray() : [0] : [package.ScheduleMinute];

            var packageTimespans = new List<PackageTimeSpan>();
            foreach (var hour in hours)
            {
                foreach (var minute in minutes)
                {
                    packageTimespans.Add(new PackageTimeSpan
                    {
                        Package = package,
                        Start = new Time
                        {
                            Hour = hour,
                            Minute = minute,
                        },
                        End = new Time
                        {
                            Hour = hour + (package.ScheduleDurationInMinutes - 1) / 60 % 24,
                            Minute = minute + (package.ScheduleDurationInMinutes - 1) % 60,
                        },
                        ExtendsToNextDay = (hour * 60 + minute + package.ScheduleDurationInMinutes - 1) / 60 >= 24
                    });
                }
            }

            // Check if the new package is subsumed by the existing packages
            if (packageTimespans.All(packageTimespan => timespans.Any(packageTimespan.IsSubsumedBy)))
            {
                param.AddTopic(
                    RedundantPackage.Format(package));
            }
            else
            {
                // Skip packages with conditions as they are not guaranteed to run all the time
                if (package.Conditions.Count > 0) continue;

                // Skip evaluation for packages that are not running every day
                if (package.ScheduleMonth != -1) continue;
                if (package.ScheduleDate != 0) continue;
                if (package.ScheduleDayOfWeek != (Bethesda.Skyrim.Package.DayOfWeek)255) continue;

                timespans.AddRange(packageTimespans);
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Packages;
    }
}
