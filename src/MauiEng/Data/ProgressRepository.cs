using MauiEng.Core;
using MauiEng.Services;
using Microsoft.Data.Sqlite;

namespace MauiEng.Data;

public sealed class ProgressRepository
{
    private readonly Database _database;
    private readonly IStatsWidgetUpdater _widget;

    public ProgressRepository(Database database, IStatsWidgetUpdater widget)
    {
        _database = database;
        _widget = widget;
    }

    public async Task AddReviewAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await EnsureDayTasksAsync(connection, today, cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                INSERT INTO day_log (day, reviews_count, goal_met)
                VALUES ($day, 1, 0)
                ON CONFLICT(day) DO UPDATE SET
                    reviews_count = day_log.reviews_count + 1
                """;
            command.Parameters.AddWithValue("$day", FormatDate(today));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        var reviewsToday = await GetReviewsCountAsync(connection, today, cancellationToken);
        var dueCount = await GetDueCountAsync(connection, today, cancellationToken);
        if (reviewsToday >= StudyStreak.DailyGoal || dueCount == 0)
            await SetTaskDoneAsync(connection, today, DayTaskKind.Cards, true, cancellationToken);

        await RefreshGoalMetAsync(connection, today, cancellationToken);
        await PublishWidgetAsync(connection, today, cancellationToken);
    }

    public async Task MarkTaskDoneAsync(DateOnly today, DayTaskKind kind, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await EnsureDayTasksAsync(connection, today, cancellationToken);
        await SetTaskDoneAsync(connection, today, kind, true, cancellationToken);
        await RefreshGoalMetAsync(connection, today, cancellationToken);
        await PublishWidgetAsync(connection, today, cancellationToken);
    }

    public async Task SetMediaAsync(MediaKind kind, string displayName, string uri, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO media_link (kind, display_name, uri)
            VALUES ($kind, $name, $uri)
            ON CONFLICT(kind) DO UPDATE SET
                display_name = excluded.display_name,
                uri = excluded.uri
            """;
        command.Parameters.AddWithValue("$kind", (int)kind);
        command.Parameters.AddWithValue("$name", displayName);
        command.Parameters.AddWithValue("$uri", uri);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<DaySnapshot> GetTodayAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenAsync(cancellationToken);
        await EnsureDayTasksAsync(connection, today, cancellationToken);

        var reviewsToday = await GetReviewsCountAsync(connection, today, cancellationToken);
        var dueCount = await GetDueCountAsync(connection, today, cancellationToken);

        if (!await IsTaskDoneAsync(connection, today, DayTaskKind.Cards, cancellationToken)
            && (reviewsToday >= StudyStreak.DailyGoal || dueCount == 0))
        {
            await SetTaskDoneAsync(connection, today, DayTaskKind.Cards, true, cancellationToken);
            await RefreshGoalMetAsync(connection, today, cancellationToken);
        }

        var tasks = await GetTasksAsync(connection, today, cancellationToken);
        var metDays = await GetGoalMetDaysAsync(connection, cancellationToken);
        var book = await GetMediaAsync(connection, MediaKind.Book, cancellationToken);
        var video = await GetMediaAsync(connection, MediaKind.Video, cancellationToken);

        var cardsDone = tasks.GetValueOrDefault(DayTaskKind.Cards);
        var readingDone = tasks.GetValueOrDefault(DayTaskKind.Reading);
        var watchingDone = tasks.GetValueOrDefault(DayTaskKind.Watching);
        var speakingDone = tasks.GetValueOrDefault(DayTaskKind.Speaking);
        var doneCount = new[] { cardsDone, readingDone, watchingDone, speakingDone }.Count(x => x);

        var snapshot = new DaySnapshot(
            StudyStreak.Current(metDays, today),
            reviewsToday,
            StudyStreak.DailyGoal,
            dueCount,
            doneCount,
            StudyStreak.TasksPerDay,
            cardsDone,
            readingDone,
            watchingDone,
            speakingDone,
            book?.Name,
            book?.Uri,
            video?.Name,
            video?.Uri);

        _widget.Publish(ToWidgetStats(snapshot));
        return snapshot;
    }

    private async Task PublishWidgetAsync(SqliteConnection connection, DateOnly today, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(connection, today, cancellationToken);
        _widget.Publish(ToWidgetStats(snapshot));
    }

