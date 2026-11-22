namespace MauiEng.Core;

public sealed record DaySnapshot(
    int Streak,
    int ReviewsToday,
    int Goal,
    int DueCount,
    int TasksDone,
    int TasksTotal,
    bool CardsDone,
    bool ReadingDone,
    bool WatchingDone,
    bool SpeakingDone,
    string? BookName,
    string? BookUri,
    string? VideoName,
    string? VideoUri);
