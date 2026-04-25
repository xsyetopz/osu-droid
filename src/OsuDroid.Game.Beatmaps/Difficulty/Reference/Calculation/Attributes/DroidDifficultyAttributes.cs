namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;

internal sealed class DroidDifficultyAttributes : DifficultyAttributes
{
    public double TapDifficulty { get; set; }

    public double RhythmDifficulty { get; set; }

    public double ReadingDifficulty { get; set; }

    public double TapDifficultStrainCount { get; set; }

    public double FlashlightDifficultStrainCount { get; set; }

    public double ReadingDifficultNoteCount { get; set; }

    public double AverageSpeedDeltaTime { get; set; }

    public List<HighStrainSection> PossibleThreeFingeredSections { get; } = [];

    public List<DifficultSlider> DifficultSliders { get; } = [];

    public double FlashlightSliderFactor { get; set; } = 1d;

    public double VibroFactor { get; set; } = 1d;
}
