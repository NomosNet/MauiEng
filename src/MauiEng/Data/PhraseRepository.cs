using MauiEng.Core;
using Microsoft.Data.Sqlite;

namespace MauiEng.Data;

public sealed class PhraseRepository
{
    private readonly Database _database;

    public PhraseRepository(Database database) => _database = database;

    public async Task AddAsync(string text, string? translation, DateOnly today, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO phrase (text, translation, added_on)
            VALUES ($text, $translation, $addedOn)
            """;
        command.Parameters.AddWithValue("$text", text.Trim());
        command.Parameters.AddWithValue("$translation", (object?)translation?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("$addedOn", today.ToString("yyyy-MM-dd"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PhraseItem>> GetRecentAsync(DateOnly today, int days = 3, CancellationToken cancellationToken = default)
    {
        var from = today.AddDays(1 - days).ToString("yyyy-MM-dd");
        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, text, translation, added_on
            FROM phrase
            WHERE added_on >= $from
            ORDER BY added_on DESC, id DESC
            """;
        command.Parameters.AddWithValue("$from", from);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<PhraseItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PhraseItem
            {
                Id = reader.GetInt32(0),
                Text = reader.GetString(1),
                Translation = reader.IsDBNull(2) ? null : reader.GetString(2),
                AddedOn = DateOnly.Parse(reader.GetString(3))
            });
        }

        return items;
    }
}
