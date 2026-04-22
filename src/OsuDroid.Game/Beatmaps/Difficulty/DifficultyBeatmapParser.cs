using System.Globalization;
namespace OsuDroid.Game.Beatmaps.Difficulty;

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
