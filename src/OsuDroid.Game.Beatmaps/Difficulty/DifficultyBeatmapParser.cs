using System.Globalization;

namespace OsuDroid.Game.Beatmaps.Difficulty;

internal static class DifficultyBeatmapParser
{
    public static DifficultyBeatmap Parse(string osuFilePath)
    {
        string section = string.Empty;
        float ar = 0f;
        float od = 0f;
        float cs = 0f;
        float hp = 0f;
        var objects = new List<DifficultyObject>();
        long length = 0;

        foreach (string rawLine in File.ReadLines(osuFilePath))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (line[0] == '[' && line[^1] == ']')
            {
                section = line[1..^1];
                continue;
            }

            if (section.Equals("Difficulty", StringComparison.OrdinalIgnoreCase))
            {
                int separator = line.IndexOf(':', StringComparison.Ordinal);
                if (separator <= 0)
                {
                    continue;
                }

                string key = line[..separator].Trim();
                float value = ParseFloat(line[(separator + 1)..].Trim());
                if (key.Equals("ApproachRate", StringComparison.OrdinalIgnoreCase))
                {
                    ar = value;
                }
                else if (key.Equals("OverallDifficulty", StringComparison.OrdinalIgnoreCase))
                {
                    od = value;
                }
                else if (key.Equals("CircleSize", StringComparison.OrdinalIgnoreCase))
                {
                    cs = value;
                }
                else if (key.Equals("HPDrainRate", StringComparison.OrdinalIgnoreCase))
                {
                    hp = value;
                }

                continue;
            }

            if (!section.Equals("HitObjects", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string[] fields = line.Split(',');
            if (fields.Length < 4)
            {
                continue;
            }

            float x = ParseFloat(fields[0]);
            float y = ParseFloat(fields[1]);
            long time = ParseLong(fields[2]);
            int type = ParseInt(fields[3]);
            DifficultyObjectKind kind =
                (type & 2) != 0 ? DifficultyObjectKind.Slider
                : (type & 8) != 0 ? DifficultyObjectKind.Spinner
                : DifficultyObjectKind.Circle;
            float pixelLength =
                kind == DifficultyObjectKind.Slider && fields.Length > 7
                    ? ParseFloat(fields[7])
                    : 0f;
            length =
                kind == DifficultyObjectKind.Spinner && fields.Length > 5
                    ? Math.Max(length, ParseLong(fields[5]))
                    : Math.Max(length, time);

            objects.Add(new DifficultyObject(x, y, time, kind, pixelLength));
        }

        if (ar <= 0f)
        {
            ar = od;
        }

        if (hp <= 0f)
        {
            hp = od;
        }

        _ = hp;
        return new DifficultyBeatmap(
            ar,
            od,
            cs,
            length,
            objects.OrderBy(obj => obj.Time).ToArray()
        );
    }

    private static int ParseInt(string? text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? parsed
            : 0;

    private static long ParseLong(string? text) =>
        long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
            ? parsed
            : 0L;

    private static float ParseFloat(string? text) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
            ? parsed
            : 0f;
}
