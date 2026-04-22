namespace OsuDroid.Game.UI;

public static class OsuDroidColors
{
    public static UiColor StarRating(float? starRating)
    {
        if (starRating is null)
            return UiColor.Opaque(170, 170, 170);

        var rounded = MathF.Ceiling(starRating.Value * 100f) / 100f;
        if (rounded < 0.1f)
            return UiColor.Opaque(170, 170, 170);

        var points = new[]
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

        for (var i = 0; i < points.Length - 1; i++)
        {
            var current = points[i];
            var next = points[i + 1];
            if (rounded > next.Item1)
                continue;

            var amount = Math.Clamp((rounded - current.Item1) / Math.Max(0.001f, next.Item1 - current.Item1), 0f, 1f);
            return new UiColor(
                (byte)MathF.Round(current.Item2.Red + (next.Item2.Red - current.Item2.Red) * amount),
                (byte)MathF.Round(current.Item2.Green + (next.Item2.Green - current.Item2.Green) * amount),
                (byte)MathF.Round(current.Item2.Blue + (next.Item2.Blue - current.Item2.Blue) * amount),
                255);
        }

        return points[^1].Item2;
    }
}
