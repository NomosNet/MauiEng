using Android.Content;
using MauiEng.Core;

namespace MauiEng;

internal static class WidgetStatsStore
{
    public const string PrefsName = "mauieng_widget_stats";
    public const string ActionUpdate = "com.companyname.mauieng.UPDATE_STATS_WIDGET";

    private const string StreakKey = "streak";
    private const string TasksDoneKey = "tasks_done";
    private const string TasksTotalKey = "tasks_total";
    private const string DueKey = "due";
    private const string CardsKey = "cards";
    private const string ReadingKey = "reading";
    private const string WatchingKey = "watching";
    private const string SpeakingKey = "speaking";

    public static void Save(Context context, WidgetStats stats)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private)!;
        using var editor = prefs.Edit()!;
        editor.PutInt(StreakKey, stats.Streak);
        editor.PutInt(TasksDoneKey, stats.TasksDone);
        editor.PutInt(TasksTotalKey, stats.TasksTotal);
        editor.PutInt(DueKey, stats.DueCount);
        editor.PutBoolean(CardsKey, stats.CardsDone);
        editor.PutBoolean(ReadingKey, stats.ReadingDone);
        editor.PutBoolean(WatchingKey, stats.WatchingDone);
        editor.PutBoolean(SpeakingKey, stats.SpeakingDone);
        editor.Apply();
    }

    public static WidgetStats Load(Context context)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private)!;
        return new WidgetStats(
            prefs.GetInt(StreakKey, 0),
            prefs.GetInt(TasksDoneKey, 0),
            prefs.GetInt(TasksTotalKey, StudyStreak.TasksPerDay),
            prefs.GetInt(DueKey, 0),
            prefs.GetBoolean(CardsKey, false),
            prefs.GetBoolean(ReadingKey, false),
            prefs.GetBoolean(WatchingKey, false),
            prefs.GetBoolean(SpeakingKey, false));
    }
}
