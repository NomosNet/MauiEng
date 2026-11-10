namespace MauiEng.Core;

public static class Sm2
{
    public const double InitialEase = 2.5;
    public const double MinEase = 1.3;
    public const double EasePenalty = 0.2;

    public static ReviewState ApplyRemember(ReviewState state, DateOnly today)
    {
        var interval = state.SuccessfulReps switch
        {
            0 => 1,
            1 => 3,
            _ => Math.Max(1, (int)Math.Round(state.IntervalDays * state.Ease, MidpointRounding.AwayFromZero))
        };

        return state with
        {
            IntervalDays = interval,
            SuccessfulReps = state.SuccessfulReps + 1,
            Due = today.AddDays(interval)
        };
    }

    public static ReviewState ApplyForgot(ReviewState state, DateOnly today)
    {
        var ease = Math.Max(
            MinEase,
            Math.Round(state.Ease - EasePenalty, 2, MidpointRounding.AwayFromZero));

        return state with
        {
            Ease = ease,
            IntervalDays = 1,
            SuccessfulReps = 0,
            Due = today.AddDays(1)
        };
    }
}
