using Mutagen.Bethesda.Analyzers.Skyrim.Record.Weapon;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Weapons;

using Fixture = ContextualRecordTestFixture<TemperAnalyzerWeapon, Weapon, IWeaponGetter>;

public class TemperAnalyzerWeaponTest
{
    // Configure weapon such that it's expected to have a temper recipe
    // Dummy argument to allow passing to prepForError
    static void ConfigureWeapon(Weapon rec, ISkyrimMod? _mod = null)
    {
        rec.Data ??= new();
        rec.Data.AnimationType = WeaponAnimationType.OneHandSword;
    }

    // Configure a recipe for tempering a weapon
    static void ConfigureTemper(ConstructibleObject cobj, Weapon rec)
    {
        cobj.WorkbenchKeyword.SetTo(FormKeys.SkyrimSE.Skyrim.Keyword.CraftingSmithingSharpeningWheel);
        cobj.CreatedObject.SetTo(rec);
    }

    [Theory, MutagenModAutoData]
    public void TestNoTemper(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureWeapon,
            prepForFix: (rec, mod) =>
            {
                var cobj = fixture.Create<ConstructibleObject>();
                mod.ConstructibleObjects.Add(cobj);
                ConfigureTemper(cobj, rec);
            },
            TemperAnalyzerWeapon.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestMultipleTemper(Fixture fixture)
    {
        var cobj1 = fixture.Create<ConstructibleObject>();
        var cobj2 = fixture.Create<ConstructibleObject>();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                ConfigureWeapon(rec);

                ConfigureTemper(cobj1, rec);
                mod.ConstructibleObjects.Add(cobj1);
                ConfigureTemper(cobj2, rec);
                mod.ConstructibleObjects.Add(cobj2);
            },
            prepForFix: (rec, mod) =>
            {
                mod.ConstructibleObjects.Remove(cobj2);
            },
            TemperAnalyzerWeapon.MultipleTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestTemplated(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureWeapon,
            prepForFix: (rec, mod) =>
            {
                rec.Template.SetTo(FormKeys.SkyrimSE.Skyrim.Weapon.IronSword);
            },
            TemperAnalyzerWeapon.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestUnplayable(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureWeapon,
            prepForFix: (rec, mod) =>
            {
                rec.Data!.Flags |= WeaponData.Flag.NonPlayable;
            },
            TemperAnalyzerWeapon.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestStaff(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureWeapon,
            prepForFix: (rec, mod) =>
            {
                rec.Data!.AnimationType = WeaponAnimationType.Staff;
            },
            TemperAnalyzerWeapon.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestDummy(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureWeapon,
            prepForFix: (rec, mod) =>
            {
                rec.Keywords ??= [];
                rec.Keywords.Add(FormKeys.SkyrimSE.Skyrim.Keyword.Dummy);
            },
            TemperAnalyzerWeapon.NoTemper);
    }
}
