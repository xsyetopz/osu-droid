using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class DroidFlashlight(IEnumerable<Mod> mods, bool withSliders)
    : DroidStrainSkill(mods)
{
    protected override double StarsPerDouble => 1.06;

    protected override int ReducedSectionCount => 0;

    protected override double ReducedSectionBaseline => 1d;

    public bool WithSliders { get; } = withSliders;

    private double currentStrain;
    private const double SkillMultiplier = 0.023;
    private const double StrainDecayBase = 0.15;

    public override double DifficultyValue() => CurrentStrainPeaks.Sum() * StarsPerDouble;

    protected override double StrainValueAt(DroidDifficultyHitObject current)
    {
        currentStrain *= StrainDecay(current.DeltaTime);
        currentStrain +=
            DroidFlashlightEvaluator.EvaluateDifficultyOf(current, Mods, WithSliders)
            * SkillMultiplier;
        ObjectStrains.Add(currentStrain);
        return currentStrain;
    }

    protected override double CalculateInitialStrain(
        double time,
        DroidDifficultyHitObject current
    ) => currentStrain * StrainDecay(time - current.Previous(0)!.StartTime);

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);

    public static new double DifficultyToPerformance(double difficulty) =>
        System.Math.Pow(difficulty, 1.6) * 25;
}
