using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Attributes;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class DroidAim(IEnumerable<Mod> mods, bool withSliders) : DroidStrainSkill(mods)
{
    protected override double StarsPerDouble => 1.05;

    public bool WithSliders { get; } = withSliders;

    public List<DifficultSlider> SliderVelocities { get; } = [];

    private readonly List<double> sliderStrains = [];
    private double maxSliderStrain;
    private double currentStrain;
    private const double SkillMultiplier = 26.5;
    private const double StrainDecayBase = 0.15;

    public double CountDifficultSliders()
    {
        return sliderStrains.Count == 0 || maxSliderStrain == 0d
            ? 0d
            : sliderStrains.Sum(strain =>
                1d / (1 + System.Math.Exp(-(strain / maxSliderStrain * 12 - 6)))
            );
    }

    protected override double StrainValueAt(DroidDifficultyHitObject current)
    {
        currentStrain *= StrainDecay(current.DeltaTime);
        currentStrain +=
            DroidAimEvaluator.EvaluateDifficultyOf(current, WithSliders) * SkillMultiplier;

        double velocity = current.TravelDistance / current.TravelTime;
        if (velocity > 0)
        {
            SliderVelocities.Add(new DifficultSlider(current.Index + 1, velocity));
        }

        if (current.Obj is Slider)
        {
            sliderStrains.Add(currentStrain);
            maxSliderStrain = System.Math.Max(maxSliderStrain, currentStrain);
        }

        ObjectStrains.Add(currentStrain);
        return currentStrain;
    }

    protected override double CalculateInitialStrain(
        double time,
        DroidDifficultyHitObject current
    ) => currentStrain * StrainDecay(time - current.Previous(0)!.StartTime);

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);

    public static new double DifficultyToPerformance(double difficulty) =>
        StrainSkill<DroidDifficultyHitObject>.DifficultyToPerformance(
            System.Math.Pow(difficulty, 0.8)
        );
}
