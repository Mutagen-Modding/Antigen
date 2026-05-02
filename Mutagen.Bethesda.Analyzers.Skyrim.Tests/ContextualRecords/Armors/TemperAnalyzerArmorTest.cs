using Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Armors;

using Fixture = ContextualRecordTestFixture<TemperAnalyzerArmor, Armor, IArmorGetter>;

public class TemperAnalyzerArmorTest
{
    // Configure armor such that it's expected to have a temper recipe
    // Dummy argument to allow passing to prepForError
    static void ConfigureArmor(Armor rec, ISkyrimMod? _mod = null)
    {
        rec.BodyTemplate ??= new();
        rec.BodyTemplate.ArmorType = ArmorType.LightArmor;
    }

    // Configure a recipe for tempering an armor
    static void ConfigureTemper(ConstructibleObject cobj, Armor rec)
    {
        cobj.WorkbenchKeyword.SetTo(FormKeys.SkyrimSE.Skyrim.Keyword.CraftingSmithingArmorTable);
        cobj.CreatedObject.SetTo(rec);
    }

    [Theory, MutagenModAutoData]
    public void TestNoTemper(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureArmor,
            prepForFix: (rec, mod) =>
            {
                var cobj = fixture.Create<ConstructibleObject>();
                mod.ConstructibleObjects.Add(cobj);
                ConfigureTemper(cobj, rec);
            },
            TemperAnalyzerArmor.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestMultipleTemper(Fixture fixture)
    {
        var cobj1 = fixture.Create<ConstructibleObject>();
        var cobj2 = fixture.Create<ConstructibleObject>();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                ConfigureArmor(rec);

                ConfigureTemper(cobj1, rec);
                mod.ConstructibleObjects.Add(cobj1);
                ConfigureTemper(cobj2, rec);
                mod.ConstructibleObjects.Add(cobj2);
            },
            prepForFix: (rec, mod) =>
            {
                mod.ConstructibleObjects.Remove(cobj2);
            },
            TemperAnalyzerArmor.MultipleTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestTemplated(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureArmor,
            prepForFix: (rec, mod) =>
            {
                rec.TemplateArmor.SetTo(FormKeys.SkyrimSE.Skyrim.Armor.ArmorLeatherCuirass);
            },
            TemperAnalyzerArmor.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestUnplayableOld(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureArmor,
            prepForFix: (rec, mod) =>
            {
                rec.MajorFlags |= Armor.MajorFlag.NonPlayable;
            },
            TemperAnalyzerArmor.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestUnplayableNew(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureArmor,
            prepForFix: (rec, mod) =>
            {
                rec.BodyTemplate!.Flags |= BodyTemplate.Flag.NonPlayable;
            },
            TemperAnalyzerArmor.NoTemper);
    }

    [Theory, MutagenModAutoData]
    public void TestDummy(Fixture fixture)
    {
        fixture.Run(
            prepForError: ConfigureArmor,
            prepForFix: (rec, mod) =>
            {
                rec.Keywords ??= [];
                rec.Keywords.Add(FormKeys.SkyrimSE.Skyrim.Keyword.Dummy);
            },
            TemperAnalyzerArmor.NoTemper);
    }
}
