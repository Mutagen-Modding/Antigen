using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Placed;

using Fixture = ContextualRecordTestFixture<PersistenceAnalyzer, PlacedObject, IPlacedGetter>;

public class PersistenceAnalyzerTest
{
    static void SetPersistent(PlacedObject rec, ISkyrimMod? _mod = null)
    {
        rec.SkyrimMajorRecordFlags |= (SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.Persistent;
    }
    static void SetTemporary(PlacedObject rec, ISkyrimMod? _mod = null)
    {
        rec.SkyrimMajorRecordFlags &= ~(SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.Persistent;
    }

    [Theory, MutagenModAutoData]
    public void ReferencedElsewhere(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var quest = mod.Quests.AddNew();
                quest.Aliases.Add(new()
                {
                    ForcedReference = rec.ToNullableLink(),
                });
            },
            prepForFix: SetPersistent,
            PersistenceAnalyzer.NotPersistent);
    }

    [Theory, MutagenModAutoData]
    public void ReferencedLocation(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                SetPersistent(rec);
                var loc = mod.Locations.AddNew();
                loc.LocationRefTypeReferencesAdded = [new()
                {
                    Ref = rec.ToNullableLink()
                }];
            },
            prepForFix: SetTemporary,
            PersistenceAnalyzer.UnnecessaryPersistence);
    }

    [Theory, MutagenModAutoData]
    public void ReferencedLocationHorseMarker(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var loc = mod.Locations.AddNew();
                loc.HorseMarkerRef.SetTo(rec);
            },
            prepForFix: SetPersistent,
            PersistenceAnalyzer.NotPersistent);
    }

    [Theory, MutagenModAutoData]
    public void ReferencedLargeRefs(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                SetPersistent(rec);
                var world = mod.Worldspaces.AddNew();
                world.LargeReferences.Add(new()
                {
                    References = [new() { Reference = rec.ToNullableLink()}]
                });
            },
            prepForFix: SetTemporary,
            PersistenceAnalyzer.UnnecessaryPersistence);
    }

    [Theory, MutagenModAutoData]
    public void PersistAll(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.PersistentLocation.SetTo(FormKeys.SkyrimSE.Skyrim.Location.PersistAll);
            },
            prepForFix: SetPersistent,
            PersistenceAnalyzer.NotPersistent);
    }

    [Theory, MutagenModAutoData]
    public void FullLod(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.SkyrimMajorRecordFlags |= (SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.IsFullLod;
            },
            prepForFix: SetPersistent,
            PersistenceAnalyzer.NotPersistent);
    }

    [Theory, MutagenModAutoData]
    public void NeverFade(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                SetPersistent(rec);
                var light = mod.Lights.AddNew();
                rec.Base.SetTo(light);
                rec.SkyrimMajorRecordFlags |= (SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.LightMajorFlag.NeverFades;
            },
            prepForFix: SetTemporary,
            PersistenceAnalyzer.UnnecessaryPersistence);
    }

    [Theory, MutagenModAutoData]
    public void MarkerBase(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.Base.SetTo(FormKeys.SkyrimSE.Skyrim.Static.MapMarker);
            },
            prepForFix: SetPersistent,
            PersistenceAnalyzer.NotPersistent);
    }

    [Theory, MutagenModAutoData]
    public void WaterBase(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var water = mod.Activators.AddNew();
                water.WaterType.SetTo(FormKeys.SkyrimSE.Skyrim.Water.DefaultWater);
                rec.Base.SetTo(water);
            },
            prepForFix: SetPersistent,
            PersistenceAnalyzer.NotPersistent);
    }

    [Theory, MutagenModAutoData]
    public void DecalBase(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var decal = mod.TextureSets.AddNew();
                rec.Base.SetTo(decal);
            },
            prepForFix: SetPersistent,
            PersistenceAnalyzer.NotPersistent);
    }
}
