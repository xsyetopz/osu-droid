namespace OsuDroid.Game.Beatmaps.Difficulty;

public enum DifficultyAlgorithm
{
    Droid,
    Standard,
}

public interface IBeatmapDifficultyService
{
    DifficultyAlgorithm Algorithm { get; }

    BeatmapInfo EnsureCalculated(BeatmapInfo beatmap);

    void EnsureCalculatorVersions();
}

public sealed class BeatmapDifficultyService(
    IBeatmapLibraryRepository repository,
    string? songsPath = null,
    IBeatmapDifficultyCalculator? calculator = null,
    DifficultyAlgorithm algorithm = DifficultyAlgorithm.Droid) : IBeatmapDifficultyService
{
    public const long DroidCalculatorVersion = BeatmapDifficultyCalculator.DroidLegacyVersion + 2;
    public const long StandardCalculatorVersion = BeatmapDifficultyCalculator.StandardLegacyVersion + 2;
    private const string DroidVersionKey = "droidStarRatingVersion";
    private const string StandardVersionKey = "standardStarRatingVersion";
    private readonly IBeatmapDifficultyCalculator calculator = calculator ?? new BeatmapDifficultyCalculator();

    public DifficultyAlgorithm Algorithm { get; } = algorithm;

    public BeatmapInfo EnsureCalculated(BeatmapInfo beatmap)
    {
        EnsureCalculatorVersions();
        if (beatmap.DroidStarRating is not null && beatmap.StandardStarRating is not null)
            return beatmap;

        if (string.IsNullOrWhiteSpace(songsPath))
            return beatmap;

        var osuFilePath = Path.Combine(songsPath, beatmap.SetDirectory, beatmap.Filename);
        if (!File.Exists(osuFilePath))
            return beatmap;

        BeatmapStarRatings ratings;
        try
        {
            ratings = calculator.Calculate(osuFilePath);
        }
        catch (Exception)
        {
            ratings = new BeatmapStarRatings(null, null);
        }

        var updated = beatmap with
        {
            DroidStarRating = beatmap.DroidStarRating ?? ratings.Droid,
            StandardStarRating = beatmap.StandardStarRating ?? ratings.Standard,
        };
        repository.UpdateStarRatings(
            updated.Md5,
            updated.SetDirectory,
            updated.Filename,
            updated.DroidStarRating,
            updated.StandardStarRating);
        return updated;
    }

    public void EnsureCalculatorVersions()
    {
        if (repository.GetDifficultyMetadata(DroidVersionKey) < DroidCalculatorVersion)
        {
            repository.ResetDroidStarRatings();
            repository.SetDifficultyMetadata(DroidVersionKey, DroidCalculatorVersion);
        }

        if (repository.GetDifficultyMetadata(StandardVersionKey) < StandardCalculatorVersion)
        {
            repository.ResetStandardStarRatings();
            repository.SetDifficultyMetadata(StandardVersionKey, StandardCalculatorVersion);
        }
    }
}
