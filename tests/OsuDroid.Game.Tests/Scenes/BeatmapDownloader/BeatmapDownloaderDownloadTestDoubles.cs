using System.Net;
using System.Reflection;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    private sealed class NoOpDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new();

        public Task<BeatmapImportResult> DownloadAndImportAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken) =>
            Task.FromResult(BeatmapImportResult.Failed("Not used."));

        public void CancelActiveDownload()
        {
        }
    }

    private sealed class ActiveDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new(
            2524875,
            "2524875 LaXal - Dam Dadi Doo",
            new BeatmapDownloadProgress(128, 1024, "Downloading", 2048),
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
