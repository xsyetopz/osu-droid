namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

internal static class Interpolation
{
    public static double Linear(double start, double end, double amount) => start + (end - start) * amount;

    public static float Linear(float start, float end, float amount) => start + (end - start) * amount;

    public static ReferenceVector2 Linear(ReferenceVector2 start, ReferenceVector2 end, float amount) => new(
        Linear(start.X, end.X, amount),
        Linear(start.Y, end.Y, amount));

    public static double ReverseLinear(double x, double start, double end) => System.Math.Clamp((x - start) / (end - start), 0.0, 1.0);

    public static float Damp(float start, float end, float @base, float exponent)
    {
        return @base is < 0 or > 1
            ? throw new ArgumentOutOfRangeException(nameof(@base), "Base must be in the range [0, 1].")
            : Linear(start, end, 1f - MathF.Pow(@base, exponent));
    }

    public static float DampContinuously(float current, float target, float halfTime, float elapsedTime) =>
        Damp(current, target, 0.5f, elapsedTime / halfTime);
}
