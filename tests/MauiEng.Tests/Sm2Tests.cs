using MauiEng.Core;

namespace MauiEng.Tests;

public class Sm2Tests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    [Fact]
    public void Remember_first_success_is_one_day()
    {
        var next = Sm2.ApplyRemember(ReviewState.New(Today), Today);

        Assert.Equal(1, next.IntervalDays);
        Assert.Equal(1, next.SuccessfulReps);
        Assert.Equal(Sm2.InitialEase, next.Ease);
        Assert.Equal(Today.AddDays(1), next.Due);
    }

    [Fact]
    public void Remember_second_success_is_three_days()
    {
        var once = Sm2.ApplyRemember(ReviewState.New(Today), Today);

        var twice = Sm2.ApplyRemember(once, Today);

        Assert.Equal(3, twice.IntervalDays);
        Assert.Equal(2, twice.SuccessfulReps);
        Assert.Equal(Today.AddDays(3), twice.Due);
    }

    [Fact]
    public void Remember_later_success_multiplies_interval_by_ease()
    {
        var state = ReviewState.New(Today);
        state = Sm2.ApplyRemember(state, Today);
        state = Sm2.ApplyRemember(state, Today);

        var third = Sm2.ApplyRemember(state, Today);

        Assert.Equal(8, third.IntervalDays);
        Assert.Equal(3, third.SuccessfulReps);
        Assert.Equal(Today.AddDays(8), third.Due);
        Assert.Equal(Sm2.InitialEase, third.Ease);
    }

    [Fact]
    public void Forgot_resets_reps_and_lowers_ease()
    {
        var next = Sm2.ApplyForgot(ReviewState.New(Today), Today);

        Assert.Equal(2.3, next.Ease);
        Assert.Equal(1, next.IntervalDays);
        Assert.Equal(0, next.SuccessfulReps);
        Assert.Equal(Today.AddDays(1), next.Due);
    }

    [Fact]
    public void Forgot_does_not_drop_ease_below_floor()
    {
        var state = new ReviewState(1.4, 8, 4, Today);

        var next = Sm2.ApplyForgot(state, Today);

        Assert.Equal(Sm2.MinEase, next.Ease);
        Assert.Equal(1, next.IntervalDays);
        Assert.Equal(0, next.SuccessfulReps);
    }

    [Fact]
    public void Remember_after_forgot_starts_again_at_one_day()
    {
        var forgotten = Sm2.ApplyForgot(ReviewState.New(Today), Today);

        var next = Sm2.ApplyRemember(forgotten, Today);

        Assert.Equal(1, next.IntervalDays);
        Assert.Equal(1, next.SuccessfulReps);
        Assert.Equal(2.3, next.Ease);
    }
}
