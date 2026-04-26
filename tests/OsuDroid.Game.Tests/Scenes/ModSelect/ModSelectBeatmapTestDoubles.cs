using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;

namespace OsuDroid.Game.Tests;

public sealed partial class ModSelectSceneTests
{
    private sealed class NoPendingBeatmapProcessingService : IBeatmapProcessingService
    {
        public BeatmapProcessingState State { get; } = new();

        public bool HasPendingWork() => false;

        public void EnqueueArchive(string archivePath, BeatmapOnlineMetadata? metadata = null) { }

        public void Start() { }

        public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
        {
            snapshot = BeatmapLibrarySnapshot.Empty;
            return false;
        }
    }

    private sealed class SingleBeatmapLibrary : IBeatmapLibrary
    {
        private readonly BeatmapLibrarySnapshot _snapshot = new([
            new BeatmapSetInfo(
                1,
                "1 Artist - Title",
                [
                    new BeatmapInfo(
                        "Easy.osu",
                        "1 Artist - Title",
                        "md5",
                        null,
                        "audio.mp3",
                        null,
                        null,
                        1,
                        "Title",
                        string.Empty,
                        "Artist",
                        string.Empty,
                        "Mapper",
                        "Easy",
                        string.Empty,
                        string.Empty,
                        0,
                        5,
                        5,
                        5,
                        5,
                        1,
                        1,
                        120,
                        120,
                        120,
                        1000,
                        0,
                        1,
                        0,
                        0,
                        1,
                        false
                    ),
                ]
            ),
        ]);

        public BeatmapLibrarySnapshot Snapshot => _snapshot;

        public BeatmapLibrarySnapshot Load() => _snapshot;

        public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null) =>
            _snapshot;

        public void ApplyOnlineMetadata(string setDirectory, BeatmapOnlineMetadata metadata) { }

        public bool NeedsScanRefresh() => false;

        public BeatmapOptions GetOptions(string setDirectory) => new(setDirectory);

        public void SaveOptions(BeatmapOptions options) { }

        public IReadOnlyList<BeatmapCollection> GetCollections(
            string? selectedSetDirectory = null
        ) => [];

        public IReadOnlySet<string> GetCollectionSetDirectories(string name) =>
            new HashSet<string>(StringComparer.Ordinal);

        public bool CreateCollection(string name) => true;

        public void DeleteCollection(string name) { }

        public void ToggleCollectionMembership(string name, string setDirectory) { }

        public void DeleteBeatmapSet(string directory) { }

        public void ClearBeatmapCache() { }

        public void ClearProperties() { }
    }
}
