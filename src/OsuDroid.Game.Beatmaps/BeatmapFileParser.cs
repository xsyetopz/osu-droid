using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OsuDroid.Game.Beatmaps.Difficulty;

namespace OsuDroid.Game.Beatmaps;

public sealed class BeatmapFileParser
{
    public static BeatmapInfo Parse(string osuFilePath, string songsPath)
    {
        Dictionary<string, Dictionary<string, string>> sections = ReadSections(osuFilePath);
        Dictionary<string, string> general =
            sections.GetValueOrDefault("General")
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> metadata =
            sections.GetValueOrDefault("Metadata")
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> difficulty =
            sections.GetValueOrDefault("Difficulty")
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> events =
            sections.GetValueOrDefault("Events")
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> timingPoints =
            sections.GetValueOrDefault("TimingPoints")
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> hitObjects =
            sections.GetValueOrDefault("HitObjects")
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (GetRulesetMode(general) != 0)
        {
            throw new NotSupportedException("Only osu!standard beatmaps are supported.");
        }

        HitObjectSummary beatmapTimes = ParseHitObjects(hitObjects.Values);
        BpmSummary bpm = ParseBpm(timingPoints.Values, beatmapTimes.LastTime);
        string setDirectory = Path.GetRelativePath(
            songsPath,
            Path.GetDirectoryName(osuFilePath) ?? songsPath
        );
        BeatmapStarRatings ratings = CalculateStarRatings(osuFilePath);

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
            TitleUnicode: metadata.GetValueOrDefault("TitleUnicode")
                ?? metadata.GetValueOrDefault("Title")
                ?? string.Empty,
            Artist: metadata.GetValueOrDefault("Artist") ?? string.Empty,
            ArtistUnicode: metadata.GetValueOrDefault("ArtistUnicode")
                ?? metadata.GetValueOrDefault("Artist")
                ?? string.Empty,
            Creator: metadata.GetValueOrDefault("Creator") ?? string.Empty,
            Version: metadata.GetValueOrDefault("Version") ?? string.Empty,
            Tags: metadata.GetValueOrDefault("Tags") ?? string.Empty,
            Source: metadata.GetValueOrDefault("Source") ?? string.Empty,
            DateImported: new DateTimeOffset(
                File.GetLastWriteTimeUtc(Path.GetDirectoryName(osuFilePath) ?? osuFilePath)
            ).ToUnixTimeMilliseconds(),
            ApproachRate: ParseFloat(difficulty.GetValueOrDefault("ApproachRate")),
            OverallDifficulty: ParseFloat(difficulty.GetValueOrDefault("OverallDifficulty")),
            CircleSize: ParseFloat(difficulty.GetValueOrDefault("CircleSize")),
            HpDrainRate: ParseFloat(difficulty.GetValueOrDefault("HPDrainRate")),
            DroidStarRating: ratings.Droid,
            StandardStarRating: ratings.Standard,
            BpmMax: bpm.Max,
            BpmMin: bpm.Min,
            MostCommonBpm: bpm.Common,
            Length: beatmapTimes.LastTime,
            PreviewTime: ParseInt(general.GetValueOrDefault("PreviewTime")) ?? -1,
            HitCircleCount: beatmapTimes.Circles,
            SliderCount: beatmapTimes.Sliders,
            SpinnerCount: beatmapTimes.Spinners,
            MaxCombo: beatmapTimes.Circles + beatmapTimes.Sliders + beatmapTimes.Spinners,
            EpilepsyWarning: ParseInt(general.GetValueOrDefault("EpilepsyWarning")) == 1
        );
    }

    public static string? ParseVideoFilename(string osuFilePath)
    {
        Dictionary<string, Dictionary<string, string>> sections = ReadSections(osuFilePath);
        Dictionary<string, string> events =
            sections.GetValueOrDefault("Events")
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        return ParseVideoFilename(events.Values);
    }

    public static bool IsStandardRulesetFile(string osuFilePath)
    {
        try
        {
            return ReadRulesetMode(osuFilePath) == 0;
        }
        catch
        {
            return true;
        }
    }

    private static int ReadRulesetMode(string path)
    {
        bool inGeneral = false;
        foreach (string rawLine in File.ReadLines(path, Encoding.UTF8))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (line[0] == '[' && line[^1] == ']')
            {
                if (inGeneral)
                {
                    return 0;
                }

                inGeneral = string.Equals(
                    line[1..^1],
                    "General",
                    StringComparison.OrdinalIgnoreCase
                );
                continue;
            }

            if (!inGeneral)
            {
                continue;
            }

            int separator = line.IndexOf(':', StringComparison.Ordinal);
            if (
                separator <= 0
                || !string.Equals(
                    line[..separator].Trim(),
                    "Mode",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            return ParseInt(line[(separator + 1)..].Trim()) ?? 0;
        }

        return 0;
    }

    private static Dictionary<string, Dictionary<string, string>> ReadSections(string path)
    {
        var sections = new Dictionary<string, Dictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase
        );
        Dictionary<string, string>? current = null;
        string currentSection = string.Empty;
        int lineNumber = 0;

        foreach (string rawLine in File.ReadLines(path, Encoding.UTF8))
        {
            lineNumber++;
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (line[0] == '[' && line[^1] == ']')
            {
                currentSection = line[1..^1];
                current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                sections[currentSection] = current;
                continue;
            }

            if (current is null)
            {
                continue;
            }

            if (currentSection is "Events" or "TimingPoints" or "HitObjects")
            {
                current[lineNumber.ToString(CultureInfo.InvariantCulture)] = line;
                continue;
            }

            int separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            current[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        return sections;
    }

    private static int GetRulesetMode(IReadOnlyDictionary<string, string> general) =>
        ParseInt(general.GetValueOrDefault("Mode")) ?? 0;

    private static string? ParseBackgroundFilename(IEnumerable<string> eventLines)
    {
        foreach (string line in eventLines)
        {
            if (
                !line.StartsWith("0,", StringComparison.Ordinal)
                && !line.StartsWith("Background,", StringComparison.OrdinalIgnoreCase)
            )
            {
                continue;
            }

            string[] fields = line.Split(',');
            if (fields.Length < 3)
            {
                continue;
            }

            return fields[2].Trim().Trim('"');
        }

        return null;
    }

    private static string? ParseVideoFilename(IEnumerable<string> eventLines)
    {
        foreach (string line in eventLines)
        {
            if (
                !line.StartsWith("1,", StringComparison.Ordinal)
                && !line.StartsWith("Video,", StringComparison.OrdinalIgnoreCase)
            )
            {
                continue;
            }

            string[] fields = line.Split(',');
            if (fields.Length < 3)
            {
                continue;
            }

            return fields[2].Trim().Trim('"');
        }

        return null;
    }

    private static BpmSummary ParseBpm(IEnumerable<string> timingLines, long lastObjectTime)
    {
        var bpms = new List<(long Time, float Bpm)>();
        foreach (string line in timingLines)
        {
            string[] fields = line.Split(',');
            if (fields.Length < 2)
            {
                continue;
            }

            float beatLength = ParseFloat(fields[1]);
            if (beatLength <= 0f)
            {
                continue;
            }

            bpms.Add((ParseLong(fields[0]) ?? 0, 60000f / beatLength));
        }

        if (bpms.Count == 0)
        {
            return new BpmSummary(0f, 0f, 0f);
        }

        float min = bpms.Min(point => point.Bpm);
        float max = bpms.Max(point => point.Bpm);
        float common = bpms.Select(
                (point, index) =>
                    new
                    {
                        point.Bpm,
                        Duration = Math.Max(
                            0L,
                            (index + 1 < bpms.Count ? bpms[index + 1].Time : lastObjectTime)
                                - point.Time
                        ),
                    }
            )
            .OrderByDescending(point => point.Duration)
            .ThenBy(point => point.Bpm)
            .First()
            .Bpm;
        return new BpmSummary(min, max, common);
    }

    private static HitObjectSummary ParseHitObjects(IEnumerable<string> objectLines)
    {
        int circles = 0;
        int sliders = 0;
        int spinners = 0;
        long lastTime = 0L;

        foreach (string line in objectLines)
        {
            string[] fields = line.Split(',');
            if (fields.Length < 4)
            {
                continue;
            }

            long time = ParseLong(fields[2]) ?? 0L;
            lastTime = Math.Max(lastTime, time);
            int type = ParseInt(fields[3]) ?? 0;
            if ((type & 1) != 0)
            {
                circles++;
            }
            else if ((type & 2) != 0)
            {
                sliders++;
            }
            else if ((type & 8) != 0)
            {
                spinners++;
            }
        }

        return new HitObjectSummary(circles, sliders, spinners, lastTime);
    }

    private static string CalculateMd5(string path)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] hash = MD5.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string NormalizeSetDirectory(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

    private static BeatmapStarRatings CalculateStarRatings(string osuFilePath)
    {
        try
        {
            return new BeatmapDifficultyCalculator().Calculate(osuFilePath);
        }
        catch (Exception)
        {
            return new BeatmapStarRatings(null, null);
        }
    }

    private static int? ParseInt(string? text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? parsed
            : null;

    private static long? ParseLong(string? text) =>
        long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
            ? parsed
            : null;

    private static float ParseFloat(string? text) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
            ? parsed
            : 0f;

    private readonly record struct BpmSummary(float Min, float Max, float Common);

    private readonly record struct HitObjectSummary(
        int Circles,
        int Sliders,
        int Spinners,
        long LastTime
    );
}
