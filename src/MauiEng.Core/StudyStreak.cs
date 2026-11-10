namespace MauiEng.Core;

public static class StudyStreak
{
    public const int DailyGoal = 20;

    public static int Current(IEnumerable<DateOnly> goalMetDays, DateOnly today)
    {
        var met = goalMetDays as IReadOnlySet<DateOnly> ?? goalMetDays.ToHashSet();
        if (!met.Contains(today) && !met.Contains(today.AddDays(-1)))
            return 0;

        var day = met.Contains(today) ? today : today.AddDays(-1);
        var count = 0;
        while (met.Contains(day))
        {
            count++;
            day = day.AddDays(-1);
        }

        return count;
    }
}
