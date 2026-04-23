using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{
    private sealed class FakeLibrary(BeatmapLibrarySnapshot snapshot) : IBeatmapLibrary
    {
        private readonly Dictionary<string, BeatmapOptions> options = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> collections = new(StringComparer.Ordinal);
        private BeatmapLibrarySnapshot snapshot = snapshot;

        public BeatmapLibrarySnapshot Snapshot => snapshot;

        public int ScanCallCount { get; private set; }

        public TimeSpan ScanDelay { get; set; }

        public bool NeedsRefresh { get; set; }

        public BeatmapLibrarySnapshot Load() => snapshot;

        public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null)
        {
            ScanCallCount++;
            NeedsRefresh = false;
            if (ScanDelay > TimeSpan.Zero)
                Thread.Sleep(ScanDelay);
            return snapshot;
        }

        public bool NeedsScanRefresh() => NeedsRefresh;

        public BeatmapOptions GetOptions(string setDirectory) => options.TryGetValue(setDirectory, out var value) ? value : new BeatmapOptions(setDirectory);

        public void SaveOptions(BeatmapOptions nextOptions) => options[nextOptions.SetDirectory] = nextOptions;

        public IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null) => collections
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new BeatmapCollection(pair.Key, pair.Value.Count, selectedSetDirectory is not null && pair.Value.Contains(selectedSetDirectory)))
            .ToArray();

        public IReadOnlySet<string> GetCollectionSetDirectories(string name) => collections.TryGetValue(name, out var sets)
            ? new HashSet<string>(sets, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        public bool CreateCollection(string name)
        {
            if (collections.ContainsKey(name))
                return false;

            collections[name] = new HashSet<string>(StringComparer.Ordinal);
            return true;
        }

        public void DeleteCollection(string name) => collections.Remove(name);

        public void ToggleCollectionMembership(string name, string setDirectory)
        {
            if (!collections.TryGetValue(name, out var sets))
                collections[name] = sets = new HashSet<string>(StringComparer.Ordinal);

            if (!sets.Add(setDirectory))
                sets.Remove(setDirectory);
        }

        public void DeleteBeatmapSet(string directory)
        {
            snapshot = new BeatmapLibrarySnapshot(snapshot.Sets.Where(set => !string.Equals(set.Directory, directory, StringComparison.Ordinal)).ToArray());
            options.Remove(directory);
            foreach (var sets in collections.Values)
                sets.Remove(directory);
        }
    }
}
