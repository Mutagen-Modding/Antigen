using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class AliasIDAnalyzer : IIsolatedRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition NextAliasIDAlreadyInUse = MutagenTopicBuilder.FromDiscussion(
            179,
            "NextAliasID in use",
            Severity.Error)
        .WithoutFormatting("NextAliasID is equal or smaller than ID of existing alias");

    public static readonly TopicDefinition<int> AliasIDDuplicate = MutagenTopicBuilder.FromDiscussion(
            180,
            "AliasID duplicate",
            Severity.Error)
        .WithFormatting<int>("AliasID {0} is used multiple times in the same quest");

    public static readonly TopicDefinition<string?, string> AliasReferencesSelf = MutagenTopicBuilder.FromDiscussion(
            181,
            "Alias references self",
            Severity.Error)
        .WithFormatting<string?, string>("Alias {0} references itself in {1}");

    public static readonly TopicDefinition<string?, uint, string, string> AliasReferenceNotPreviousAlias = MutagenTopicBuilder.FromDiscussion(
            182,
            "Alias reference not previous alias",
            Severity.Error)
        .WithFormatting<string?, uint, string, string>("Alias {0} references AliasID {1} {2} in {3} that is not defined earlier");

    IEnumerable<TopicDefinition> IAnalyzer.Topics { get; } = [NextAliasIDAlreadyInUse, AliasIDDuplicate, AliasReferencesSelf, AliasReferenceNotPreviousAlias];

    private static readonly string aliasrefSourceLocation = "'Fill Type - Location Alias Reference'";
    private static readonly string aliasrefSourceCreate = "'Fill Type - Create Reference To Object";
    private static readonly string aliasrefSourceMatchingRef = "'Fill Type - Find Matchning Reference Near Alias'";

    void IIsolatedRecordAnalyzer<IQuestGetter>.AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;

        if (quest.Aliases.Any(a => a.ID >= quest.NextAliasID))
        {
            param.AddTopic(NextAliasIDAlreadyInUse.Format());
        }

        Dictionary<uint, IQuestAliasGetter> aliases = [];
        foreach (var alias in quest.Aliases)
        {
            _ = aliases.TryAdd(alias.ID, alias);
        }

        HashSet<uint> ids = [];
        foreach(var alias in quest.Aliases)
        {
            if (alias.Location != null)
            {
                CheckAliasAlreadyDefined(aliasrefSourceLocation, param, alias.Location.AliasID, alias, ids, aliases);
            }

            if (alias.CreateReferenceToObject != null)
            {
                CheckAliasAlreadyDefined(aliasrefSourceCreate, param, alias.CreateReferenceToObject.AliasID, alias, ids, aliases);
            }

            if (alias.FindMatchingRefNearAlias != null)
            {
                CheckAliasAlreadyDefined(aliasrefSourceMatchingRef,param, alias.FindMatchingRefNearAlias.AliasID, alias, ids, aliases);
            }

            if(!ids.Add(alias.ID))
            {
                param.AddTopic(AliasIDDuplicate.Format());
            }
        }
    }

    private static void CheckAliasAlreadyDefined(string refSource, IsolatedRecordAnalyzerParams<IQuestGetter> param, uint idToCheck, IQuestAliasGetter currentAlias, HashSet<uint> previousIds, Dictionary<uint, IQuestAliasGetter> aliases)
    {
        if (idToCheck == currentAlias.ID)
        {
            param.AddTopic(AliasReferencesSelf.Format(currentAlias.Name, refSource));
        }
        else if (!previousIds.Contains(idToCheck))
        {
            param.AddTopic(AliasReferenceNotPreviousAlias.Format(
                currentAlias.Name,
                idToCheck,
                GetReferencedAliasName(idToCheck, aliases),
                refSource));
        }
    }

    private static void CheckAliasAlreadyDefined(string refSource, IsolatedRecordAnalyzerParams<IQuestGetter> param, int? idToCheck, IQuestAliasGetter currentAlias, HashSet<uint> previousIds, Dictionary<uint, IQuestAliasGetter> aliases)
    {
        if (idToCheck != null)
        {
            CheckAliasAlreadyDefined(refSource, param, Convert.ToUInt32(idToCheck), currentAlias, previousIds, aliases);
        }
    }

    private static void CheckAliasAlreadyDefined(string refSource, IsolatedRecordAnalyzerParams<IQuestGetter> param, short? idToCheck, IQuestAliasGetter currentAlias, HashSet<uint> previousIds, Dictionary<uint, IQuestAliasGetter> aliases)
    {
        if (idToCheck != null)
        {
            CheckAliasAlreadyDefined(refSource, param, Convert.ToUInt32(idToCheck), currentAlias, previousIds, aliases);
        }
    }

    private static string GetReferencedAliasName(uint idToCheck, Dictionary<uint, IQuestAliasGetter> aliases)
    {
        if (aliases.TryGetValue(idToCheck, out var referencedAlias)
            && referencedAlias.Name != null)
        {
            return referencedAlias.Name;
        }
        return "[[MISSING]]";
    }

    IEnumerable<Func<IQuestGetter, object?>> IIsolatedRecordAnalyzer<IQuestGetter>.FieldsOfInterest()
    {
        yield return x => x.NextAliasID;
        yield return x => x.Aliases;
    }
}

