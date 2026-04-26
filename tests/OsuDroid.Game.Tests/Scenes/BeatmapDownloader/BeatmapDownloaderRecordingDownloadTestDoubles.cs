using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    private sealed class ThrowingDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new();

        public Task<BeatmapDownloadResult> DownloadAsync(
            BeatmapMirrorSet beatmapSet,
            bool withVideo,
            CancellationToken cancellationToken
        ) => throw new InvalidOperationException("download exploded");

        public void CancelActiveDownload() { }
    }

    private sealed class RecordingDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new();

        public int CallCount { get; private set; }

        public bool? LastWithVideo { get; private set; }

        public Task<BeatmapDownloadResult> DownloadAsync(
            BeatmapMirrorSet beatmapSet,
            bool withVideo,
            CancellationToken cancellationToken
        )
        {
            CallCount++;
            LastWithVideo = withVideo;
            return Task.FromResult(BeatmapDownloadResult.Failed("recorded"));
        }

        public void CancelActiveDownload() { }
    }
}
