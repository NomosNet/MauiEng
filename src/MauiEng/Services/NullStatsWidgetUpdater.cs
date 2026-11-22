using MauiEng.Core;

namespace MauiEng.Services;

public sealed class NullStatsWidgetUpdater : IStatsWidgetUpdater
{
    public void Publish(WidgetStats stats)
    {
    }
}
