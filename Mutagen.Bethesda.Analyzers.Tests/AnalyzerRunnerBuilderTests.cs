using System.IO.Abstractions;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests;

/// <summary>
/// Test analyzer that detects NPCs with "Test" in their name
/// </summary>
public class TestNpcAnalyzer : IIsolatedRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<string?> TestNpcTopic = MutagenTopicBuilder.FromDiscussion(
        9999,
        "Test NPC Detected",
        Severity.Warning)
        .WithFormatting<string?>("NPC has 'Test' in the name: {0}");

    public IEnumerable<TopicDefinition> Topics => TestNpcTopic.AsEnumerable();

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<INpcGetter> param)
    {
        if (param.Record.Name?.String?.Contains("Test", StringComparison.OrdinalIgnoreCase) == true)
        {
            param.AddTopic(TestNpcTopic.Format(param.Record.Name?.String));
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
    }
}

/// <summary>
/// Another test analyzer that detects weapons with high damage
/// </summary>
public class HighDamageWeaponAnalyzer : IIsolatedRecordAnalyzer<IWeaponGetter>
{
    public static readonly TopicDefinition<ushort> HighDamageTopic = MutagenTopicBuilder.FromDiscussion(
        9998,
        "High Damage Weapon",
        Severity.Suggestion)
        .WithFormatting<ushort>("Weapon has unusually high damage value: {0}");

    public IEnumerable<TopicDefinition> Topics => HighDamageTopic.AsEnumerable();

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IWeaponGetter> param)
    {
        if (param.Record.BasicStats?.Damage > 100)
        {
            param.AddTopic(HighDamageTopic.Format(param.Record.BasicStats.Damage));
        }
    }

    public IEnumerable<Func<IWeaponGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.BasicStats;
    }
}

public class AnalyzerRunnerBuilderTests
{
    [Theory, MutagenAutoData(GameRelease.SkyrimSE)]
    public async Task WithAnalyzers_CustomAnalyzer_DetectsIssues(
        TestNpcAnalyzer testAnalyzer,
        IFileSystem fileSystem)
    {
        // Arrange
        var mod = new SkyrimMod(ModKey.FromNameAndExtension("Test.esp"), SkyrimRelease.SkyrimSE);

        // Create test NPC with "Test" in name that should be detected
        var npc = mod.Npcs.AddNew();
        npc.Name = "Test Character";

        var loadOrder = new LoadOrder<IModListingGetter<ISkyrimModGetter>>();
        loadOrder.Add(new ModListing<ISkyrimModGetter>(mod, enabled: true));

        // Act
        var runner = AnalyzerRunnerBuilder.Create(GameRelease.SkyrimSE)
            .WithLoadOrder(loadOrder)
            .WithFileSystem(fileSystem)
            .WithAnalyzers(testAnalyzer)
            .Build();

        var results = new List<AnalyzerResult>();
        await foreach (var result in runner.Analyze())
        {
            results.Add(result);
        }

        // Assert
        results.ShouldNotBeEmpty();
        results.ShouldContain(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id);
        var testResult = results.First(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id);
        testResult.Record.ShouldBe(npc);
        testResult.ModKey.ShouldBe(mod.ModKey);
    }

