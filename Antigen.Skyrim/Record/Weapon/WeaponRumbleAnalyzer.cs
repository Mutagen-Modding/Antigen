using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Weapon;

public class WeaponRumbleAnalyzer : IIsolatedRecordAnalyzer<IWeaponGetter>
{
    public static readonly TopicDefinition RumbleDurationZero = MutagenTopicBuilder.FromDiscussion(
            662,
            "Rumble Duration Zero",
            Severity.Warning)
        .WithoutFormatting("Rumble duration is zero");

    public static readonly TopicDefinition RumbleMotorStrengthZero = MutagenTopicBuilder.FromDiscussion(
            663,
            "Rumble Motor Strength Zero",
            Severity.Warning)
        .WithoutFormatting("Rumble motor strength is zero for both left and right motors");

    public IEnumerable<TopicDefinition> Topics { get; } = [RumbleDurationZero, RumbleMotorStrengthZero];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IWeaponGetter> param)
    {
        var weapon = param.Record;

        if (weapon.Data is { RumbleDuration: var rumbleDuration, RumbleLeftMotorStrength: var rumbleLeftMotorStrength, RumbleRightMotorStrength: var rumbleRightMotorStrength })
        {
            if (rumbleDuration == 0)
            {
                param.AddTopic(RumbleDurationZero.Format());
            }

            if (rumbleLeftMotorStrength == 0 && rumbleRightMotorStrength == 0)
            {
                param.AddTopic(RumbleMotorStrengthZero.Format());
            }
        }
    }

    public IEnumerable<Func<IWeaponGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Data?.RumbleDuration;
        yield return x => x.Data?.RumbleLeftMotorStrength;
        yield return x => x.Data?.RumbleRightMotorStrength;
    }
}
