namespace MauiEng.Core;

public sealed class PhraseItem
{
    public required int Id { get; init; }

    public required string Text { get; init; }

    public string? Translation { get; init; }

    public required DateOnly AddedOn { get; init; }
}