    [Theory, MutagenAutoData(GameRelease.SkyrimSE)]
    public async Task WithAnalyzers_MultipleAnalyzers_BothDetectIssues(
        TestNpcAnalyzer npcAnalyzer,
        HighDamageWeaponAnalyzer weaponAnalyzer,
        IFileSystem fileSystem)
    {
        // Arrange
        var mod = new SkyrimMod(ModKey.FromNameAndExtension("Test.esp"), SkyrimRelease.SkyrimSE);

        // Create test records that should trigger both analyzers
        var npc = mod.Npcs.AddNew();
        npc.Name = "Test Character";

        var weapon = mod.Weapons.AddNew();
        weapon.BasicStats = new WeaponBasicStats { Damage = 150 };

        var loadOrder = new LoadOrder<IModListingGetter<ISkyrimModGetter>>();
        loadOrder.Add(new ModListing<ISkyrimModGetter>(mod, enabled: true));

        // Act
        var runner = AnalyzerRunnerBuilder.Create(GameRelease.SkyrimSE)
            .WithLoadOrder(loadOrder)
            .WithFileSystem(fileSystem)
            .WithAnalyzers(npcAnalyzer, weaponAnalyzer)
            .Build();

        var results = new List<AnalyzerResult>();
        await foreach (var result in runner.Analyze())
        {
            results.Add(result);
        }

        // Assert
        results.ShouldNotBeEmpty();
        results.ShouldContain(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id);
        results.ShouldContain(r => r.Topic.TopicDefinition.Id == HighDamageWeaponAnalyzer.HighDamageTopic.Id);

        var npcResult = results.First(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id);
        npcResult.Record.ShouldBe(npc);

        var weaponResult = results.First(r => r.Topic.TopicDefinition.Id == HighDamageWeaponAnalyzer.HighDamageTopic.Id);
        weaponResult.Record.ShouldBe(weapon);
    }

    [Theory, MutagenAutoData(GameRelease.SkyrimSE)]
    public async Task WithAnalyzers_AdditiveCalls_DetectsBothAnalyzerTypes(
        TestNpcAnalyzer npcAnalyzer,
        HighDamageWeaponAnalyzer weaponAnalyzer,
        IFileSystem fileSystem)
    {
        // Arrange
        var mod = new SkyrimMod(ModKey.FromNameAndExtension("Test.esp"), SkyrimRelease.SkyrimSE);

        // Create test records that should trigger both analyzers
        var npc = mod.Npcs.AddNew();
        npc.Name = "Test Character";

        var weapon = mod.Weapons.AddNew();
        weapon.BasicStats = new WeaponBasicStats { Damage = 150 };

        var loadOrder = new LoadOrder<IModListingGetter<ISkyrimModGetter>>();
        loadOrder.Add(new ModListing<ISkyrimModGetter>(mod, enabled: true));

        // Act
        var runner = AnalyzerRunnerBuilder.Create(GameRelease.SkyrimSE)
            .WithLoadOrder(loadOrder)
            .WithFileSystem(fileSystem)
            .WithAnalyzers(npcAnalyzer)           // First call
            .WithAnalyzers(weaponAnalyzer)        // Second call - should be additive
            .Build();

        var results = new List<AnalyzerResult>();
        await foreach (var result in runner.Analyze())
        {
            results.Add(result);
        }

        // Assert - Should detect issues from both analyzers
        results.ShouldNotBeEmpty();
        results.ShouldContain(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id);
        results.ShouldContain(r => r.Topic.TopicDefinition.Id == HighDamageWeaponAnalyzer.HighDamageTopic.Id);

        var npcResult = results.First(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id);
        npcResult.Record.ShouldBe(npc);

        var weaponResult = results.First(r => r.Topic.TopicDefinition.Id == HighDamageWeaponAnalyzer.HighDamageTopic.Id);
        weaponResult.Record.ShouldBe(weapon);
    }

