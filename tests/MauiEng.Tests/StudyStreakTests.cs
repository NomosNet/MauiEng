using MauiEng.Core;

namespace MauiEng.Tests;

public class StudyStreakTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    [Fact]
    public void Empty_history_is_zero()
    {
        Assert.Equal(0, StudyStreak.Current([], Today));
    }

    [Fact]
    public void Today_alone_is_one()
    {
        Assert.Equal(1, StudyStreak.Current([Today], Today));
    }

    [Fact]
    public void Unbroken_days_including_today_count()
    {
        var days = new[] { Today, Today.AddDays(-1), Today.AddDays(-2) };

        Assert.Equal(3, StudyStreak.Current(days, Today));
    }

    [Fact]
    public void Yesterday_keeps_the_streak_until_today_is_closed()
    {
        var days = new[] { Today.AddDays(-1), Today.AddDays(-2) };

        Assert.Equal(2, StudyStreak.Current(days, Today));
    }

    [Fact]
    public void Gap_before_yesterday_clears_the_streak()
    {
        Assert.Equal(0, StudyStreak.Current([Today.AddDays(-2)], Today));
    }

    [Fact]
    public void Missing_yesterday_leaves_only_today()
    {
        var days = new[] { Today, Today.AddDays(-2) };

        Assert.Equal(1, StudyStreak.Current(days, Today));
    }
}
