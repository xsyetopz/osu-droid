using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class StandardFlashlight(IEnumerable<Mod> mods) : StandardStrainSkill(mods)
{
    protected override int ReducedSectionCount => 0;

    protected override double ReducedSectionBaseline => 1d;

    protected override double DecayWeight => 1d;

    private double currentStrain;
    private const double SkillMultiplier = 0.05512;
    private const double StrainDecayBase = 0.15;

    protected override double StrainValueAt(StandardDifficultyHitObject current)
    {
        currentStrain *= StrainDecay(current.DeltaTime);
        currentStrain += StandardFlashlightEvaluator.EvaluateDifficultyOf(current, Mods) * SkillMultiplier;
        return currentStrain;
    }

    protected override double CalculateInitialStrain(double time, StandardDifficultyHitObject current) =>
        currentStrain * StrainDecay(time - current.Previous(0)!.StartTime);

    public override double DifficultyValue() => CurrentStrainPeaks.Sum();

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);

    public static new double DifficultyToPerformance(double difficulty) => System.Math.Pow(difficulty, 2) * 25;
}
