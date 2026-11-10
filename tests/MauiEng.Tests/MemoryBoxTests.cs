using MauiEng.Core;

namespace MauiEng.Tests;

public class MemoryBoxTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    [Fact]
    public void Unseen_card_is_new()
    {
        var state = ReviewState.New(Today);

        Assert.Equal(MemoryBox.New, MemoryBoxes.Classify(state));
        Assert.Equal("Новое", MemoryBoxes.Label(state));
    }

    [Fact]
    public void Short_interval_is_short_memory()
    {
        var state = new ReviewState(2.5, 3, 2, Today);

        Assert.Equal(MemoryBox.Short, MemoryBoxes.Classify(state));
        Assert.Equal("Короткая память", MemoryBoxes.Label(state));
    }

    [Fact]
    public void Longer_interval_is_long_memory()
    {
        var state = new ReviewState(2.5, 8, 3, Today);

        Assert.Equal(MemoryBox.Long, MemoryBoxes.Classify(state));
        Assert.Equal("Долгая память", MemoryBoxes.Label(state));
    }

    [Fact]
    public void Forgotten_card_is_short_memory()
    {
        var state = Sm2.ApplyForgot(ReviewState.New(Today), Today);

        Assert.Equal(MemoryBox.Short, MemoryBoxes.Classify(state));
    }
}
