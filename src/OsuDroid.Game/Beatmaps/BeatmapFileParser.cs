using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace OsuDroid.Game.Beatmaps;

public sealed class BeatmapFileParser
{
    public static BeatmapInfo Parse(string osuFilePath, string songsPath)
    {
        var sections = ReadSections(osuFilePath);
        var general = sections.GetValueOrDefault("General") ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var metadata = sections.GetValueOrDefault("Metadata") ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var difficulty = sections.GetValueOrDefault("Difficulty") ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var events = sections.GetValueOrDefault("Events") ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var timingPoints = sections.GetValueOrDefault("TimingPoints") ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var hitObjects = sections.GetValueOrDefault("HitObjects") ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var beatmapTimes = ParseHitObjects(hitObjects.Values);
        var bpm = ParseBpm(timingPoints.Values, beatmapTimes.LastTime);
        var setDirectory = Path.GetRelativePath(songsPath, Path.GetDirectoryName(osuFilePath) ?? songsPath);

        return new BeatmapInfo(
            Filename: Path.GetFileName(osuFilePath),
            SetDirectory: NormalizeSetDirectory(setDirectory),
            Md5: CalculateMd5(osuFilePath),
            Id: ParseLong(metadata.GetValueOrDefault("BeatmapID")),
            AudioFilename: general.GetValueOrDefault("AudioFilename") ?? string.Empty,
            BackgroundFilename: ParseBackgroundFilename(events.Values),
            Status: null,
            SetId: ParseInt(metadata.GetValueOrDefault("BeatmapSetID")),
            Title: metadata.GetValueOrDefault("Title") ?? string.Empty,
            TitleUnicode: metadata.GetValueOrDefault("TitleUnicode") ?? metadata.GetValueOrDefault("Title") ?? string.Empty,
            Artist: metadata.GetValueOrDefault("Artist") ?? string.Empty,
            ArtistUnicode: metadata.GetValueOrDefault("ArtistUnicode") ?? metadata.GetValueOrDefault("Artist") ?? string.Empty,
            Creator: metadata.GetValueOrDefault("Creator") ?? string.Empty,
            Version: metadata.GetValueOrDefault("Version") ?? string.Empty,
            Tags: metadata.GetValueOrDefault("Tags") ?? string.Empty,
            Source: metadata.GetValueOrDefault("Source") ?? string.Empty,
            DateImported: new DateTimeOffset(File.GetLastWriteTimeUtc(Path.GetDirectoryName(osuFilePath) ?? osuFilePath)).ToUnixTimeMilliseconds(),
            ApproachRate: ParseFloat(difficulty.GetValueOrDefault("ApproachRate")),
            OverallDifficulty: ParseFloat(difficulty.GetValueOrDefault("OverallDifficulty")),
            CircleSize: ParseFloat(difficulty.GetValueOrDefault("CircleSize")),
            HpDrainRate: ParseFloat(difficulty.GetValueOrDefault("HPDrainRate")),
            DroidStarRating: null,
            StandardStarRating: null,
            BpmMax: bpm.Max,
            BpmMin: bpm.Min,
            MostCommonBpm: bpm.Common,
            Length: beatmapTimes.LastTime,
            PreviewTime: ParseInt(general.GetValueOrDefault("PreviewTime")) ?? -1,
            HitCircleCount: beatmapTimes.Circles,
            SliderCount: beatmapTimes.Sliders,
            SpinnerCount: beatmapTimes.Spinners,
            MaxCombo: beatmapTimes.Circles + beatmapTimes.Sliders + beatmapTimes.Spinners,
            EpilepsyWarning: ParseInt(general.GetValueOrDefault("EpilepsyWarning")) == 1);
    }

    private static Dictionary<string, Dictionary<string, string>> ReadSections(string path)
    {
        var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string>? current = null;
        var currentSection = string.Empty;
        var lineNumber = 0;

        foreach (var rawLine in File.ReadLines(path, Encoding.UTF8))
        {
            lineNumber++;
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                continue;

            if (line[0] == '[' && line[^1] == ']')
            {
                currentSection = line[1..^1];
                current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                sections[currentSection] = current;
                continue;
            }

            if (current is null)
                continue;

            if (currentSection is "Events" or "TimingPoints" or "HitObjects")
            {
                current[lineNumber.ToString(CultureInfo.InvariantCulture)] = line;
                continue;
            }

            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
                continue;

            current[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        return sections;
    }

    private static string? ParseBackgroundFilename(IEnumerable<string> eventLines)
    {
        foreach (var line in eventLines)
        {
            if (!line.StartsWith("0,", StringComparison.Ordinal) && !line.StartsWith("Background,", StringComparison.OrdinalIgnoreCase))
                continue;

            var fields = line.Split(',');
            if (fields.Length < 3)
                continue;

            return fields[2].Trim().Trim('"');
        }

        return null;
    }

    private static BpmSummary ParseBpm(IEnumerable<string> timingLines, long lastObjectTime)
    {
        var bpms = new List<(long Time, float Bpm)>();
        foreach (var line in timingLines)
        {
            var fields = line.Split(',');
            if (fields.Length < 2)
                continue;

            var beatLength = ParseFloat(fields[1]);
            if (beatLength <= 0f)
                continue;

            bpms.Add((ParseLong(fields[0]) ?? 0, 60000f / beatLength));
        }

        if (bpms.Count == 0)
            return new BpmSummary(0f, 0f, 0f);

        var min = bpms.Min(point => point.Bpm);
        var max = bpms.Max(point => point.Bpm);
        var common = bpms
            .Select((point, index) => new
            {
                point.Bpm,
                Duration = Math.Max(0L, (index + 1 < bpms.Count ? bpms[index + 1].Time : lastObjectTime) - point.Time),
            })
            .OrderByDescending(point => point.Duration)
            .ThenBy(point => point.Bpm)
            .First().Bpm;
        return new BpmSummary(min, max, common);
    }

    private static HitObjectSummary ParseHitObjects(IEnumerable<string> objectLines)
    {
        var circles = 0;
        var sliders = 0;
        var spinners = 0;
        var lastTime = 0L;

        foreach (var line in objectLines)
        {
            var fields = line.Split(',');
            if (fields.Length < 4)
                continue;

            var time = ParseLong(fields[2]) ?? 0L;
            lastTime = Math.Max(lastTime, time);
            var type = ParseInt(fields[3]) ?? 0;
            if ((type & 1) != 0)
                circles++;
            else if ((type & 2) != 0)
                sliders++;
            else if ((type & 8) != 0)
                spinners++;
        }

        return new HitObjectSummary(circles, sliders, spinners, lastTime);
    }

    private static string CalculateMd5(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = MD5.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string NormalizeSetDirectory(string path) => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

    private static int? ParseInt(string? text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static long? ParseLong(string? text) => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static float ParseFloat(string? text) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0f;

    private readonly record struct BpmSummary(float Min, float Max, float Common);

    private readonly record struct HitObjectSummary(int Circles, int Sliders, int Spinners, long LastTime);
}
