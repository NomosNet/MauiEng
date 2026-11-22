using MauiEng.Core;

namespace MauiEng.Services;

public interface IStatsWidgetUpdater
{
    void Publish(WidgetStats stats);
}
