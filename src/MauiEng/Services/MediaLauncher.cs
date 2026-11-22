namespace MauiEng.Services;

public sealed class MediaLauncher
{
    public async Task<bool> OpenAsync(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return false;

        try
        {
            return await Launcher.Default.OpenAsync(uri);
        }
        catch
        {
            return false;
        }
    }

    public async Task<(string Name, string Uri)?> PickBookAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите книгу",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, [".txt", ".epub", ".pdf", "text/plain", "application/epub+zip", "application/pdf"] },
                { DevicePlatform.WinUI, [".txt", ".epub", ".pdf"] }
            })
        });

        return result is null ? null : (result.FileName, result.FullPath);
    }

    public async Task<(string Name, string Uri)?> PickVideoAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите видео",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, [".mp4", ".mkv", ".avi", "video/*"] },
                { DevicePlatform.WinUI, [".mp4", ".mkv", ".avi"] }
            })
        });

        return result is null ? null : (result.FileName, result.FullPath);
    }
}
