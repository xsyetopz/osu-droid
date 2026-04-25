using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Skills;

internal abstract class StrainSkill<TObject>(IEnumerable<Mod> mods) : Skill<TObject>(mods)
    where TObject : DifficultyHitObject
{
    protected virtual int ReducedSectionCount => 10;

    protected virtual double ReducedSectionBaseline => 0.75;

    public List<double> ObjectStrains { get; } = [];

    protected double difficulty;

    private readonly List<double> strainPeaks = [];
    private double currentSectionPeak;
    private double currentSectionEnd;
    private const int sectionLength = 400;

    public override void Process(TObject current)
    {
        if (current.Index == 0)
        {
            currentSectionEnd = CalculateCurrentSectionStart(current);
        }

        while (current.StartTime > currentSectionEnd)
        {
            SaveCurrentPeak();
            StartNewSectionFrom(currentSectionEnd, current);
            currentSectionEnd += sectionLength;
        }

        currentSectionPeak = System.Math.Max(StrainValueAt(current), currentSectionPeak);
    }

    protected List<double> CurrentStrainPeaks
    {
        get
        {
            List<double> peaks = [.. strainPeaks];
            peaks.Add(currentSectionPeak);
            return peaks;
        }
    }

    public double CountTopWeightedStrains()
    {
        if (difficulty == 0d)
        {
            return 0d;
        }

        double consistentTopStrain = difficulty / 10d;
        return consistentTopStrain == 0d
            ? ObjectStrains.Count
            : ObjectStrains.Sum(strain => 1.1d / (1 + System.Math.Exp(-10 * (strain / consistentTopStrain - 0.88))));
    }

    protected void ReduceHighestStrainPeaks(List<double> peaks)
    {
        int[] highestIndices = Enumerable.Repeat(-1, System.Math.Min(peaks.Count, ReducedSectionCount)).ToArray();
        if (highestIndices.Length == 0)
        {
            return;
        }

        for (int i = 0; i < peaks.Count; ++i)
        {
            double strain = peaks[i];
            int lowestIndex = highestIndices[^1];
            double lowestStrain = lowestIndex > -1 ? peaks[lowestIndex] : 0d;
            if (strain <= lowestStrain)
            {
                continue;
            }

            int insertionIndex = Array.FindIndex(highestIndices, index => strain > (index > -1 ? peaks[index] : 0d));
            for (int j = highestIndices.Length - 1; j > insertionIndex; --j)
            {
                highestIndices[j] = highestIndices[j - 1];
            }

            highestIndices[insertionIndex] = i;
        }

        for (int i = 0; i < highestIndices.Length; ++i)
        {
            int index = highestIndices[i];
            if (index == -1)
            {
                continue;
            }

            double scale = System.Math.Log10(Interpolation.Linear(1d, 10d, i / (double)ReducedSectionCount));
            peaks[index] *= Interpolation.Linear(ReducedSectionBaseline, 1d, scale);
        }
    }

    protected virtual double CalculateCurrentSectionStart(TObject current) =>
        System.Math.Ceiling(current.StartTime / sectionLength) * sectionLength;

    protected abstract double StrainValueAt(TObject current);

    protected abstract double CalculateInitialStrain(double time, TObject current);

    private void SaveCurrentPeak() => strainPeaks.Add(currentSectionPeak);

    private void StartNewSectionFrom(double time, TObject current) =>
        currentSectionPeak = CalculateInitialStrain(time, current);

    public static double DifficultyToPerformance(double difficultyValue) =>
        System.Math.Pow(5 * System.Math.Max(1d, difficultyValue / 0.0675d) - 4, 3) / 100000d;
}
