using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Evaluators;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal sealed class DroidReading(IEnumerable<Mod> mods, double clockRate, IReadOnlyList<HitObject> hitObjects) : Skill<DroidDifficultyHitObject>(mods)
{
    private readonly List<double> noteDifficulties = [];
    private const double StrainDecayBase = 0.8;
    private const double SkillMultiplier = 2;
    private double currentNoteDifficulty;
    private double difficulty;
    private double noteWeightSum;

    public override void Process(DroidDifficultyHitObject current)
    {
        currentNoteDifficulty *= StrainDecay(current.DeltaTime);
        currentNoteDifficulty += DroidReadingEvaluator.EvaluateDifficultyOf(current, clockRate, Mods) * SkillMultiplier;
        noteDifficulties.Add(currentNoteDifficulty * current.RhythmMultiplier);
    }

    public override double DifficultyValue()
    {
        if (hitObjects.Count == 0)
        {
            return 0d;
        }

        var peaks = noteDifficulties.Where(static d => d > 0).ToList();
        double reducedDuration = hitObjects[0].StartTime / clockRate + 60 * 1000;
        int reducedCount = 0;

        foreach (HitObject obj in hitObjects)
        {
            if (obj.StartTime / clockRate > reducedDuration)
            {
                break;
            }

            ++reducedCount;
        }

        for (int i = 0; i < System.Math.Min(peaks.Count, reducedCount); ++i)
        {
            peaks[i] *= System.Math.Log10(Interpolation.Linear(1d, 10d, System.Math.Clamp(i / (double)reducedCount, 0, 1)));
        }

        peaks.Sort((a, b) => b.CompareTo(a));
        difficulty = 0d;
        noteWeightSum = 0d;

        for (int i = 0; i < peaks.Count; ++i)
        {
            double weight = (1 + 1d / (1 + i)) / (System.Math.Pow(i, 0.8) + 1 + 1d / (1 + i));
            if (weight == 0d)
            {
                break;
            }

            difficulty += peaks[i] * weight;
            noteWeightSum += weight;
        }

        return difficulty;
    }

    public double CountTopWeightedNotes()
    {
        if (noteDifficulties.Count == 0 || difficulty == 0d || noteWeightSum == 0d)
        {
            return 0d;
        }

        double consistentTopNote = difficulty / noteWeightSum;
        return noteDifficulties.Sum(d => 1.1d / (1 + System.Math.Exp(-5 * (d / consistentTopNote - 1.15))));
    }

    private static double StrainDecay(double ms) => System.Math.Pow(StrainDecayBase, ms / 1000d);

    public static double DifficultyToPerformance(double difficulty) => System.Math.Pow(System.Math.Pow(difficulty, 2) * 25, 0.8);
}
