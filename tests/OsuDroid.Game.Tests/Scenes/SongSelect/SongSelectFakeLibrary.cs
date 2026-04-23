using OsuDroid.Game.Beatmaps;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{
    private sealed class FakeLibrary(BeatmapLibrarySnapshot snapshot) : IBeatmapLibrary
    {
        private readonly Dictionary<string, BeatmapOptions> _options = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _collections = new(StringComparer.Ordinal);
        private BeatmapLibrarySnapshot _snapshot = snapshot;

        public BeatmapLibrarySnapshot Snapshot => _snapshot;

        public int ScanCallCount { get; private set; }

        public TimeSpan ScanDelay { get; set; }

        public bool NeedsRefresh { get; set; }

        public BeatmapLibrarySnapshot Load() => _snapshot;

        public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null)
        {
            ScanCallCount++;
            NeedsRefresh = false;
            if (ScanDelay > TimeSpan.Zero)
            {
                Thread.Sleep(ScanDelay);
            }

            return _snapshot;
        }

        public bool NeedsScanRefresh() => NeedsRefresh;

        public BeatmapOptions GetOptions(string setDirectory) => _options.TryGetValue(setDirectory, out BeatmapOptions? value) ? value : new BeatmapOptions(setDirectory);

        public void SaveOptions(BeatmapOptions nextOptions) => _options[nextOptions.SetDirectory] = nextOptions;

        public IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null) => _collections
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new BeatmapCollection(pair.Key, pair.Value.Count, selectedSetDirectory is not null && pair.Value.Contains(selectedSetDirectory)))
            .ToArray();

        public IReadOnlySet<string> GetCollectionSetDirectories(string name) => _collections.TryGetValue(name, out HashSet<string>? sets)
            ? new HashSet<string>(sets, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        public bool CreateCollection(string name)
        {
            if (_collections.ContainsKey(name))
            {
                return false;
            }

            _collections[name] = new HashSet<string>(StringComparer.Ordinal);
            return true;
        }

        public void DeleteCollection(string name) => _collections.Remove(name);

        public void ToggleCollectionMembership(string name, string setDirectory)
        {
            if (!_collections.TryGetValue(name, out HashSet<string>? sets))
            {
                _collections[name] = sets = new HashSet<string>(StringComparer.Ordinal);
            }

            if (!sets.Add(setDirectory))
            {
                sets.Remove(setDirectory);
            }
        }

        public void DeleteBeatmapSet(string directory)
        {
            _snapshot = new BeatmapLibrarySnapshot(_snapshot.Sets.Where(set => !string.Equals(set.Directory, directory, StringComparison.Ordinal)).ToArray());
            _options.Remove(directory);
            foreach (HashSet<string> sets in _collections.Values)
            {
                sets.Remove(directory);
            }
        }
    }
}
