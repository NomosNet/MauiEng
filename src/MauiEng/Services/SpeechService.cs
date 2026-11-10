namespace MauiEng.Services;

public sealed class SpeechService
{
    private CancellationTokenSource? _speech;

    public async Task SpeakAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        _speech?.Cancel();
        _speech?.Dispose();
        var speech = new CancellationTokenSource();
        _speech = speech;

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = locales.FirstOrDefault(item =>
                item.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));

            await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
            {
                Locale = locale,
                Volume = 1,
                Pitch = 1
            }, speech.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
        }
    }
}
