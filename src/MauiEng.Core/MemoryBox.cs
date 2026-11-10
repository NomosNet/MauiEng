namespace MauiEng.Core;

public enum MemoryBox
{
    New,
    Short,
    Long
}

public static class MemoryBoxes
{
    public static MemoryBox Classify(ReviewState state)
    {
        if (state.SuccessfulReps == 0 && state.IntervalDays == 0)
            return MemoryBox.New;

        if (state.IntervalDays <= 3)
            return MemoryBox.Short;

        return MemoryBox.Long;
    }

    public static string Label(ReviewState state) => Classify(state) switch
    {
        MemoryBox.New => "Новое",
        MemoryBox.Short => "Короткая память",
        MemoryBox.Long => "Долгая память",
        _ => "Новое"
    };
}
