using MauiEng.Core;
using Microsoft.Data.Sqlite;

namespace MauiEng.Data;

public sealed class WordRepository
{
    private readonly Database _database;

    public WordRepository(Database database) => _database = database;

    public async Task<IReadOnlyList<DeckSummary>> GetDecksAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT d.id,
                   d.name,
                   COUNT(w.id) AS word_count,
                   COUNT(CASE WHEN r.due_on <= $today THEN 1 END) AS due_count
            FROM deck d
            LEFT JOIN word w ON w.deck_id = d.id
            LEFT JOIN review r ON r.word_id = w.id
            GROUP BY d.id, d.name
            ORDER BY d.name
            """;

        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$today", FormatDate(today));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var decks = new List<DeckSummary>();
        while (await reader.ReadAsync(cancellationToken))
        {
            decks.Add(new DeckSummary
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                WordCount = reader.GetInt32(2),
                DueCount = reader.GetInt32(3)
            });
        }

        return decks;
    }

    public async Task<string?> GetDeckNameAsync(int deckId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM deck WHERE id = $id";
        command.Parameters.AddWithValue("$id", deckId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result as string;
    }

    public async Task<int> GetOrCreateDeckIdAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await using (var find = connection.CreateCommand())
        {
            find.CommandText = "SELECT id FROM deck WHERE name = $name";
            find.Parameters.AddWithValue("$name", name);
            var existing = await find.ExecuteScalarAsync(cancellationToken);
            if (existing is not null and not DBNull)
                return DbValues.Id(existing);
        }

        await using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO deck (name) VALUES ($name); SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$name", name);
        return DbValues.Id(await insert.ExecuteScalarAsync(cancellationToken));
    }

    public async Task AddWordAsync(
        int deckId,
        string term,
        string translation,
        string? transcription,
        string? example,
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        int wordId;
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO word (deck_id, term, translation, transcription, example)
                VALUES ($deckId, $term, $translation, $transcription, $example);
                SELECT last_insert_rowid();
                """;
            command.Parameters.AddWithValue("$deckId", deckId);
            command.Parameters.AddWithValue("$term", term.Trim());
            command.Parameters.AddWithValue("$translation", translation.Trim());
            command.Parameters.AddWithValue("$transcription", (object?)transcription?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("$example", (object?)example?.Trim() ?? DBNull.Value);
            wordId = DbValues.Id(await command.ExecuteScalarAsync(cancellationToken));
        }

        await using (var review = connection.CreateCommand())
        {
            review.Transaction = transaction;
            review.CommandText = """
                INSERT INTO review (word_id, ease, interval_days, successful_reps, due_on)
                VALUES ($wordId, $ease, 0, 0, $due)
                """;
            review.Parameters.AddWithValue("$wordId", wordId);
            review.Parameters.AddWithValue("$ease", Sm2.InitialEase);
            review.Parameters.AddWithValue("$due", FormatDate(today));
            await review.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WordCard>> GetDueAsync(DateOnly today, int? deckId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT w.id,
                   w.deck_id,
                   d.name AS deck_name,
                   w.term,
                   w.translation,
                   w.transcription,
                   w.example,
                   r.ease,
                   r.interval_days,
                   r.successful_reps,
                   r.due_on
            FROM word w
            JOIN review r ON r.word_id = w.id
            JOIN deck d ON d.id = w.deck_id
            WHERE r.due_on <= $today
              AND ($deckId IS NULL OR w.deck_id = $deckId)
            ORDER BY r.due_on, w.id
            """;

        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$today", FormatDate(today));
        command.Parameters.AddWithValue("$deckId", deckId.HasValue ? deckId.Value : DBNull.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var cards = new List<WordCard>();
        while (await reader.ReadAsync(cancellationToken))
            cards.Add(ReadCard(reader));

        return cards;
    }

    public async Task SaveReviewAsync(int wordId, ReviewState review, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO review (word_id, ease, interval_days, successful_reps, due_on)
            VALUES ($wordId, $ease, $intervalDays, $reps, $due)
            ON CONFLICT (word_id) DO UPDATE SET
                ease = excluded.ease,
                interval_days = excluded.interval_days,
                successful_reps = excluded.successful_reps,
                due_on = excluded.due_on
            """;

        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$wordId", wordId);
        command.Parameters.AddWithValue("$ease", review.Ease);
        command.Parameters.AddWithValue("$intervalDays", review.IntervalDays);
        command.Parameters.AddWithValue("$reps", review.SuccessfulReps);
        command.Parameters.AddWithValue("$due", FormatDate(review.Due));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static WordCard ReadCard(SqliteDataReader reader)
    {
        return new WordCard
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            DeckId = reader.GetInt32(reader.GetOrdinal("deck_id")),
            DeckName = reader.GetString(reader.GetOrdinal("deck_name")),
            Term = reader.GetString(reader.GetOrdinal("term")),
            Translation = reader.GetString(reader.GetOrdinal("translation")),
            Transcription = ReadOptional(reader, "transcription"),
            Example = ReadOptional(reader, "example"),
            Review = new ReviewState(
                reader.GetDouble(reader.GetOrdinal("ease")),
                reader.GetInt32(reader.GetOrdinal("interval_days")),
                reader.GetInt32(reader.GetOrdinal("successful_reps")),
                DateOnly.Parse(reader.GetString(reader.GetOrdinal("due_on"))))
        };
    }

    private static string? ReadOptional(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static string FormatDate(DateOnly day) => day.ToString("yyyy-MM-dd");
}