    private async Task<DaySnapshot> BuildSnapshotAsync(SqliteConnection connection, DateOnly today, CancellationToken cancellationToken)
    {
        var reviewsToday = await GetReviewsCountAsync(connection, today, cancellationToken);
        var dueCount = await GetDueCountAsync(connection, today, cancellationToken);
        var tasks = await GetTasksAsync(connection, today, cancellationToken);
        var metDays = await GetGoalMetDaysAsync(connection, cancellationToken);
        var book = await GetMediaAsync(connection, MediaKind.Book, cancellationToken);
        var video = await GetMediaAsync(connection, MediaKind.Video, cancellationToken);
        var cardsDone = tasks.GetValueOrDefault(DayTaskKind.Cards);
        var readingDone = tasks.GetValueOrDefault(DayTaskKind.Reading);
        var watchingDone = tasks.GetValueOrDefault(DayTaskKind.Watching);
        var speakingDone = tasks.GetValueOrDefault(DayTaskKind.Speaking);
        var doneCount = new[] { cardsDone, readingDone, watchingDone, speakingDone }.Count(x => x);

        return new DaySnapshot(
            StudyStreak.Current(metDays, today),
            reviewsToday,
            StudyStreak.DailyGoal,
            dueCount,
            doneCount,
            StudyStreak.TasksPerDay,
            cardsDone,
            readingDone,
            watchingDone,
            speakingDone,
            book?.Name,
            book?.Uri,
            video?.Name,
            video?.Uri);
    }

    private static WidgetStats ToWidgetStats(DaySnapshot snapshot) => new(
        snapshot.Streak,
        snapshot.TasksDone,
        snapshot.TasksTotal,
        snapshot.DueCount,
        snapshot.CardsDone,
        snapshot.ReadingDone,
        snapshot.WatchingDone,
        snapshot.SpeakingDone);

    private static async Task EnsureDayTasksAsync(SqliteConnection connection, DateOnly today, CancellationToken cancellationToken)
    {
        foreach (DayTaskKind kind in Enum.GetValues<DayTaskKind>())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO day_task (day, kind, done)
                VALUES ($day, $kind, 0)
                ON CONFLICT(day, kind) DO NOTHING
                """;
            command.Parameters.AddWithValue("$day", FormatDate(today));
            command.Parameters.AddWithValue("$kind", (int)kind);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var log = connection.CreateCommand();
        log.CommandText = """
            INSERT INTO day_log (day, reviews_count, goal_met)
            VALUES ($day, 0, 0)
            ON CONFLICT(day) DO NOTHING
            """;
        log.Parameters.AddWithValue("$day", FormatDate(today));
        await log.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SetTaskDoneAsync(
        SqliteConnection connection,
        DateOnly today,
        DayTaskKind kind,
        bool done,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO day_task (day, kind, done)
            VALUES ($day, $kind, $done)
            ON CONFLICT(day, kind) DO UPDATE SET done = excluded.done
            """;
        command.Parameters.AddWithValue("$day", FormatDate(today));
        command.Parameters.AddWithValue("$kind", (int)kind);
        command.Parameters.AddWithValue("$done", done ? 1 : 0);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task RefreshGoalMetAsync(SqliteConnection connection, DateOnly today, CancellationToken cancellationToken)
    {
        var tasks = await GetTasksAsync(connection, today, cancellationToken);
        var allDone = Enum.GetValues<DayTaskKind>().All(kind => tasks.GetValueOrDefault(kind));

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO day_log (day, reviews_count, goal_met)
            VALUES ($day, 0, $goalMet)
            ON CONFLICT(day) DO UPDATE SET goal_met = excluded.goal_met
            """;
        command.Parameters.AddWithValue("$day", FormatDate(today));
        command.Parameters.AddWithValue("$goalMet", allDone ? 1 : 0);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Dictionary<DayTaskKind, bool>> GetTasksAsync(
        SqliteConnection connection,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var result = Enum.GetValues<DayTaskKind>().ToDictionary(kind => kind, _ => false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT kind, done FROM day_task WHERE day = $day";
        command.Parameters.AddWithValue("$day", FormatDate(today));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result[(DayTaskKind)reader.GetInt32(0)] = reader.GetInt32(1) != 0;
        return result;
    }

    private static async Task<bool> IsTaskDoneAsync(
        SqliteConnection connection,
        DateOnly today,
        DayTaskKind kind,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT done FROM day_task WHERE day = $day AND kind = $kind";
        command.Parameters.AddWithValue("$day", FormatDate(today));
        command.Parameters.AddWithValue("$kind", (int)kind);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is int or long && Convert.ToInt32(value) != 0;
    }

    private static async Task<int> GetReviewsCountAsync(SqliteConnection connection, DateOnly today, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT reviews_count FROM day_log WHERE day = $day";
        command.Parameters.AddWithValue("$day", FormatDate(today));
        return DbValues.Count(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task<int> GetDueCountAsync(SqliteConnection connection, DateOnly today, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM review WHERE due_on <= $today";
        command.Parameters.AddWithValue("$today", FormatDate(today));
        return DbValues.Count(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task<List<DateOnly>> GetGoalMetDaysAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var metDays = new List<DateOnly>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT day FROM day_log WHERE goal_met = 1 ORDER BY day";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            metDays.Add(DateOnly.Parse(reader.GetString(0)));
        return metDays;
    }

    private static async Task<(string Name, string Uri)?> GetMediaAsync(
        SqliteConnection connection,
        MediaKind kind,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT display_name, uri FROM media_link WHERE kind = $kind";
        command.Parameters.AddWithValue("$kind", (int)kind);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return (reader.GetString(0), reader.GetString(1));
    }

    private static string FormatDate(DateOnly day) => day.ToString("yyyy-MM-dd");
}
