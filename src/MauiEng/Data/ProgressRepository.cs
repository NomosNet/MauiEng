using MauiEng.Core;
using Npgsql;
using NpgsqlTypes;

namespace MauiEng.Data;

public sealed class ProgressRepository
{
    private readonly Database _database;

    public ProgressRepository(Database database) => _database = database;

    public async Task AddReviewAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO day_log (day, reviews_count, goal_met)
            VALUES (@day, 1, @goalMet)
            ON CONFLICT (day) DO UPDATE SET
                reviews_count = day_log.reviews_count + 1,
                goal_met = day_log.reviews_count + 1 >= @goal
            """;

        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("day", NpgsqlDbType.Date) { Value = today });
        command.Parameters.AddWithValue("goalMet", StudyStreak.DailyGoal <= 1);
        command.Parameters.AddWithValue("goal", StudyStreak.DailyGoal);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<DaySnapshot> GetTodayAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);

        var reviewsToday = 0;
        await using (var command = new NpgsqlCommand(
            "SELECT reviews_count FROM day_log WHERE day = @day",
            connection))
        {
            command.Parameters.Add(new NpgsqlParameter("day", NpgsqlDbType.Date) { Value = today });
            reviewsToday = DbValues.Count(await command.ExecuteScalarAsync(cancellationToken));
        }

        var metDays = new List<DateOnly>();
        await using (var command = new NpgsqlCommand(
            "SELECT day FROM day_log WHERE goal_met ORDER BY day",
            connection))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                metDays.Add(reader.GetFieldValue<DateOnly>(0));
        }

        int dueCount;
        await using (var command = new NpgsqlCommand(
            "SELECT COUNT(*)::int FROM review WHERE due_on <= @today",
            connection))
        {
            command.Parameters.Add(new NpgsqlParameter("today", NpgsqlDbType.Date) { Value = today });
            dueCount = DbValues.Count(await command.ExecuteScalarAsync(cancellationToken));
        }

        return new DaySnapshot(
            StudyStreak.Current(metDays, today),
            reviewsToday,
            StudyStreak.DailyGoal,
            dueCount);
    }
}
