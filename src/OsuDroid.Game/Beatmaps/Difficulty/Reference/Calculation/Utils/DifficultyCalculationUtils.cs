using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Calculation.Utils;

internal static class DifficultyCalculationUtils
{
    public static double BpmToMilliseconds(double bpm, int delimiter = 4) => 60000d / bpm / delimiter;

    public static double MillisecondsToBpm(double milliseconds, int delimiter = 4) => 60000d / (milliseconds * delimiter);

    public static double Logistic(double exponent, double maxValue = 1d) => maxValue / (1 + System.Math.Exp(exponent));

    public static double Logistic(double x, double midpointOffset, double multiplier, double maxValue = 1d) =>
        maxValue / (1 + System.Math.Exp(-multiplier * (x - midpointOffset)));

    public static double Smoothstep(double x, double start, double end)
    {
        double t = Interpolation.ReverseLinear(x, start, end);
        return t * t * (3 - 2 * t);
    }

    public static double Smootherstep(double x, double start, double end)
    {
        double t = Interpolation.ReverseLinear(x, start, end);
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    public static double SmoothstepBellCurve(double x, double mean = 0.5, double width = 0.5)
    {
        double adjusted = x - mean;
        adjusted = adjusted > 0 ? width - adjusted : width + adjusted;
        return Smoothstep(adjusted, 0, width);
    }
}
