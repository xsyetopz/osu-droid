using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class DroidRhythm(IEnumerable<Mod> mods) : DroidStrainSkill(mods)
{
    protected override int ReducedSectionCount => 5;

    protected override double StarsPerDouble => 1.75;

    private double currentStrain;
    private const double StrainDecayBase = 0.3;
    private readonly bool useSliderAccuracy = mods.Any(static mod => mod is ModScoreV2);

    protected override double StrainValueAt(DroidDifficultyHitObject current)
    {
        double rhythmMultiplier = DroidRhythmEvaluator.EvaluateDifficultyOf(
            current,
            useSliderAccuracy
        );
        double doubletapness = 1 - current.GetDoubletapness(current.Next(0));
        current.RhythmMultiplier = rhythmMultiplier * doubletapness;

        currentStrain *= StrainDecay(current.DeltaTime);
        currentStrain += (rhythmMultiplier - 1) * doubletapness;
        return currentStrain;
    }

    protected override double CalculateInitialStrain(
        double time,
        DroidDifficultyHitObject current
    ) => currentStrain * StrainDecay(time - current.Previous(0)!.StartTime);

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);
}
