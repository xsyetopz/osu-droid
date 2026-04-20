using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Beatmaps;

public interface IBeatmapLibrary
{
    BeatmapLibrarySnapshot Snapshot { get; }

    BeatmapLibrarySnapshot Load();

    BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null);
}

public sealed class BeatmapLibrary(
    DroidGamePathLayout paths,
    IBeatmapLibraryRepository repository) : IBeatmapLibrary
{
    private BeatmapLibrarySnapshot snapshot = BeatmapLibrarySnapshot.Empty;

    public BeatmapLibrarySnapshot Snapshot => snapshot;

    public BeatmapLibrarySnapshot Load()
    {
        snapshot = repository.LoadLibrary();
        return snapshot;
    }

    public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null)
    {
        Directory.CreateDirectory(paths.Songs);
        var beatmaps = new List<BeatmapInfo>();
        var importedDirectories = repository.GetBeatmapSetDirectories().ToHashSet(StringComparer.Ordinal);
        var existingDirectories = new HashSet<string>(StringComparer.Ordinal);

        foreach (var directory in Directory.EnumerateDirectories(paths.Songs))
        {
            var directoryName = Path.GetFileName(directory);
            existingDirectories.Add(directoryName);
            if (forceUpdateDirectories is null && repository.IsBeatmapSetImported(directoryName))
                continue;

            if (forceUpdateDirectories is not null && !forceUpdateDirectories.Contains(directoryName) && repository.IsBeatmapSetImported(directoryName))
                continue;

            beatmaps.AddRange(ParseBeatmapSet(directory));
        }

        if (forceUpdateDirectories is not null && forceUpdateDirectories.Count > 0)
            repository.DeleteBeatmapSets(forceUpdateDirectories.ToArray());

        if (beatmaps.Count > 0)
            repository.UpsertBeatmaps(beatmaps);

        var missingDirectories = importedDirectories.Where(directory => !existingDirectories.Contains(directory)).ToArray();
        if (missingDirectories.Length > 0)
            repository.DeleteBeatmapSets(missingDirectories);

        return Load();
    }

    private IEnumerable<BeatmapInfo> ParseBeatmapSet(string directory)
    {
        foreach (var osuFile in Directory.EnumerateFiles(directory, "*.osu", SearchOption.TopDirectoryOnly))
        {
            BeatmapInfo? beatmap = null;
            try
            {
                beatmap = BeatmapFileParser.Parse(osuFile, paths.Songs);
            }
            catch (Exception)
            {
            }

            if (beatmap is not null)
                yield return beatmap;
        }
    }
}
