namespace MauiEng.Core;

public sealed class DeckSummary
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required int WordCount { get; init; }

    public required int DueCount { get; init; }

    public string Summary => $"{WordCount} {WordsLabel(WordCount)} · {DueCount} к повторению";

    private static string WordsLabel(int count)
    {
        var lastTwo = count % 100;
        if (lastTwo is >= 11 and <= 14)
            return "слов";

        return (count % 10) switch
        {
            1 => "слово",
            2 or 3 or 4 => "слова",
            _ => "слов"
        };
    }
}
