using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;

internal abstract class DifficultyAttributes
{
    public double ClockRate { get; set; } = 1d;

    public ISet<Mod> Mods { get; set; } = new HashSet<Mod>();

    public double StarRating { get; set; }

    public int MaxCombo { get; set; }

    public double AimDifficulty { get; set; }

    public double FlashlightDifficulty { get; set; }

    public double SpeedNoteCount { get; set; }

    public double AimSliderFactor { get; set; }

    public double AimDifficultStrainCount { get; set; }

    public double AimDifficultSliderCount { get; set; }

    public double AimTopWeightedSliderFactor { get; set; }

    public double OverallDifficulty { get; set; }

    public int HitCircleCount { get; set; }

    public int SliderCount { get; set; }

    public int SpinnerCount { get; set; }
}
