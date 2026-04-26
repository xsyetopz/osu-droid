using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal abstract class StandardStrainSkill(IEnumerable<Mod> mods)
    : StrainSkill<StandardDifficultyHitObject>(mods)
{
    protected virtual double DecayWeight => 0.9;

    public override double DifficultyValue()
    {
        List<double> peaks = CurrentStrainPeaks;
        ReduceHighestStrainPeaks(peaks);
        peaks.Sort((a, b) => b.CompareTo(a));

        difficulty = 0d;
        double weight = 1d;

        foreach (double strain in peaks)
        {
            difficulty += strain * weight;
            weight *= DecayWeight;
        }

        return difficulty;
    }
}
