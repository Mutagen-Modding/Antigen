using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Record.Book;

using Keywords = FormKeys.SkyrimSE.Skyrim.Keyword;

public class MissingFieldsAnalyzerBook : IIsolatedRecordAnalyzer<IBookGetter>
{
    public static readonly TopicDefinition NoTeachesSpell = MutagenTopicBuilder.FromDiscussion(
            670,
            "Missing spell",
            Severity.CTD)
        .WithoutFormatting("Book teaches spell has no spell to teach");

    public static readonly TopicDefinition NoNoteUpSound = MutagenTopicBuilder.FromDiscussion(
            671,
            "Note without pickup sound",
            Severity.Suggestion)
        .WithoutFormatting("Book is a note but has no pick up sound");

    public static readonly TopicDefinition NoInventoryArt = MutagenTopicBuilder.FromDiscussion(
            669,
            "Missing Inventory Art",
            Severity.CTD)
        .WithoutFormatting("Book is missing inventory art");

    public IEnumerable<TopicDefinition> Topics =>
    [
        NoTeachesSpell,
        NoNoteUpSound,
        NoInventoryArt,
    ];

    // Heuristic to determine if a book should be a note
    bool IsNote(IBookGetter book)
    {
        return book.Model != null && book.Model.File.ContainsInsensitive("Note");
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IBookGetter> param)
    {
        var book = param.Record;

        if (book.HasKeyword(Keywords.Dummy))
            return;

        if (book.InventoryArt.IsNull)
        {
            param.AddTopic(NoInventoryArt.Format());
        }

        if (book.Teaches is IBookSpellGetter teachesSpell)
        {
            if (teachesSpell.Spell.IsNull)
            {
                param.AddTopic(NoTeachesSpell.Format());
            }
        }

        if (book.PickUpSound.IsNull && IsNote(book))
        {
            param.AddTopic(NoNoteUpSound.Format());
        }
    }

    public IEnumerable<Func<IBookGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.Teaches;
        yield return x => x.InventoryArt;
        yield return x => x.PickUpSound;
    }
}
