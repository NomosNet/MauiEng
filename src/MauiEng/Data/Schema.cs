using Microsoft.Data.Sqlite;

namespace MauiEng.Data;

internal static class Schema
{
    private const string Sql = """
        CREATE TABLE IF NOT EXISTS deck (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL UNIQUE
        );

        CREATE TABLE IF NOT EXISTS word (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            deck_id INTEGER NOT NULL REFERENCES deck (id),
            term TEXT NOT NULL,
            translation TEXT NOT NULL,
            transcription TEXT,
            example TEXT
        );

        CREATE TABLE IF NOT EXISTS review (
            word_id INTEGER PRIMARY KEY REFERENCES word (id),
            ease REAL NOT NULL,
            interval_days INTEGER NOT NULL,
            successful_reps INTEGER NOT NULL,
            due_on TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS day_log (
            day TEXT PRIMARY KEY,
            reviews_count INTEGER NOT NULL,
            goal_met INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS day_task (
            day TEXT NOT NULL,
            kind INTEGER NOT NULL,
            done INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (day, kind)
        );

        CREATE TABLE IF NOT EXISTS media_link (
            kind INTEGER PRIMARY KEY,
            display_name TEXT NOT NULL,
            uri TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS phrase (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            text TEXT NOT NULL,
            translation TEXT,
            added_on TEXT NOT NULL
        );

        CREATE INDEX IF NOT EXISTS review_due_on_idx ON review (due_on);
        CREATE INDEX IF NOT EXISTS phrase_added_on_idx ON phrase (added_on);
        """;

    public static async Task EnsureAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = Sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
