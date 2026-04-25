using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal abstract class DroidStrainSkill(IEnumerable<Mod> mods) : StrainSkill<DroidDifficultyHitObject>(mods)
{
    protected abstract double StarsPerDouble { get; }

    public override void Process(DroidDifficultyHitObject current)
    {
        if (current.Index < 0)
        {
            return;
        }

        base.Process(current);
    }

    public override double DifficultyValue()
    {
        List<double> peaks = CurrentStrainPeaks;
        ReduceHighestStrainPeaks(peaks);
        double starsPerDoubleLog2 = System.Math.Log2(StarsPerDouble);
        difficulty = System.Math.Pow(peaks.Sum(strain => System.Math.Pow(strain, 1 / starsPerDoubleLog2)), starsPerDoubleLog2);
        return difficulty;
    }

    protected override double CalculateCurrentSectionStart(DroidDifficultyHitObject current) => current.StartTime;
}
