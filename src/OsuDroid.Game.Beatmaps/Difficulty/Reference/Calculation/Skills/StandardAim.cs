using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Utils;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class StandardAim(IEnumerable<Mod> mods, bool withSliders) : StandardStrainSkill(mods)
{
    public bool WithSliders { get; } = withSliders;

    private double currentStrain;
    private const double SkillMultiplier = 26;
    private const double StrainDecayBase = 0.15;
    private readonly List<double> sliderStrains = [];
    private double maxSliderStrain;

    public double CountDifficultSliders()
    {
        return sliderStrains.Count == 0 ? 0d : sliderStrains.Sum(strain => 1d / (1 + System.Math.Exp(-(strain / maxSliderStrain * 12 - 6))));
    }

    public double CountTopWeightedSliders() => StrainUtils.CountTopWeightedSliders(sliderStrains, difficulty);

    protected override double StrainValueAt(StandardDifficultyHitObject current)
    {
        currentStrain *= StrainDecay(current.DeltaTime);
        currentStrain += StandardAimEvaluator.EvaluateDifficultyOf(current, WithSliders) * SkillMultiplier;

        if (current.Obj is Slider)
        {
            sliderStrains.Add(currentStrain);
            maxSliderStrain = System.Math.Max(maxSliderStrain, currentStrain);
        }

        ObjectStrains.Add(currentStrain);
        return currentStrain;
    }

    protected override double CalculateInitialStrain(double time, StandardDifficultyHitObject current) =>
        currentStrain * StrainDecay(time - current.Previous(0)!.StartTime);

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);
}
