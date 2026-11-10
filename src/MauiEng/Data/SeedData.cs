using MauiEng.Core;
using Npgsql;
using NpgsqlTypes;

namespace MauiEng.Data;

internal static class SeedData
{
    private sealed record SeedWord(string Term, string Translation, string Transcription, string Example);

    private static readonly SeedWord[] Words =
    [
        new("hello", "привет", "/həˈləʊ/", "Hello, how are you?"),
        new("water", "вода", "/ˈwɔːtə/", "A glass of water, please."),
        new("book", "книга", "/bʊk/", "This book is interesting."),
        new("friend", "друг", "/frend/", "She is my friend."),
        new("house", "дом", "/haʊs/", "Their house is near the park."),
        new("time", "время", "/taɪm/", "What time is it?"),
        new("food", "еда", "/fuːd/", "The food was delicious."),
        new("city", "город", "/ˈsɪti/", "This city is very big."),
        new("work", "работа", "/wɜːk/", "I have a lot of work today."),
        new("morning", "утро", "/ˈmɔːnɪŋ/", "Good morning!"),
        new("night", "ночь", "/naɪt/", "Good night, see you tomorrow."),
        new("family", "семья", "/ˈfæməli/", "My family lives here."),
        new("school", "школа", "/skuːl/", "The school starts at eight."),
        new("question", "вопрос", "/ˈkwestʃən/", "May I ask a question?"),
        new("answer", "ответ", "/ˈɑːnsə/", "I know the answer."),
        new("travel", "путешествие", "/ˈtrævəl/", "Travel opens the mind."),
        new("weather", "погода", "/ˈweðə/", "The weather is cold today."),
        new("music", "музыка", "/ˈmjuːzɪk/", "She listens to music."),
        new("language", "язык", "/ˈlæŋɡwɪdʒ/", "English is a useful language."),
        new("remember", "помнить", "/rɪˈmembə/", "I remember this word."),
        new("learn", "учить", "/lɜːn/", "I learn English every day."),
        new("speak", "говорить", "/spiːk/", "Can you speak slowly?"),
        new("write", "писать", "/raɪt/", "Please write your name."),
        new("read", "читать", "/riːd/", "I read a short story.")
    ];

    public static async Task EnsureAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var check = new NpgsqlCommand("SELECT COUNT(*)::int FROM deck", connection);
        if (DbValues.Count(await check.ExecuteScalarAsync(cancellationToken)) > 0)
            return;

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);

        int deckId;
        await using (var command = new NpgsqlCommand(
            "INSERT INTO deck (name) VALUES (@name) RETURNING id",
            connection,
            transaction))
        {
            command.Parameters.AddWithValue("name", "Базовые слова");
            deckId = DbValues.Id(await command.ExecuteScalarAsync(cancellationToken));
        }

        foreach (var word in Words)
        {
            int wordId;
            await using (var command = new NpgsqlCommand(
                """
                INSERT INTO word (deck_id, term, translation, transcription, example)
                VALUES (@deckId, @term, @translation, @transcription, @example)
                RETURNING id
                """,
                connection,
                transaction))
            {
                command.Parameters.AddWithValue("deckId", deckId);
                command.Parameters.AddWithValue("term", word.Term);
                command.Parameters.AddWithValue("translation", word.Translation);
                command.Parameters.AddWithValue("transcription", word.Transcription);
                command.Parameters.AddWithValue("example", word.Example);
                wordId = DbValues.Id(await command.ExecuteScalarAsync(cancellationToken));
            }

            await using var review = new NpgsqlCommand(
                """
                INSERT INTO review (word_id, ease, interval_days, successful_reps, due_on)
                VALUES (@wordId, @ease, 0, 0, @due)
                """,
                connection,
                transaction);
            review.Parameters.AddWithValue("wordId", wordId);
            review.Parameters.AddWithValue("ease", Sm2.InitialEase);
            review.Parameters.Add(new NpgsqlParameter("due", NpgsqlDbType.Date) { Value = today });
            await review.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
