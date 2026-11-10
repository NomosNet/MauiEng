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
}
