namespace OsuDroid.Game.Beatmaps.Difficulty;

public interface IBeatmapDifficultyCalculator
{
    BeatmapStarRatings Calculate(string osuFilePath);
}
