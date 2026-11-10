namespace MauiEng.Core;

public sealed class WordCard
{
    public required int Id { get; init; }

    public required int DeckId { get; init; }

    public required string DeckName { get; init; }

    public required string Term { get; init; }

    public required string Translation { get; init; }

    public string? Transcription { get; init; }

    public string? Example { get; init; }

    public required ReviewState Review { get; set; }
}
