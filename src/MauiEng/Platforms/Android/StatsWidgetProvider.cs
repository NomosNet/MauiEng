using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

namespace MauiEng;

[BroadcastReceiver(Label = "MauiEng — Сегодня", Exported = true)]
[IntentFilter([AppWidgetManager.ActionAppwidgetUpdate, WidgetStatsStore.ActionUpdate])]
[MetaData(AppWidgetManager.MetaDataAppwidgetProvider, Resource = "@xml/stats_widget_info")]
public sealed class StatsWidgetProvider : AppWidgetProvider
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        base.OnReceive(context, intent);
        if (context is null || intent?.Action != WidgetStatsStore.ActionUpdate)
            return;

        var manager = AppWidgetManager.GetInstance(context);
        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(StatsWidgetProvider)));
        var ids = manager?.GetAppWidgetIds(component);
        if (ids is { Length: > 0 })
            OnUpdate(context, manager, ids);
    }

    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context is null || appWidgetManager is null || appWidgetIds is null)
            return;

        var stats = WidgetStatsStore.Load(context);
        foreach (var id in appWidgetIds)
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.stats_widget);
            views.SetTextViewText(Resource.Id.widget_streak, $"🔥 {stats.Streak}");
            views.SetTextViewText(Resource.Id.widget_progress, $"{stats.TasksDone} / {stats.TasksTotal}");
            views.SetTextViewText(Resource.Id.widget_due, $"Карточки: {stats.DueCount}");
            views.SetTextViewText(
                Resource.Id.widget_blocks,
                $"{Mark(stats.CardsDone)} {Mark(stats.ReadingDone)} {Mark(stats.WatchingDone)} {Mark(stats.SpeakingDone)}");

            var launch = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName!);
            if (launch is not null)
            {
                launch.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                var flags = PendingIntentFlags.UpdateCurrent;
                if (OperatingSystem.IsAndroidVersionAtLeast(23))
                    flags |= PendingIntentFlags.Immutable;

                var pending = PendingIntent.GetActivity(context, 0, launch, flags);
                views.SetOnClickPendingIntent(Resource.Id.widget_root, pending);
            }

            appWidgetManager.UpdateAppWidget(id, views);
        }
    }

    private static string Mark(bool done) => done ? "●" : "○";
}
