using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Calculators;

namespace OsuDroid.Game.Beatmaps.Difficulty;

public sealed class BeatmapDifficultyCalculator : IBeatmapDifficultyCalculator
{
    public const long DroidReferenceVersion = DroidDifficultyCalculator.Version;
    public const long StandardReferenceVersion = StandardDifficultyCalculator.Version;

    public BeatmapStarRatings Calculate(string osuFilePath)
    {
        Beatmap beatmap = ReferenceBeatmapParser.Parse(osuFilePath);
        if (beatmap.HitObjects.Objects.Count == 0)
        {
            return new BeatmapStarRatings(null, null);
        }

        double droid = new DroidDifficultyCalculator().Calculate(beatmap).StarRating;
        double standard = new StandardDifficultyCalculator().Calculate(beatmap).StarRating;
        return new BeatmapStarRatings(RoundStarRating(droid), RoundStarRating(standard));
    }

    private static float RoundStarRating(double value) =>
        (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
