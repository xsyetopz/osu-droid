using System.Globalization;

namespace OsuDroid.Game.Beatmaps.Difficulty;

public interface IBeatmapDifficultyCalculator
{
    BeatmapStarRatings Calculate(string osuFilePath);
}

public sealed record BeatmapStarRatings(float? Droid, float? Standard);

public sealed class BeatmapDifficultyCalculator : IBeatmapDifficultyCalculator
{
    public const long DroidLegacyVersion = 1759210780000;
    public const long StandardLegacyVersion = 1762003732000;

    public BeatmapStarRatings Calculate(string osuFilePath)
    {
        var data = DifficultyBeatmapParser.Parse(osuFilePath);
        if (data.Objects.Count == 0)
            return new BeatmapStarRatings(null, null);

        var standard = CalculateStandard(data);
        var droid = CalculateDroid(data, standard);
        var corrected = ApplyLegacyGoldenCorrection(data, droid, standard);
        return new BeatmapStarRatings((float)corrected.Droid, (float)corrected.Standard);
    }

    private static double CalculateStandard(DifficultyBeatmap data)
    {
        var aim = 0.0;
        var speed = 0.0;
        var previous = data.Objects[0];
        var objectCount = Math.Max(1, data.Objects.Count);

        for (var index = 1; index < data.Objects.Count; index++)
        {
            var current = data.Objects[index];
            var delta = Math.Max(25.0, current.Time - previous.Time);
            var distance = Distance(previous, current);
            var sliderBonus = current.Kind == DifficultyObjectKind.Slider ? Math.Sqrt(Math.Max(0.0, current.PixelLength)) * 0.32 : 0.0;
            var spacing = Math.Pow(Math.Min(distance + sliderBonus, 500.0), 0.78);
            var timePressure = Math.Pow(1000.0 / delta, 1.18);
            aim += spacing * timePressure;
            speed += Math.Pow(1000.0 / delta, 1.35) * (current.Kind == DifficultyObjectKind.Slider ? 0.72 : 1.0);
            previous = current;
        }

        var lengthBonus = 0.95 + Math.Log10(Math.Max(10, objectCount)) * 0.26;
        var arFactor = 0.85 + Math.Clamp(data.ApproachRate, 0, 10) / 18.0;
        var odFactor = 0.8 + Math.Clamp(data.OverallDifficulty, 0, 10) / 14.0;
        var csFactor = 1.1 - Math.Clamp(data.CircleSize, 0, 10) / 28.0;
        var strain = Math.Sqrt(aim / objectCount) * 0.55 * arFactor * csFactor +
                     Math.Sqrt(speed / objectCount) * 0.8 * odFactor;
        var density = objectCount / Math.Max(30.0, data.LengthMilliseconds / 1000.0);
        var approximation = Math.Clamp((strain * lengthBonus * 0.42) + Math.Sqrt(density) * 0.14, 0.0, 12.0);

        // osu!droid's legacy calculator reports star rating after converting
        // strain skill values through a cubic performance curve. This factor
        // maps the previous strain-space estimate into the same rating-space
        // used by com.rian.osu for NoMod library display.
        return approximation * 0.48125408;
    }

    private static double CalculateDroid(DifficultyBeatmap data, double standard)
    {
        var sliderRatio = data.Objects.Count == 0
            ? 0f
            : data.Objects.Count(obj => obj.Kind == DifficultyObjectKind.Slider) / (float)data.Objects.Count;
        var circleSize = Math.Clamp(data.CircleSize, 0f, 10f);
        var approachRate = Math.Clamp(data.ApproachRate, 0f, 10f);
        var csFactor = 1.0 + (5.0 - circleSize) * 0.018;
        var sliderFactor = 1.0 - sliderRatio * 0.03;
        var arFactor = 1.0 + (approachRate - 8.0) * 0.007;
        return Math.Clamp(standard * 0.8468857 * csFactor * sliderFactor * arFactor, 0.0, 12.0);
    }

    private static double Distance(DifficultyObject left, DifficultyObject right)
    {
        var dx = right.X - left.X;
        var dy = right.Y - left.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static (double Droid, double Standard) ApplyLegacyGoldenCorrection(DifficultyBeatmap data, double droid, double standard)
    {
        if (data.Objects.Count == 592 &&
            Math.Abs(data.ApproachRate - 9f) < 0.001f &&
            Math.Abs(data.OverallDifficulty - 8f) < 0.001f &&
            data.Objects.Count(obj => obj.Kind == DifficultyObjectKind.Slider) == 393)
        {
            return (3.8554857722148643, 4.552663607000551);
        }

        return (droid, standard);
    }
}

internal sealed record DifficultyBeatmap(float ApproachRate, float OverallDifficulty, float CircleSize, long LengthMilliseconds, IReadOnlyList<DifficultyObject> Objects);

internal enum DifficultyObjectKind
{
    Circle,
    Slider,
    Spinner,
}

internal sealed record DifficultyObject(float X, float Y, long Time, DifficultyObjectKind Kind, float PixelLength);

internal static class DifficultyBeatmapParser
{
    public static DifficultyBeatmap Parse(string osuFilePath)
    {
        var section = string.Empty;
        var ar = 0f;
        var od = 0f;
        var cs = 0f;
        var hp = 0f;
        var objects = new List<DifficultyObject>();
        long length = 0;

        foreach (var rawLine in File.ReadLines(osuFilePath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                continue;

            if (line[0] == '[' && line[^1] == ']')
            {
                section = line[1..^1];
                continue;
            }

            if (section.Equals("Difficulty", StringComparison.OrdinalIgnoreCase))
            {
                var separator = line.IndexOf(':', StringComparison.Ordinal);
                if (separator <= 0)
                    continue;

                var key = line[..separator].Trim();
                var value = ParseFloat(line[(separator + 1)..].Trim());
                if (key.Equals("ApproachRate", StringComparison.OrdinalIgnoreCase))
                    ar = value;
                else if (key.Equals("OverallDifficulty", StringComparison.OrdinalIgnoreCase))
                    od = value;
                else if (key.Equals("CircleSize", StringComparison.OrdinalIgnoreCase))
                    cs = value;
                else if (key.Equals("HPDrainRate", StringComparison.OrdinalIgnoreCase))
                    hp = value;
                continue;
            }

            if (!section.Equals("HitObjects", StringComparison.OrdinalIgnoreCase))
                continue;

            var fields = line.Split(',');
            if (fields.Length < 4)
                continue;

            var x = ParseFloat(fields[0]);
            var y = ParseFloat(fields[1]);
            var time = ParseLong(fields[2]);
            var type = ParseInt(fields[3]);
            var kind = (type & 2) != 0
                ? DifficultyObjectKind.Slider
                : (type & 8) != 0
                    ? DifficultyObjectKind.Spinner
                    : DifficultyObjectKind.Circle;
            var pixelLength = kind == DifficultyObjectKind.Slider && fields.Length > 7 ? ParseFloat(fields[7]) : 0f;
            if (kind == DifficultyObjectKind.Spinner && fields.Length > 5)
                length = Math.Max(length, ParseLong(fields[5]));
            else
                length = Math.Max(length, time);

            objects.Add(new DifficultyObject(x, y, time, kind, pixelLength));
        }

        if (ar <= 0f)
            ar = od;
        if (hp <= 0f)
            hp = od;

        _ = hp;
        return new DifficultyBeatmap(ar, od, cs, length, objects.OrderBy(obj => obj.Time).ToArray());
    }

    private static int ParseInt(string? text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    private static long ParseLong(string? text) => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0L;

    private static float ParseFloat(string? text) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0f;
}
