using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Location;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Locations;

using Fixture = ContextualRecordTestFixture<NoParentLocationAnalyzer, Location, ILocationGetter>;

public class NoParentLocationAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void NoParent(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.ParentLocation.SetToNull();
            },
            prepForFix: (rec, mod) =>
            {
                rec.ParentLocation.SetTo(FormKeys.SkyrimSE.Skyrim.Location.RiverwoodLocation);
            },
            NoParentLocationAnalyzer.NoParentLocation);
    }

    [Theory, MutagenModAutoData]
    public void WorldspaceLocation(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.ParentLocation.SetToNull();
            },
            prepForFix: (rec, mod) =>
            {
                var world = fixture.Create<Worldspace>();
                mod.Worldspaces.Add(world);
                world.Location.SetTo(rec);
            },
            NoParentLocationAnalyzer.NoParentLocation);
    }
}
