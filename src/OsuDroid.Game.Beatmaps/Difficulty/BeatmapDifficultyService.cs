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

    DifficultyVersionState EnsureCalculatorVersions();
}

public readonly record struct DifficultyVersionState(bool DroidReset, bool StandardReset)
{
    public bool AnyReset => DroidReset || StandardReset;
}

public sealed class BeatmapDifficultyService(
    IBeatmapLibraryRepository repository,
    string? songsPath = null,
    IBeatmapDifficultyCalculator? calculator = null,
    DifficultyAlgorithm algorithm = DifficultyAlgorithm.Droid
) : IBeatmapDifficultyService
{
    public const long DroidCalculatorVersion = BeatmapDifficultyCalculator.DroidReferenceVersion;
    public const long StandardCalculatorVersion =
        BeatmapDifficultyCalculator.StandardReferenceVersion;
    private const string DroidVersionKey = "droidStarRatingVersion";
    private const string StandardVersionKey = "standardStarRatingVersion";
    private readonly IBeatmapDifficultyCalculator _calculator =
        calculator ?? new BeatmapDifficultyCalculator();

    public DifficultyAlgorithm Algorithm { get; } = algorithm;

    public BeatmapInfo EnsureCalculated(BeatmapInfo beatmap)
    {
        DifficultyVersionState versionState = EnsureCalculatorVersions();
        bool needsDroidRating = versionState.DroidReset || beatmap.DroidStarRating is null;
        bool needsStandardRating = versionState.StandardReset || beatmap.StandardStarRating is null;

        if (!needsDroidRating && !needsStandardRating)
        {
            return beatmap;
        }

        if (string.IsNullOrWhiteSpace(songsPath))
        {
            return beatmap;
        }

        string osuFilePath = Path.Combine(songsPath, beatmap.SetDirectory, beatmap.Filename);
        if (!File.Exists(osuFilePath))
        {
            return beatmap;
        }

        BeatmapStarRatings ratings;
        try
        {
            ratings = _calculator.Calculate(osuFilePath);
        }
        catch (Exception)
        {
            ratings = new BeatmapStarRatings(null, null);
        }

        BeatmapInfo updated = beatmap with
        {
            DroidStarRating = needsDroidRating ? ratings.Droid : beatmap.DroidStarRating,
            StandardStarRating = needsStandardRating
                ? ratings.Standard
                : beatmap.StandardStarRating,
        };
        repository.UpdateStarRatings(
            updated.Md5,
            updated.SetDirectory,
            updated.Filename,
            updated.DroidStarRating,
            updated.StandardStarRating
        );
        return updated;
    }

    public DifficultyVersionState EnsureCalculatorVersions()
    {
        bool droidReset = false;
        bool standardReset = false;

        if (repository.GetDifficultyMetadata(DroidVersionKey) < DroidCalculatorVersion)
        {
            repository.ResetDroidStarRatings();
            repository.SetDifficultyMetadata(DroidVersionKey, DroidCalculatorVersion);
            droidReset = true;
        }

        if (repository.GetDifficultyMetadata(StandardVersionKey) < StandardCalculatorVersion)
        {
            repository.ResetStandardStarRatings();
            repository.SetDifficultyMetadata(StandardVersionKey, StandardCalculatorVersion);
            standardReset = true;
        }

        return new DifficultyVersionState(droidReset, standardReset);
    }
}
