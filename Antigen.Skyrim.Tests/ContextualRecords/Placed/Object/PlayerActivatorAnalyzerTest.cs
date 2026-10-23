using Antigen.Skyrim.Record.Placed.Object;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Xunit;
using Activator = Mutagen.Bethesda.Skyrim.Activator;

namespace Antigen.Skyrim.Tests.ContextualRecords.Placed.Object;

using Fixture = ContextualRecordTestFixture<PlayerActivatorAnalyzer, PlacedObject, IPlacedObjectGetter>;

public sealed class PlayerActivatorAnalyzerTest
{
    // Configure the placed object as a player activator with the given primitive and rotation
    Activator Setup(Fixture fixture, ISkyrimMod mod, PlacedObject placed, PlacedPrimitive.TypeEnum primitiveType, P3Float rotation)
    {
        var activator = fixture.Create<Activator>();
        mod.Activators.Add(activator);

        placed.Base.SetTo(activator);
        placed.CollisionLayer = PlayerActivatorAnalyzer.L_NONCOLLIDABLE;
        placed.Primitive = new PlacedPrimitive { Type = primitiveType };
        placed.Placement = new Placement { Rotation = rotation };
        return activator;
    }

    // An unnamed player activator should be reported
    [Theory, MutagenModAutoData]
    public void MissingName(Fixture fixture)
    {
        Activator? activator = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                activator = Setup(fixture, mod, rec, PlacedPrimitive.TypeEnum.Box, new P3Float(1, 2, 3));
            },
            prepForFix: (rec, mod) =>
            {
                activator!.Name = "Named Activator";
            },
            PlayerActivatorAnalyzer.MissingName);
    }

    // A box player activator without rotation should be reported
    [Theory, MutagenModAutoData]
    public void InvalidRotation(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var activator = Setup(fixture, mod, rec, PlacedPrimitive.TypeEnum.Box, new P3Float(0, 0, 0));
                activator.Name = "Named Activator";
            },
            prepForFix: (rec, mod) =>
            {
                rec.Placement = new Placement { Rotation = new P3Float(1, 2, 3) };
            },
            PlayerActivatorAnalyzer.InvalidRotation);
    }

    // A named box player activator with rotation should not be reported
    [Theory, MutagenModAutoData]
    public void ValidBoxActivator(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            var activator = Setup(fixture, mod, rec, PlacedPrimitive.TypeEnum.Box, new P3Float(1, 2, 3));
            activator.Name = "Named Activator";
        });
    }

    // Non-box primitives are not checked for rotation
    [Theory, MutagenModAutoData]
    public void SphereActivator(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            var activator = Setup(fixture, mod, rec, PlacedPrimitive.TypeEnum.Sphere, new P3Float(0, 0, 0));
            activator.Name = "Named Activator";
        });
    }

    // Objects on other collision layers are skipped
    [Theory, MutagenModAutoData]
    public void WrongCollisionLayer(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            Setup(fixture, mod, rec, PlacedPrimitive.TypeEnum.Box, new P3Float(0, 0, 0));
            rec.CollisionLayer = 0;
        });
    }
}
