using System.Security.Cryptography;
using System.Text;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Runtime.Settings;

namespace OsuDroid.Game.Beatmaps;

public interface IBeatmapLibrary
{
    BeatmapLibrarySnapshot Snapshot { get; }

    BeatmapLibrarySnapshot Load();

    BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null);

    void ApplyOnlineMetadata(string setDirectory, BeatmapOnlineMetadata metadata);

    bool NeedsScanRefresh();

    BeatmapOptions GetOptions(string setDirectory);

    void SaveOptions(BeatmapOptions options);

    IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null);

    IReadOnlySet<string> GetCollectionSetDirectories(string name);

    bool CreateCollection(string name);

    void DeleteCollection(string name);

    void ToggleCollectionMembership(string name, string setDirectory);

    void DeleteBeatmapSet(string directory);

    void ClearBeatmapCache();

    void ClearProperties();
}

public sealed partial class BeatmapLibrary(
    DroidGamePathLayout paths,
    IBeatmapLibraryRepository repository,
    IGameSettingsStore? settingsStore = null
) : IBeatmapLibrary
{
    private const string StandardRulesetFilterMetadataKey = "library.standardRulesetFilterVersion";
    private const long StandardRulesetFilterVersion = 2;
    private const string BeatmapSetIndexMetadataPrefix = "library.beatmapSetIndex.";

    private BeatmapLibrarySnapshot _snapshot = BeatmapLibrarySnapshot.Empty;

    public BeatmapLibrarySnapshot Snapshot => _snapshot;

    public BeatmapLibrarySnapshot Load()
    {
        _snapshot = FilterStandardRuleset(repository.LoadLibrary());
        return _snapshot;
    }

    public bool NeedsScanRefresh() =>
        repository.GetDifficultyMetadata(StandardRulesetFilterMetadataKey)
        < StandardRulesetFilterVersion;

    public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null)
    {
        Directory.CreateDirectory(paths.Songs);
        var beatmaps = new List<BeatmapInfo>();
        var importedDirectories = repository
            .GetBeatmapSetDirectories()
            .ToHashSet(StringComparer.Ordinal);
        var existingDirectories = new HashSet<string>(StringComparer.Ordinal);
        var reindexedDirectories = new List<string>();
        var indexedStamps = new Dictionary<string, long>(StringComparer.Ordinal);
        bool forceStandardRulesetRefresh =
            forceUpdateDirectories is null
            && repository.GetDifficultyMetadata(StandardRulesetFilterMetadataKey)
                < StandardRulesetFilterVersion;

        foreach (string directory in Directory.EnumerateDirectories(paths.Songs))
        {
            string directoryName = Path.GetFileName(directory);
            existingDirectories.Add(directoryName);
            long indexStamp = GetDirectoryIndexStamp(directory);
            bool forceDirectoryUpdate = forceUpdateDirectories?.Contains(directoryName) == true;
            if (
                !forceStandardRulesetRefresh
                && !forceDirectoryUpdate
                && IsDirectoryIndexCurrent(directoryName, indexStamp)
            )
            {
                continue;
            }

            if (
                !forceStandardRulesetRefresh
                && forceUpdateDirectories is not null
                && !forceDirectoryUpdate
                && repository.IsBeatmapSetImported(directoryName)
            )
            {
                continue;
            }

            reindexedDirectories.Add(directoryName);
            BeatmapInfo[] parsedBeatmaps = ParseBeatmapSet(directory).ToArray();
            if (DeleteUnimportedBeatmaps() && parsedBeatmaps.Length == 0)
            {
                TryDeleteDirectory(directory);
            }
            else
            {
                indexedStamps[directoryName] = indexStamp;
                beatmaps.AddRange(parsedBeatmaps);
            }
        }

        if (reindexedDirectories.Count > 0)
        {
            repository.DeleteBeatmapSets(reindexedDirectories);
        }

        if (beatmaps.Count > 0)
        {
            repository.UpsertBeatmaps(beatmaps);
        }

        foreach ((string? directory, long stamp) in indexedStamps)
        {
            repository.SetDifficultyMetadata(GetDirectoryIndexMetadataKey(directory), stamp);
        }

        string[] missingDirectories = importedDirectories
            .Where(directory => !existingDirectories.Contains(directory))
            .ToArray();
        if (missingDirectories.Length > 0)
        {
            repository.DeleteBeatmapSets(missingDirectories);
        }

        if (forceStandardRulesetRefresh)
        {
            repository.SetDifficultyMetadata(
                StandardRulesetFilterMetadataKey,
                StandardRulesetFilterVersion
            );
        }

        return Load();
    }

    public void ApplyOnlineMetadata(string setDirectory, BeatmapOnlineMetadata metadata)
    {
        foreach (BeatmapOnlineDifficultyMetadata beatmap in metadata.Beatmaps)
        {
            repository.UpdateOnlineMetadata(
                setDirectory,
                beatmap.Id,
                beatmap.Version,
                (int)metadata.Status,
                null,
                beatmap.StarRating
            );
        }

        _snapshot = Load();
    }

    public BeatmapOptions GetOptions(string setDirectory) =>
        repository.GetBeatmapOptions(setDirectory);

    public void SaveOptions(BeatmapOptions options) => repository.UpsertBeatmapOptions(options);

    public IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null) =>
        repository.GetCollections(selectedSetDirectory);

    public IReadOnlySet<string> GetCollectionSetDirectories(string name) =>
        repository.GetCollectionSetDirectories(name);

    public bool CreateCollection(string name)
    {
        string trimmed = name.Trim();
        if (trimmed.Length == 0 || repository.CollectionExists(trimmed))
        {
            return false;
        }

        repository.CreateCollection(trimmed);
        return true;
    }

    public void DeleteCollection(string name) => repository.DeleteCollection(name);

    public void ToggleCollectionMembership(string name, string setDirectory)
    {
        BeatmapCollection? collection = repository
            .GetCollections(setDirectory)
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Name, name, StringComparison.Ordinal)
            );
        if (collection?.ContainsSelectedSet == true)
        {
            repository.RemoveBeatmapFromCollection(name, setDirectory);
        }
        else
        {
            repository.AddBeatmapToCollection(name, setDirectory);
        }
    }

    public void DeleteBeatmapSet(string directory)
    {
        repository.DeleteBeatmapSetData(directory);
        string path = Path.Combine(paths.Songs, directory);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        _snapshot = Load();
    }

    public void ClearBeatmapCache()
    {
        repository.ClearBeatmapCache();
        _snapshot = BeatmapLibrarySnapshot.Empty;
    }

    public void ClearProperties() => repository.ClearBeatmapOptions();

    private IEnumerable<BeatmapInfo> ParseBeatmapSet(string directory)
    {
        foreach (
            string osuFile in Directory.EnumerateFiles(
                directory,
                "*.osu",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            BeatmapInfo? beatmap = null;
            try
            {
                beatmap = BeatmapFileParser.Parse(osuFile, paths.Songs);
                DeleteUnsupportedVideoIfNeeded(osuFile);
            }
            catch (Exception)
            {
                if (DeleteUnimportedBeatmaps())
                {
                    TryDeleteFile(osuFile);
                }
            }

            if (beatmap is not null)
            {
                yield return beatmap;
            }
        }
    }

    private bool IsDirectoryIndexCurrent(string directoryName, long indexStamp) =>
        repository.GetDifficultyMetadata(GetDirectoryIndexMetadataKey(directoryName)) == indexStamp;

    private BeatmapLibrarySnapshot FilterStandardRuleset(BeatmapLibrarySnapshot source)
    {
        if (source.Sets.Count == 0)
        {
            return source;
        }

        bool changed = false;
        var sets = new List<BeatmapSetInfo>(source.Sets.Count);
        foreach (BeatmapSetInfo set in source.Sets)
        {
            BeatmapInfo[] beatmaps = set
                .Beatmaps.Where(beatmap =>
                    BeatmapFileParser.IsStandardRulesetFile(
                        Path.Combine(paths.Songs, beatmap.SetDirectory, beatmap.Filename)
                    )
                )
                .ToArray();

            if (beatmaps.Length != set.Beatmaps.Count)
            {
                changed = true;
            }

            if (beatmaps.Length > 0)
            {
                sets.Add(set with { Beatmaps = beatmaps });
            }
        }

        return changed ? new BeatmapLibrarySnapshot(sets) : source;
    }

    private static string GetDirectoryIndexMetadataKey(string directoryName)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(directoryName));
        return BeatmapSetIndexMetadataPrefix + Convert.ToHexString(hash);
    }

    private static long GetDirectoryIndexStamp(string directory)
    {
        long latest = Directory.GetLastWriteTimeUtc(directory).Ticks;
        foreach (
            string osuFile in Directory.EnumerateFiles(
                directory,
                "*.osu",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            latest = Math.Max(latest, File.GetLastWriteTimeUtc(osuFile).Ticks);
        }

        return latest;
    }
}
