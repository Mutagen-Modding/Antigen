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

    public static readonly TopicDefinition<string?> AliasReferencesSelf = MutagenTopicBuilder.FromDiscussion(
            181,
            "Alias references self",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} references itself");

    public static readonly TopicDefinition<string?, uint> AliasReferenceNotPreviousAlias = MutagenTopicBuilder.FromDiscussion(
            182,
            "Alias reference not previous alias",
            Severity.Error)
        .WithFormatting<string?, uint>("Alias {0} references AliasID {1} that is not among the previous aliases");

    IEnumerable<TopicDefinition> IAnalyzer.Topics { get; } = [NextAliasIDAlreadyInUse, AliasIDDuplicate, AliasReferencesSelf, AliasReferenceNotPreviousAlias];

    void IIsolatedRecordAnalyzer<IQuestGetter>.AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;

        if (quest.Aliases.Any(a => a.ID >= quest.NextAliasID))
        {
            param.AddTopic(NextAliasIDAlreadyInUse.Format());
        }

        HashSet<uint> ids = [];
        foreach(IQuestAliasGetter alias in quest.Aliases)
        {
            if (alias.Location != null)
            {
                CheckAliasAlreadyDefined(param, alias.Location.AliasID, alias, ids);
            }

            if (alias.CreateReferenceToObject != null)
            {
                CheckAliasAlreadyDefined(param, alias.CreateReferenceToObject.AliasID, alias, ids);
            }

            if (alias.FindMatchingRefNearAlias != null)
            {
                CheckAliasAlreadyDefined(param, alias.FindMatchingRefNearAlias.AliasID, alias, ids);
            }

            if(!ids.Add(alias.ID))
            {
                param.AddTopic(AliasIDDuplicate.Format());
            }
        }
    }

    private static void CheckAliasAlreadyDefined(IsolatedRecordAnalyzerParams<IQuestGetter> param, uint idToCheck, IQuestAliasGetter currentAlias, HashSet<uint> previousIds)
    {
        if (idToCheck == currentAlias.ID)
        {
            param.AddTopic(AliasReferencesSelf.Format(currentAlias.Name));
        }
        else if (!previousIds.Contains(idToCheck))
        {
            param.AddTopic(AliasReferenceNotPreviousAlias.Format(currentAlias.Name, idToCheck));
        }
    }

    private static void CheckAliasAlreadyDefined(IsolatedRecordAnalyzerParams<IQuestGetter> param, int? idToCheck, IQuestAliasGetter currentAlias, HashSet<uint> previousIds)
    {
        if (idToCheck != null)
        {
            CheckAliasAlreadyDefined(param, Convert.ToUInt32(idToCheck), currentAlias, previousIds);
        }
    }

    private static void CheckAliasAlreadyDefined(IsolatedRecordAnalyzerParams<IQuestGetter> param, short? idToCheck, IQuestAliasGetter currentAlias, HashSet<uint> previousIds)
    {
        if (idToCheck != null)
        {
            CheckAliasAlreadyDefined(param, Convert.ToUInt32(idToCheck), currentAlias, previousIds);
        }
    }

    IEnumerable<Func<IQuestGetter, object?>> IIsolatedRecordAnalyzer<IQuestGetter>.FieldsOfInterest()
    {
        yield return x => x.NextAliasID;
        yield return x => x.Aliases;
    }
}

