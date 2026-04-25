namespace OsuDroid.Game.UI.Style;

public static class OsuDroidColors
{
    public static UiColor StarRating(float? starRating)
    {
        if (starRating is null)
        {
            return UiColor.Opaque(170, 170, 170);
        }

        float rounded = MathF.Ceiling(starRating.Value * 100f) / 100f;
        if (rounded < 0.1f)
        {
            return UiColor.Opaque(170, 170, 170);
        }

        (float, UiColor)[] points = new[]
        {
            (0.1f, UiColor.Opaque(66, 144, 251)),
            (1.25f, UiColor.Opaque(79, 192, 255)),
            (2.0f, UiColor.Opaque(79, 255, 213)),
            (2.5f, UiColor.Opaque(124, 255, 79)),
            (3.3f, UiColor.Opaque(246, 240, 92)),
            (4.2f, UiColor.Opaque(255, 128, 104)),
            (4.9f, UiColor.Opaque(255, 78, 111)),
            (5.8f, UiColor.Opaque(198, 69, 184)),
            (6.7f, UiColor.Opaque(101, 99, 222)),
            (7.7f, UiColor.Opaque(24, 21, 142)),
            (9.0f, UiColor.Opaque(0, 0, 0)),
        };

        for (int i = 0; i < points.Length - 1; i++)
        {
            (float, UiColor) current = points[i];
            (float, UiColor) next = points[i + 1];
            if (rounded > next.Item1)
            {
                continue;
            }

            float amount = Math.Clamp((rounded - current.Item1) / Math.Max(0.001f, next.Item1 - current.Item1), 0f, 1f);
            return new UiColor(
                (byte)MathF.Round(current.Item2.Red + (next.Item2.Red - current.Item2.Red) * amount),
                (byte)MathF.Round(current.Item2.Green + (next.Item2.Green - current.Item2.Green) * amount),
                (byte)MathF.Round(current.Item2.Blue + (next.Item2.Blue - current.Item2.Blue) * amount),
                255);
        }

        return points[^1].Item2;
    }

    public static UiColor StarRatingText(float starRating) =>
        Interpolate(MathF.Ceiling(starRating), [
            (9.0f, UiColor.Opaque(246, 240, 92)),
            (9.9f, UiColor.Opaque(255, 128, 104)),
            (10.6f, UiColor.Opaque(255, 78, 111)),
            (11.5f, UiColor.Opaque(198, 69, 184)),
            (12.4f, UiColor.Opaque(101, 99, 222)),
        ]);

    public static UiColor StarRatingBucket(float starRating)
    {
        float rounded = MathF.Ceiling(starRating);
        return rounded < 0.1f
            ? DroidUiColors.StarNeutral
            : Interpolate(rounded, [
                (0.1f, UiColor.Opaque(66, 144, 251)),
                (1.25f, UiColor.Opaque(79, 192, 255)),
                (2.0f, UiColor.Opaque(79, 255, 213)),
                (2.5f, UiColor.Opaque(124, 255, 79)),
                (3.3f, UiColor.Opaque(246, 240, 92)),
                (4.2f, UiColor.Opaque(255, 128, 104)),
                (4.9f, UiColor.Opaque(255, 78, 111)),
                (5.8f, UiColor.Opaque(198, 69, 184)),
                (6.7f, UiColor.Opaque(101, 99, 222)),
                (7.7f, UiColor.Opaque(24, 21, 142)),
                (9.0f, DroidUiColors.Black),
            ]);
    }

    public static UiColor Interpolate(float value, ReadOnlySpan<(float Domain, UiColor Color)> gradient)
    {
        if (value <= gradient[0].Domain)
        {
            return gradient[0].Color;
        }

        if (value >= gradient[^1].Domain)
        {
            return gradient[^1].Color;
        }

        for (int index = 0; index < gradient.Length - 1; index++)
        {
            (float startDomain, UiColor startColor) = gradient[index];
            (float endDomain, UiColor endColor) = gradient[index + 1];
            if (value >= endDomain)
            {
                continue;
            }

            float t = (value - startDomain) / (endDomain - startDomain);
            return new UiColor(
                LerpByte(startColor.Red, endColor.Red, t),
                LerpByte(startColor.Green, endColor.Green, t),
                LerpByte(startColor.Blue, endColor.Blue, t),
                LerpByte(startColor.Alpha, endColor.Alpha, t));
        }

        return gradient[^1].Color;
    }

    private static byte LerpByte(byte start, byte end, float t) =>
        (byte)Math.Clamp(MathF.Round(start + (end - start) * t), 0f, 255f);
}
