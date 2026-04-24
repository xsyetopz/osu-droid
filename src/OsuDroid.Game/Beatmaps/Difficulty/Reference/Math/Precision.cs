namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

internal static class Precision
{
    public const float FloatEpsilon = 1e-3f;

    public const double DoubleEpsilon = 1e-7;

    public static bool DefinitelyBigger(float value1, float value2, float acceptableDifference = FloatEpsilon) =>
        value1 - acceptableDifference > value2;

    public static bool DefinitelyBigger(double value1, double value2, double acceptableDifference = DoubleEpsilon) =>
        value1 - acceptableDifference > value2;

    public static bool AlmostEquals(float value1, float value2, float acceptableDifference = FloatEpsilon) =>
        MathF.Abs(value1 - value2) <= acceptableDifference;

    public static bool AlmostEquals(double value1, double value2, double acceptableDifference = DoubleEpsilon) =>
        System.Math.Abs(value1 - value2) <= acceptableDifference;
}
