using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class HasAmmoAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<IWeaponGetter, string> MissingAmmo = MutagenTopicBuilder.FromDiscussion(
            405,
            "Npc is missing ammo",
            Severity.Error)
        .WithFormatting<IWeaponGetter, string>("Npc has ranged weapon {0} but no {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingAmmo];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (npc.Items is null) return;

        IWeaponGetter? crossbow = null;
        IWeaponGetter? bow = null;
        IAmmunitionGetter? arrow = null;
        IAmmunitionGetter? bolt = null;
        foreach (var entry in npc.Items)
        {
            var item = entry.Item.Item.TryResolve(param.LinkCache);

            var weapon = item?.FindItem<IWeaponGetter>(param.LinkCache, w =>
            {
                if (w.Data is null) return false;

                return w.Data.AnimationType switch
                {
                    WeaponAnimationType.Bow or WeaponAnimationType.Crossbow => true,
                    _ => false
                };
            });

            if (weapon != null)
            {
                if (weapon.Data is null) break;

                if (weapon.Data.AnimationType == WeaponAnimationType.Bow)
                {
                    bow = weapon;
                }
                else if (weapon.Data.AnimationType == WeaponAnimationType.Crossbow)
                {
                    crossbow = weapon;
                }
            }

            var ammo = item?.FindItem<IAmmunitionGetter>(param.LinkCache, _ => true);

            if (ammo != null)
            {
                if (ammo.Flags.HasFlag(Ammunition.Flag.NonBolt))
                {
                    arrow = ammo;
                }
                else
                {
                    bolt = ammo;
                }
            }
        }

        if (crossbow is not null && bolt is null)
        {
            param.AddTopic(MissingAmmo.Format(crossbow, "bolts"));
        }

        if (bow is not null && arrow is null)
        {
            param.AddTopic(MissingAmmo.Format(bow, "arrows"));
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Items;
    }
}
