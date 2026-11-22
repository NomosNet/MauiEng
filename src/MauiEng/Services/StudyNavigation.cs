using MauiEng.Views;

namespace MauiEng.Services;

public sealed class StudyNavigation : IStudyNavigation
{
    public Task OpenAsync(int? deckId)
    {
        var route = deckId is int id
            ? $"{nameof(StudyPage)}?deckId={id}"
            : nameof(StudyPage);

        return Shell.Current.GoToAsync(route);
    }

    public Task OpenSpeakAsync() => Shell.Current.GoToAsync(nameof(SpeakPage));

    public Task OpenAddWordAsync() => Shell.Current.GoToAsync(nameof(AddWordPage));

    public Task OpenAddPhraseAsync() => Shell.Current.GoToAsync(nameof(AddPhrasePage));
}
