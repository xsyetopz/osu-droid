using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    private sealed class ImportingDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new(
            2524875,
            "2524875 LaXal - Dam Dadi Doo",
            new BeatmapDownloadProgress(1024, 1024, BeatmapDownloadPhase.Importing),
            IsActive: true);

        public Task<BeatmapImportResult> DownloadAndImportAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken) =>
            Task.FromResult(BeatmapImportResult.Failed("Not used."));

        public void CancelActiveDownload()
        {
        }
    }

    private sealed class ImmediateSuccessDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new();

        public Task<BeatmapImportResult> DownloadAndImportAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken) =>
            Task.FromResult(BeatmapImportResult.Success("100 Artist - Title"));

        public void CancelActiveDownload()
        {
        }
    }
}
