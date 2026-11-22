namespace MauiEng.Core;

public sealed record WidgetStats(
    int Streak,
    int TasksDone,
    int TasksTotal,
    int DueCount,
    bool CardsDone,
    bool ReadingDone,
    bool WatchingDone,
    bool SpeakingDone);
