using System.IO.Compression;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class BeatmapDownloadServiceTests
{
    [Test]
    public async Task NoVideoDownloadMovesValidatedArchiveAndEnqueuesImport()
    {
        string root = CreateTempDirectory();
        try
        {
            var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
            paths.EnsureDirectories();
            var processingService = new RecordingProcessingService();
            var downloadService = new BeatmapDownloadService(paths, new WritingMirrorClient(CreateOszBytes()), processingService);

            BeatmapDownloadResult downloadResult = await downloadService.DownloadAsync(CreateSet(hasVideo: true), withVideo: false, CancellationToken.None).ConfigureAwait(false);

            Assert.That(downloadResult.IsSuccess, Is.True);
            Assert.That(downloadResult.ArchivePath, Is.Not.Null);
            Assert.That(Path.GetFileName(downloadResult.ArchivePath!), Is.EqualTo("123 Artist - Title [no video].osz"));
            Assert.That(File.Exists(downloadResult.ArchivePath!), Is.True);
            Assert.That(File.Exists(downloadResult.ArchivePath! + ".download"), Is.False);
            Assert.That(processingService.QueuedArchives, Is.EqualTo(new[] { downloadResult.ArchivePath! }));
            Assert.That(processingService.LastMetadata, Is.Not.Null);
            Assert.That(processingService.LastMetadata!.SetId, Is.EqualTo(123));
            Assert.That(processingService.LastMetadata.Beatmaps.Single().StarRating, Is.EqualTo(2.1f));
            Assert.That(downloadService.State.IsActive, Is.False);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public async Task InvalidNoVideoDownloadDoesNotPublishArchiveOrImportWork()
    {
        string root = CreateTempDirectory();
        try
        {
            var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
            paths.EnsureDirectories();
            var processingService = new RecordingProcessingService();
            var downloadService = new BeatmapDownloadService(paths, new WritingMirrorClient("not a zip"u8.ToArray()), processingService);

            BeatmapDownloadResult downloadResult = await downloadService.DownloadAsync(CreateSet(hasVideo: true), withVideo: false, CancellationToken.None).ConfigureAwait(false);

            Assert.That(downloadResult.IsSuccess, Is.False);
            Assert.That(downloadResult.ErrorMessage, Is.EqualTo("Downloaded beatmap archive is invalid."));
            Assert.That(Directory.EnumerateFiles(paths.Downloads), Is.Empty);
            Assert.That(processingService.QueuedArchives, Is.Empty);
            Assert.That(downloadService.State.ErrorMessage, Is.EqualTo("Downloaded beatmap archive is invalid."));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Test]
    public async Task LateProgressAfterSuccessDoesNotReactivateDownloadState()
    {
        string root = CreateTempDirectory();
        try
        {
            var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
            paths.EnsureDirectories();
            var mirror = new CapturingMirrorClient(CreateOszBytes());
            var downloadService = new BeatmapDownloadService(paths, mirror, new RecordingProcessingService());

            BeatmapDownloadResult downloadResult = await downloadService.DownloadAsync(CreateSet(hasVideo: true), withVideo: true, CancellationToken.None).ConfigureAwait(false);
            mirror.ReportLateProgress();

            Assert.That(downloadResult.IsSuccess, Is.True);
            Assert.That(downloadService.State.IsActive, Is.False);
            Assert.That(downloadService.State.Progress, Is.Null);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    private static BeatmapMirrorSet CreateSet(bool hasVideo) => new(
        BeatmapMirrorKind.OsuDirect,
        123,
        "Title",
        "Title",
        "Artist",
        "Artist",
        BeatmapRankedStatus.Ranked,
        "Mapper",
        null,
        hasVideo,
        [new BeatmapMirrorBeatmap(456, "Normal", 2.1f, 5, 4, 5, 5, 120, 90, 10, 20, 0, 0)]);

    private static byte[] CreateOszBytes()
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            ZipArchiveEntry entry = archive.CreateEntry("map.osu");
            using StreamWriter writer = new(entry.Open());
            writer.WriteLine("osu file format v14");
        }

        return stream.ToArray();
    }

    private static string CreateTempDirectory()
    {
        string root = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"download-service-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }

    private sealed class WritingMirrorClient(byte[] bytes) : IBeatmapMirrorClient
    {
        public IReadOnlyList<BeatmapMirrorDefinition> Mirrors { get; } =
        [
            new(BeatmapMirrorKind.OsuDirect, "https://osu.direct", "osu.direct", "osu-direct", true),
        ];

        public Uri CreateSearchUri(BeatmapMirrorSearchRequest request) => new("https://osu.direct/api/v2/search");

        public Uri CreateDownloadUri(long beatmapSetId, bool withVideo) => CreateDownloadUri(BeatmapMirrorKind.OsuDirect, beatmapSetId, withVideo);

        public Uri CreateDownloadUri(BeatmapMirrorKind mirror, long beatmapSetId, bool withVideo) => new($"https://osu.direct/api/d/{beatmapSetId}{(withVideo ? string.Empty : "?noVideo=1")}");

        public Uri CreatePreviewUri(BeatmapMirrorKind mirror, long beatmapId) => new($"https://osu.direct/api/media/preview/{beatmapId}");

        public Task<IReadOnlyList<BeatmapMirrorSet>> SearchAsync(BeatmapMirrorSearchRequest request, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BeatmapMirrorSet>>([]);

        public async Task DownloadAsync(Uri source, string destinationPath, IProgress<BeatmapDownloadProgress>? progress, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            progress?.Report(new BeatmapDownloadProgress(0, bytes.Length, BeatmapDownloadPhase.Connecting));
            await File.WriteAllBytesAsync(destinationPath, bytes, cancellationToken).ConfigureAwait(false);
            progress?.Report(new BeatmapDownloadProgress(bytes.Length, bytes.Length, BeatmapDownloadPhase.Downloading, bytes.Length));
        }
    }

    private sealed class CapturingMirrorClient(byte[] bytes) : IBeatmapMirrorClient
    {
        private IProgress<BeatmapDownloadProgress>? _progress;

        public IReadOnlyList<BeatmapMirrorDefinition> Mirrors { get; } =
        [
            new(BeatmapMirrorKind.OsuDirect, "https://osu.direct", "osu.direct", "osu-direct", true),
        ];

        public Uri CreateSearchUri(BeatmapMirrorSearchRequest request) => new("https://osu.direct/api/v2/search");

        public Uri CreateDownloadUri(long beatmapSetId, bool withVideo) => CreateDownloadUri(BeatmapMirrorKind.OsuDirect, beatmapSetId, withVideo);

        public Uri CreateDownloadUri(BeatmapMirrorKind mirror, long beatmapSetId, bool withVideo) => new($"https://osu.direct/api/d/{beatmapSetId}");

        public Uri CreatePreviewUri(BeatmapMirrorKind mirror, long beatmapId) => new($"https://osu.direct/api/media/preview/{beatmapId}");

        public Task<IReadOnlyList<BeatmapMirrorSet>> SearchAsync(BeatmapMirrorSearchRequest request, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BeatmapMirrorSet>>([]);

        public Task DownloadAsync(Uri source, string destinationPath, IProgress<BeatmapDownloadProgress>? progress, CancellationToken cancellationToken)
        {
            _progress = progress;
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.WriteAllBytes(destinationPath, bytes);
            _progress?.Report(new BeatmapDownloadProgress(bytes.Length, bytes.Length, BeatmapDownloadPhase.Downloading, bytes.Length));
            return Task.CompletedTask;
        }

        public void ReportLateProgress() => _progress?.Report(new BeatmapDownloadProgress(bytes.Length, bytes.Length, BeatmapDownloadPhase.Downloading, bytes.Length));
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

        public void Start()
        {
        }

        public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
        {
            snapshot = BeatmapLibrarySnapshot.Empty;
            return false;
        }
    }
}
