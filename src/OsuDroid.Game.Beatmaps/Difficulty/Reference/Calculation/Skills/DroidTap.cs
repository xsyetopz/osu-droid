using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class DroidTap(IEnumerable<Mod> mods, bool considerCheesability, double? strainTimeCap = null) : DroidStrainSkill(mods)
{
    protected override double StarsPerDouble => 1.1;

    public bool ConsiderCheesability { get; } = considerCheesability;

    public double? StrainTimeCap { get; } = strainTimeCap;

    private double currentStrain;
    private double currentRhythm;
    private const double SkillMultiplier = 1.375;
    private const double StrainDecayBase = 0.3;
    private readonly List<double> objectDeltaTimes = [];
    private double maxStrain;

    public double RelevantNoteCount()
    {
        return ObjectStrains.Count == 0 || maxStrain == 0d
            ? 0d
            : ObjectStrains.Sum(strain => 1d / (1 + System.Math.Exp(-(strain / maxStrain * 12 - 6))));
    }

    public double RelevantDeltaTime()
    {
        if (ObjectStrains.Count == 0 || maxStrain == 0d)
        {
            return 0d;
        }

        double numerator = 0d;
        double denominator = 0d;

        for (int i = 0; i < objectDeltaTimes.Count; ++i)
        {
            numerator += objectDeltaTimes[i] / (1 + System.Math.Exp(-(ObjectStrains[i] / maxStrain * 25 - 20)));
            denominator += 1d / (1 + System.Math.Exp(-(ObjectStrains[i] / maxStrain * 25 - 20)));
        }

        return denominator == 0d ? 0d : numerator / denominator;
    }

    protected override double StrainValueAt(DroidDifficultyHitObject current)
    {
        currentStrain *= StrainDecay(current.StrainTime);
        currentStrain += DroidTapEvaluator.EvaluateDifficultyOf(current, ConsiderCheesability, StrainTimeCap) * SkillMultiplier;
        currentRhythm = current.RhythmMultiplier;

        double totalStrain = currentStrain * currentRhythm;
        maxStrain = System.Math.Max(maxStrain, totalStrain);
        ObjectStrains.Add(totalStrain);
        objectDeltaTimes.Add(current.DeltaTime);
        return totalStrain;
    }

    protected override double CalculateInitialStrain(double time, DroidDifficultyHitObject current) =>
        currentStrain * currentRhythm * StrainDecay(time - current.Previous(0)!.StartTime);

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);
}
