using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloadServiceTests
{
    private sealed class WritingMirrorClient(byte[] bytes) : IBeatmapMirrorClient
    {
        public IReadOnlyList<BeatmapMirrorDefinition> Mirrors { get; } =
        [new(BeatmapMirrorKind.OsuDirect, "https://osu.direct", "osu.direct", true)];

        public Uri CreateSearchUri(BeatmapMirrorSearchRequest request) =>
            new("https://osu.direct/api/v2/search");

        public Uri CreateDownloadUri(long beatmapSetId, bool withVideo) =>
            CreateDownloadUri(BeatmapMirrorKind.OsuDirect, beatmapSetId, withVideo);

        public Uri CreateDownloadUri(BeatmapMirrorKind mirror, long beatmapSetId, bool withVideo) =>
            new(
                $"https://osu.direct/api/d/{beatmapSetId}{(withVideo ? string.Empty : "?noVideo=1")}"
            );

        public Uri CreatePreviewUri(BeatmapMirrorKind mirror, long beatmapId) =>
            new($"https://osu.direct/api/media/preview/{beatmapId}");

        public Task<IReadOnlyList<BeatmapMirrorSet>> SearchAsync(
            BeatmapMirrorSearchRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<BeatmapMirrorSet>>([]);

        public async Task DownloadAsync(
            Uri source,
            string destinationPath,
            IProgress<BeatmapDownloadProgress>? progress,
            CancellationToken cancellationToken
        )
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            progress?.Report(
                new BeatmapDownloadProgress(0, bytes.Length, BeatmapDownloadPhase.Connecting)
            );
            await File.WriteAllBytesAsync(destinationPath, bytes, cancellationToken)
                .ConfigureAwait(false);
            progress?.Report(
                new BeatmapDownloadProgress(
                    bytes.Length,
                    bytes.Length,
                    BeatmapDownloadPhase.Downloading,
                    bytes.Length
                )
            );
        }
    }

    private sealed class CapturingMirrorClient(byte[] bytes) : IBeatmapMirrorClient
    {
        private IProgress<BeatmapDownloadProgress>? _progress;

        public IReadOnlyList<BeatmapMirrorDefinition> Mirrors { get; } =
        [new(BeatmapMirrorKind.OsuDirect, "https://osu.direct", "osu.direct", true)];

        public Uri CreateSearchUri(BeatmapMirrorSearchRequest request) =>
            new("https://osu.direct/api/v2/search");

        public Uri CreateDownloadUri(long beatmapSetId, bool withVideo) =>
            CreateDownloadUri(BeatmapMirrorKind.OsuDirect, beatmapSetId, withVideo);

        public Uri CreateDownloadUri(BeatmapMirrorKind mirror, long beatmapSetId, bool withVideo) =>
            new($"https://osu.direct/api/d/{beatmapSetId}");

        public Uri CreatePreviewUri(BeatmapMirrorKind mirror, long beatmapId) =>
            new($"https://osu.direct/api/media/preview/{beatmapId}");

        public Task<IReadOnlyList<BeatmapMirrorSet>> SearchAsync(
            BeatmapMirrorSearchRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<BeatmapMirrorSet>>([]);

        public Task DownloadAsync(
            Uri source,
            string destinationPath,
            IProgress<BeatmapDownloadProgress>? progress,
            CancellationToken cancellationToken
        )
        {
            _progress = progress;
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.WriteAllBytes(destinationPath, bytes);
            _progress?.Report(
                new BeatmapDownloadProgress(
                    bytes.Length,
                    bytes.Length,
                    BeatmapDownloadPhase.Downloading,
                    bytes.Length
                )
            );
            return Task.CompletedTask;
        }

        public void ReportLateProgress() =>
            _progress?.Report(
                new BeatmapDownloadProgress(
                    bytes.Length,
                    bytes.Length,
                    BeatmapDownloadPhase.Downloading,
                    bytes.Length
                )
            );
    }

    private sealed class RecordingProcessingService : IBeatmapProcessingService
    {
        private readonly List<string> _queuedArchives = [];

        public BeatmapProcessingState State { get; } = new();

        public IReadOnlyList<string> QueuedArchives => _queuedArchives;

        public BeatmapOnlineMetadata? LastMetadata { get; private set; }

        public void EnqueueArchive(string archivePath, BeatmapOnlineMetadata? metadata = null)
        {
            _queuedArchives.Add(archivePath);
            LastMetadata = metadata;
        }

        public bool HasPendingWork() => _queuedArchives.Count > 0;

        public void Start() { }

        public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
        {
            snapshot = BeatmapLibrarySnapshot.Empty;
            return false;
        }
    }
}
