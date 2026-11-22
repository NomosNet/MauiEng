using Android.App;
using Android.Content;
using MauiEng.Core;
using MauiEng.Services;

namespace MauiEng;

public sealed class AndroidStatsWidgetUpdater : IStatsWidgetUpdater
{
    public void Publish(WidgetStats stats)
    {
        var context = Android.App.Application.Context;
        WidgetStatsStore.Save(context, stats);

        var intent = new Intent(context, typeof(StatsWidgetProvider));
        intent.SetAction(WidgetStatsStore.ActionUpdate);
        context.SendBroadcast(intent);
    }
}
