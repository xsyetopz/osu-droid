namespace OsuDroid.Game.Beatmaps.Online;

public sealed record BeatmapDownloadState(
    long? ActiveSetId = null,
    string? Filename = null,
    BeatmapDownloadProgress? Progress = null,
    string? ErrorMessage = null,
    bool IsActive = false,
    long SessionId = 0);

public sealed record BeatmapDownloadResult(bool IsSuccess, string? ArchivePath, string? ErrorMessage)
{
    public static BeatmapDownloadResult Success(string archivePath) => new(true, archivePath, null);

    public static BeatmapDownloadResult Failed(string errorMessage) => new(false, null, errorMessage);
}

