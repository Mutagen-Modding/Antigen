using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Dialog.Responses;

using Fixture = ContextualRecordTestFixture<SpeakerAnalyzer, DialogResponses, IDialogResponsesGetter>;

public class SpeakerAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void NoSpeakers(
        Fixture fixture,
        DialogTopic topic,
        Quest quest,
        Npc npc,
        VoiceType voice)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                topic.Responses.Add(rec);
                topic.Quest.SetTo(quest);
                npc.Voice.SetTo(voice);

                var data = new GetIsIDConditionData();
                data.Object.Link.SetTo(npc);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    ComparisonValue = 1,
                });
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    ComparisonValue = 0,
                });
            },
            prepForFix: (rec, mod) =>
            {
                rec.Conditions.RemoveAt(1);
            },
            SpeakerAnalyzer.MissingSpeaker);
    }

    // Different speaker and different voice have almost identical setup, but differ in severity due to diffent voice
    // being silent ingame while different speaker only indicates possibly unintended usage.
    static void PrepDifferentSpeakerShared(DialogResponses rec, DialogResponses sharedResponse, Npc npc1, Npc npc2)
    {
        var data1 = new GetIsIDConditionData();
        data1.Object.Link.SetTo(npc1);
        var data2 = new GetIsIDConditionData();
        data2.Object.Link.SetTo(npc2);

        // GetIsId (npc1 || npc2)
        rec.Conditions.Add(new ConditionFloat()
        {
            Data = data1,
            ComparisonValue = 1,
            Flags = Condition.Flag.OR
        });
        rec.Conditions.Add(new ConditionFloat()
        {
            Data = data2,
            ComparisonValue = 1,
        });
        rec.ResponseData.SetTo(sharedResponse);

        // GetIsId (npc1)
        sharedResponse.Conditions.Add(new ConditionFloat()
        {
            Data = data1,
            ComparisonValue = 1,
            Flags = Condition.Flag.OR
        });
    }

    [Theory, MutagenModAutoData]
    public void DifferentSpeakerSharedInfo(Fixture fixture,
        DialogTopic topic,
        Quest quest,
        DialogResponses sharedResponse,
        Npc npc1,
        Npc npc2,
        VoiceType voice)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                topic.Responses.AddRange(rec, sharedResponse);
                topic.Quest.SetTo(quest);
                npc1.Voice.SetTo(voice);
                npc2.Voice.SetTo(voice);

                PrepDifferentSpeakerShared(rec, sharedResponse, npc1, npc2);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Conditions.RemoveAt(1);
            },
            SpeakerAnalyzer.DifferentSpeakerInSharedInfo);
    }

    [Theory, MutagenModAutoData]
    public void DifferentVoiceSharedInfo(
        Fixture fixture,
        DialogTopic topic,
        Quest quest,
        DialogResponses sharedResponse,
        Npc npc1,
        VoiceType voice1,
        Npc npc2,
        VoiceType voice2)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                topic.Responses.AddRange(rec, sharedResponse);
                topic.Quest.SetTo(quest);
                npc1.Voice.SetTo(voice1);
                npc2.Voice.SetTo(voice2);

                PrepDifferentSpeakerShared(rec, sharedResponse, npc1, npc2);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Conditions.RemoveAt(1);
            },
            // Different voice implies different speaker
            SpeakerAnalyzer.DifferentVoiceInSharedInfo, SpeakerAnalyzer.DifferentSpeakerInSharedInfo);
    }
}
