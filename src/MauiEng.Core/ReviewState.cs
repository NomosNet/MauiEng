namespace MauiEng.Core;

public sealed record ReviewState(double Ease, int IntervalDays, int SuccessfulReps, DateOnly Due)
{
    public static ReviewState New(DateOnly today) => new(Sm2.InitialEase, 0, 0, today);
}