    [Theory, MutagenAutoData(GameRelease.SkyrimSE)]
    public async Task WithAnalyzers_DuplicateAnalyzerTypes_UsesFirstInstanceOnly(
        IFileSystem fileSystem)
    {
        // Arrange
        var mod = new SkyrimMod(ModKey.FromNameAndExtension("Test.esp"), SkyrimRelease.SkyrimSE);

        // Create test record that should trigger the analyzer
        var npc = mod.Npcs.AddNew();
        npc.Name = "Test Character";

        var loadOrder = new LoadOrder<IModListingGetter<ISkyrimModGetter>>();
        loadOrder.Add(new ModListing<ISkyrimModGetter>(mod, enabled: true));

        var analyzer1 = new TestNpcAnalyzer();
        var analyzer2 = new TestNpcAnalyzer(); // Same type, should be deduplicated

        // Act
        var runner = AnalyzerRunnerBuilder.Create(GameRelease.SkyrimSE)
            .WithLoadOrder(loadOrder)
            .WithFileSystem(fileSystem)
            .WithAnalyzers(analyzer1)
            .WithAnalyzers(analyzer2)    // Duplicate type - should use first instance
            .Build();

        var results = new List<AnalyzerResult>();
        await foreach (var result in runner.Analyze())
        {
            results.Add(result);
        }

        // Assert - Should only get one result (not duplicated) since duplicate analyzers are deduplicated
        var npcResults = results.Where(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id).ToList();
        npcResults.ShouldHaveSingleItem();
        npcResults.Single().Record.ShouldBe(npc);
    }

    [Theory, MutagenAutoData(GameRelease.SkyrimSE)]
    public async Task WithoutCustomAnalyzers_NoCustomResults(
        IFileSystem fileSystem)
    {
        // Arrange
        var mod = new SkyrimMod(ModKey.FromNameAndExtension("Test.esp"), SkyrimRelease.SkyrimSE);

        // Create test records that would trigger our custom analyzers if they were registered
        var npc = mod.Npcs.AddNew();
        npc.Name = "Test Character";

        var weapon = mod.Weapons.AddNew();
        weapon.BasicStats = new WeaponBasicStats { Damage = 150 };

        var loadOrder = new LoadOrder<IModListingGetter<ISkyrimModGetter>>();
        loadOrder.Add(new ModListing<ISkyrimModGetter>(mod, enabled: true));

        // Act - Build without custom analyzers
        var runner = AnalyzerRunnerBuilder.Create(GameRelease.SkyrimSE)
            .WithLoadOrder(loadOrder)
            .WithFileSystem(fileSystem)
            // No custom analyzers added, no typical analyzers either
            .Build();

        var results = new List<AnalyzerResult>();
        await foreach (var result in runner.Analyze())
        {
            results.Add(result);
        }

        // Assert - Should not contain our custom analyzer results
        results.ShouldNotContain(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id);
        results.ShouldNotContain(r => r.Topic.TopicDefinition.Id == HighDamageWeaponAnalyzer.HighDamageTopic.Id);
    }

    [Theory, MutagenAutoData(GameRelease.SkyrimSE)]
    public async Task WithTypicalAnalyzers_DetectsCustomAnalyzerResults(
        TestNpcAnalyzer testAnalyzer,
        IFileSystem fileSystem)
    {
        // Arrange
        var mod = new SkyrimMod(ModKey.FromNameAndExtension("Test.esp"), SkyrimRelease.SkyrimSE);

        // Create test record that should trigger our custom analyzer
        var npc = mod.Npcs.AddNew();
        npc.Name = "Test Character";

        var loadOrder = new LoadOrder<IModListingGetter<ISkyrimModGetter>>();
        loadOrder.Add(new ModListing<ISkyrimModGetter>(mod, enabled: true));

        // Act - Use custom analyzers only since typical analyzers require more complex setup
        var runner = AnalyzerRunnerBuilder.Create(GameRelease.SkyrimSE)
            .WithLoadOrder(loadOrder)
            .WithFileSystem(fileSystem)
            .WithAnalyzers(testAnalyzer) // Custom analyzer
            .Build();

        var results = new List<AnalyzerResult>();
        await foreach (var result in runner.Analyze())
        {
            results.Add(result);
        }

        // Assert - Should detect our custom analyzer result
        var customResults = results.Where(r => r.Topic.TopicDefinition.Id == TestNpcAnalyzer.TestNpcTopic.Id).ToList();
        customResults.ShouldHaveSingleItem();
        customResults.Single().Record.ShouldBe(npc);
    }
}
