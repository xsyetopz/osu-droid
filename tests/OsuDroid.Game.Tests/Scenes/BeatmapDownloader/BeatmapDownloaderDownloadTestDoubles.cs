using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    private sealed class NoOpDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new();

        public Task<BeatmapDownloadResult> DownloadAsync(
            BeatmapMirrorSet beatmapSet,
            bool withVideo,
            CancellationToken cancellationToken
        ) => Task.FromResult(BeatmapDownloadResult.Failed("Not used."));

        public void CancelActiveDownload() { }
    }
}
