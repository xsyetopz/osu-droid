namespace OsuDroid.Game.Beatmaps;

public interface IBeatmapLibraryRepository
{
    void UpsertBeatmaps(IReadOnlyList<BeatmapInfo> beatmaps);

    void DeleteBeatmapSets(IReadOnlyList<string> directories);

    void DeleteBeatmapSetData(string directory);

    void UpdateStarRatings(string md5, string setDirectory, string filename, float? droidStarRating, float? standardStarRating);

    long GetDifficultyMetadata(string key);

    void SetDifficultyMetadata(string key, long value);

    void ResetDroidStarRatings();

    void ResetStandardStarRatings();

    bool IsBeatmapSetImported(string directory);

    IReadOnlyList<string> GetBeatmapSetDirectories();

    BeatmapLibrarySnapshot LoadLibrary();

    BeatmapOptions GetBeatmapOptions(string setDirectory);

    void UpsertBeatmapOptions(BeatmapOptions options);

    IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null);

    IReadOnlySet<string> GetCollectionSetDirectories(string name);

    bool CollectionExists(string name);

    void CreateCollection(string name);

    void DeleteCollection(string name);

    void AddBeatmapToCollection(string name, string setDirectory);

    void RemoveBeatmapFromCollection(string name, string setDirectory);
}
