namespace OsuDroid.Game.Beatmaps.Online;

public interface IBeatmapDownloadService
{
    BeatmapDownloadState State { get; }

    Task<BeatmapDownloadResult> DownloadAsync(
        BeatmapMirrorSet beatmapSet,
        bool withVideo,
        CancellationToken cancellationToken
    );

    void CancelActiveDownload();
}
