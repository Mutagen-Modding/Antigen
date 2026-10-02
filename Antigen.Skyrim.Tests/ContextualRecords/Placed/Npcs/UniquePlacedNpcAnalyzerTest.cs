using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Placed.Npcs;

using Fixture = ContextualRecordTestFixture<UniquePlacedNpcAnalyzer, PlacedNpc, IPlacedNpcGetter>;

public class UniquePlacedNpcAnalyzerTest
{
    Npc Setup(PlacedNpc rec, ISkyrimMod mod)
    {
        var npc = mod.Npcs.AddNew();
        npc.Configuration.Flags |= NpcConfiguration.Flag.Unique;
        rec.Base.SetTo(npc);
        return npc;
    }

    [Theory, MutagenModAutoData]
    public void NoPersistLoc(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                Setup(rec, mod);
            },
            prepForFix: (rec, mod) =>
            {
                rec.PersistentLocation.SetTo(FormKeys.SkyrimSE.Skyrim.Location.RiverwoodLocation);
            },
            UniquePlacedNpcAnalyzer.UniqueNpcWithoutPersistenceLocation);
    }

    [Theory, MutagenModAutoData]
    public void NotUnique(Fixture fixture)
    {
        Npc? npc = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                npc = Setup(rec, mod);
            },
            prepForFix: (rec, mod) =>
            {
                npc!.Configuration.Flags &= ~NpcConfiguration.Flag.Unique;
            },
            UniquePlacedNpcAnalyzer.UniqueNpcWithoutPersistenceLocation);
    }

    [Theory, MutagenModAutoData]
    public void StartDead(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                Setup(rec, mod);
            },
            prepForFix: (rec, mod) =>
            {
                rec.MajorFlags |= PlacedNpc.MajorFlag.StartsDead;
            },
            UniquePlacedNpcAnalyzer.UniqueNpcWithoutPersistenceLocation);
    }

    [Theory, MutagenModAutoData]
    public void InitiallyDisabled(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                Setup(rec, mod);
            },
            prepForFix: (rec, mod) =>
            {
                rec.MajorFlags |= PlacedNpc.MajorFlag.InitiallyDisabled;
            },
            UniquePlacedNpcAnalyzer.UniqueNpcWithoutPersistenceLocation);
    }
}
