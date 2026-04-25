namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;

internal sealed class StandardDifficultyAttributes : DifficultyAttributes
{
    public double SpeedDifficulty { get; set; }

    public double SpeedDifficultStrainCount { get; set; }

    public double SpeedTopWeightedSliderFactor { get; set; }

    public double ApproachRate { get; set; }
}
