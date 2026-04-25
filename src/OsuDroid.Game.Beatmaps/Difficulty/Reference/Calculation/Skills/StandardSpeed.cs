using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Utils;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class StandardSpeed(IEnumerable<Mod> mods) : StandardStrainSkill(mods)
{
    protected override int ReducedSectionCount => 5;

    private double currentStrain;
    private double maxStrain;
    private double currentRhythm;
    private const double SkillMultiplier = 1.47;
    private const double StrainDecayBase = 0.3;
    private readonly List<double> sliderStrains = [];

    public double RelevantNoteCount()
    {
        return ObjectStrains.Count == 0 || maxStrain == 0d
            ? 0d
            : ObjectStrains.Sum(strain => 1d / (1 + System.Math.Exp(-(strain / maxStrain * 12 - 6))));
    }

    public double CountTopWeightedSliders() => StrainUtils.CountTopWeightedSliders(sliderStrains, difficulty);

    protected override double StrainValueAt(StandardDifficultyHitObject current)
    {
        currentStrain *= StrainDecay(current.StrainTime);
        currentStrain += StandardSpeedEvaluator.EvaluateDifficultyOf(current, Mods) * SkillMultiplier;
        currentRhythm = StandardRhythmEvaluator.EvaluateDifficultyOf(current);
        double totalStrain = currentStrain * currentRhythm;

        maxStrain = System.Math.Max(maxStrain, totalStrain);
        ObjectStrains.Add(totalStrain);

        if (current.Obj is Slider)
        {
            sliderStrains.Add(totalStrain);
        }

        return totalStrain;
    }

    protected override double CalculateInitialStrain(double time, StandardDifficultyHitObject current) =>
        currentStrain * currentRhythm * StrainDecay(time - current.Previous(0)!.StartTime);

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);
}
