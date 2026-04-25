using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{

    private sealed class ActiveDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new(
            2524875,
            "2524875 LaXal - Dam Dadi Doo",
            new BeatmapDownloadProgress(128, 1024, BeatmapDownloadPhase.Downloading, 2048),
            IsActive: true);

        public Task<BeatmapDownloadResult> DownloadAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken) =>
            Task.FromResult(BeatmapDownloadResult.Failed("Not used."));

        public void CancelActiveDownload()
        {
        }
    }

    private sealed class ActiveDownloadServiceWithoutFilename : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new(
            2524875,
            null,
            new BeatmapDownloadProgress(0, null, BeatmapDownloadPhase.Downloading),
            IsActive: true);

        public Task<BeatmapDownloadResult> DownloadAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken) =>
            Task.FromResult(BeatmapDownloadResult.Failed("Not used."));

        public void CancelActiveDownload()
        {
        }
    }
}
