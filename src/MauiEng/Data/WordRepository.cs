using MauiEng.Core;
using Npgsql;
using NpgsqlTypes;

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
                   COUNT(w.id)::int AS word_count,
                   COUNT(w.id) FILTER (WHERE r.due_on <= @today)::int AS due_count
            FROM deck d
            LEFT JOIN word w ON w.deck_id = d.id
            LEFT JOIN review r ON r.word_id = w.id
            GROUP BY d.id, d.name
            ORDER BY d.name
            """;

        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("today", NpgsqlDbType.Date) { Value = today });
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
        await using var command = new NpgsqlCommand("SELECT name FROM deck WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", deckId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result as string;
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
            WHERE r.due_on <= @today
              AND (@deckId IS NULL OR w.deck_id = @deckId)
            ORDER BY r.due_on, w.id
            """;

        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("today", NpgsqlDbType.Date) { Value = today });
        command.Parameters.Add(new NpgsqlParameter("deckId", NpgsqlDbType.Integer)
        {
            Value = deckId.HasValue ? deckId.Value : DBNull.Value
        });
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
            VALUES (@wordId, @ease, @intervalDays, @reps, @due)
            ON CONFLICT (word_id) DO UPDATE SET
                ease = EXCLUDED.ease,
                interval_days = EXCLUDED.interval_days,
                successful_reps = EXCLUDED.successful_reps,
                due_on = EXCLUDED.due_on
            """;

        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("wordId", wordId);
        command.Parameters.AddWithValue("ease", review.Ease);
        command.Parameters.AddWithValue("intervalDays", review.IntervalDays);
        command.Parameters.AddWithValue("reps", review.SuccessfulReps);
        command.Parameters.Add(new NpgsqlParameter("due", NpgsqlDbType.Date) { Value = review.Due });
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static WordCard ReadCard(NpgsqlDataReader reader)
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
                reader.GetFieldValue<DateOnly>(reader.GetOrdinal("due_on")))
        };
    }

    private static string? ReadOptional(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
