using System.Globalization;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Mods;
using OsuDroid.Game.Beatmaps.Difficulty.Reference.Rulesets;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;

internal sealed class Beatmap
{
    public int FormatVersion { get; set; } = 14;

    public double StackLeniency { get; set; } = 0.7;

    public ReferenceBeatmapDifficulty Difficulty { get; set; } = new();

    public BeatmapControlPoints ControlPoints { get; set; } = new();

    public BeatmapHitObjects HitObjects { get; set; } = new([]);

    public DroidPlayableBeatmap CreateDroidPlayableBeatmap(IEnumerable<Mod>? mods) => new(this, GameMode.Droid, mods);

    public StandardPlayableBeatmap CreateStandardPlayableBeatmap(IEnumerable<Mod>? mods) => new(this, GameMode.Standard, mods);
}

internal sealed class BeatmapHitObjects(IReadOnlyList<HitObject> objects)
{
    public IReadOnlyList<HitObject> Objects { get; } = objects;

    public int CircleCount => Objects.Count(static obj => obj is HitCircle);

    public int SliderCount => Objects.Count(static obj => obj is Slider);

    public int SpinnerCount => Objects.Count(static obj => obj is Spinner);
}

internal sealed class ModCollection
{
    private readonly Dictionary<Type, Mod> mods = [];

    public ModCollection(IEnumerable<Mod>? source)
    {
        if (source is null)
        {
            return;
        }

        foreach (Mod mod in source)
        {
            mods[mod.GetType()] = mod;
        }
    }

    public IReadOnlyCollection<Mod> Values => mods.Values;

    public bool Contains(Type type) => mods.Values.Any(type.IsInstanceOfType);
}

internal abstract class PlayableBeatmap
{
    protected PlayableBeatmap(Beatmap beatmap, GameMode mode, IEnumerable<Mod>? mods)
    {
        Difficulty = new ReferenceBeatmapDifficulty(
            beatmap.Difficulty.DifficultyCircleSize,
            beatmap.Difficulty.ApproachRate,
            beatmap.Difficulty.OverallDifficulty,
            beatmap.Difficulty.HealthDrainRate)
        {
            GameplayCircleSize = beatmap.Difficulty.GameplayCircleSize,
            SliderMultiplier = beatmap.Difficulty.SliderMultiplier,
            SliderTickRate = beatmap.Difficulty.SliderTickRate,
        };

        ControlPoints = beatmap.ControlPoints;
        Mods = new ModCollection(mods);
        SpeedMultiplier = Mods.Values.OfType<IReferenceModApplicableToTrackRate>()
            .Aggregate(1f, static (rate, mod) => mod.ApplyToRate(double.PositiveInfinity, rate));

        foreach (var difficultyAdjust in Mods.Values.OfType<ReferenceModDifficultyAdjust>())
        {
            difficultyAdjust.ApplyDefaultValues(Difficulty);
        }

        ReferenceModApplicator.ApplyModsToBeatmapDifficulty(
            Difficulty,
            mode,
            Mods.Values.OfType<ReferenceMod>(),
            withRateChange: true);

        HitObject[] objects = beatmap.HitObjects.Objects.Select(obj => CloneAndApply(obj, Difficulty, ControlPoints, mode)).ToArray();
        ApplyStacking(objects, beatmap.FormatVersion, beatmap.StackLeniency);
        HitObjects = new BeatmapHitObjects(objects);
        MaxCombo = HitObjects.CircleCount + HitObjects.SliderCount + HitObjects.Objects.OfType<Slider>().Sum(static slider => slider.NestedHitObjects.Count - 1);
    }

    public ReferenceBeatmapDifficulty Difficulty { get; }

    public BeatmapControlPoints ControlPoints { get; }

    public BeatmapHitObjects HitObjects { get; }

    public ModCollection Mods { get; }

    public float SpeedMultiplier { get; }

    public int MaxCombo { get; }

    private static HitObject CloneAndApply(HitObject obj, ReferenceBeatmapDifficulty difficulty, BeatmapControlPoints controlPoints, GameMode mode)
    {
        HitObject clone = obj switch
        {
            HitCircle circle => new HitCircle(circle.StartTime, circle.Position),
            Spinner spinner => new Spinner(spinner.StartTime, spinner.EndTime, spinner.Position),
            Slider slider => new Slider(slider.StartTime, slider.Position, slider.RepeatCount, slider.Path),
            _ => throw new InvalidOperationException($"Unsupported hit object {obj.GetType().Name}."),
        };

        clone.ApplyDefaults(difficulty, controlPoints, mode);
        return clone;
    }

    private static void ApplyStacking(HitObject[] objects, int formatVersion, double stackLeniency)
    {
        foreach (HitObject obj in objects)
        {
            obj.DifficultyStackHeight = 0;
        }

        if (objects.Length == 0)
        {
            return;
        }

        if (formatVersion < 6)
        {
            ApplyOldStacking(objects, stackLeniency);
            return;
        }

        const float stackDistanceSquared = 9f;
        int startIndex = 0;
        int endIndex = objects.Length - 1;
        int extendedEndIndex = endIndex;

        for (int i = endIndex; i > startIndex; i--)
        {
            int n = i;
            HitObject objectI = objects[i];
            if (objectI.DifficultyStackHeight != 0 || objectI is Spinner)
            {
                continue;
            }

            double stackThreshold = CalculateStackThreshold(objectI, stackLeniency);
            if (objectI is HitCircle)
            {
                while (--n >= 0)
                {
                    HitObject objectN = objects[n];
                    if (objectN is Spinner)
                    {
                        continue;
                    }

                    if ((int)objectI.StartTime - (int)objectN.EndTime > stackThreshold)
                    {
                        break;
                    }

                    if (objectN is Slider && objectN.EndPosition.DistanceSquaredTo(objectI.Position) < stackDistanceSquared)
                    {
                        int offset = objectI.DifficultyStackHeight - objectN.DifficultyStackHeight + 1;
                        for (int j = n + 1; j <= i; j++)
                        {
                            HitObject objectJ = objects[j];
                            if (objectN.EndPosition.DistanceSquaredTo(objectJ.Position) < stackDistanceSquared)
                            {
                                objectJ.DifficultyStackHeight -= offset;
                            }
                        }

                        break;
                    }

                    if (objectN.Position.DistanceSquaredTo(objectI.Position) < stackDistanceSquared)
                    {
                        objectN.DifficultyStackHeight = objectI.DifficultyStackHeight + 1;
                        objectI = objectN;
                    }
                }
            }
            else if (objectI is Slider)
            {
                while (--n >= startIndex)
                {
                    HitObject objectN = objects[n];
                    if (objectN is Spinner)
                    {
                        continue;
                    }

                    if (objectI.StartTime - objectN.StartTime > stackThreshold)
                    {
                        break;
                    }

                    if (objectN.EndPosition.DistanceSquaredTo(objectI.Position) < stackDistanceSquared)
                    {
                        objectN.DifficultyStackHeight = objectI.DifficultyStackHeight + 1;
                        objectI = objectN;
                    }
                }
            }
        }

        _ = extendedEndIndex;
    }

    private static void ApplyOldStacking(HitObject[] objects, double stackLeniency)
    {
        const float stackDistanceSquared = 9f;
        for (int i = 0; i < objects.Length; i++)
        {
            HitObject current = objects[i];
            if (current.DifficultyStackHeight != 0 && current is not Slider)
            {
                continue;
            }

            int sliderStack = 0;
            double startTime = current.EndTime;
            double stackThreshold = CalculateStackThreshold(current, stackLeniency);

            for (int j = i + 1; j < objects.Length; j++)
            {
                if (objects[j].StartTime - stackThreshold > startTime)
                {
                    break;
                }

                if (objects[j].Position.DistanceSquaredTo(current.Position) < stackDistanceSquared)
                {
                    current.DifficultyStackHeight++;
                    startTime = objects[j].StartTime;
                }
                else if (objects[j].Position.DistanceSquaredTo(current.EndPosition) < stackDistanceSquared)
                {
                    sliderStack++;
                    objects[j].DifficultyStackHeight -= sliderStack;
                    startTime = objects[j].StartTime;
                }
            }
        }
    }

    private static double CalculateStackThreshold(HitObject hitObject, double stackLeniency) => (int)hitObject.TimePreempt * stackLeniency;
}

internal sealed class DroidPlayableBeatmap(Beatmap beatmap, GameMode mode, IEnumerable<Mod>? mods)
    : PlayableBeatmap(beatmap, mode, mods);

internal sealed class StandardPlayableBeatmap(Beatmap beatmap, GameMode mode, IEnumerable<Mod>? mods)
    : PlayableBeatmap(beatmap, mode, mods);

internal static class ReferenceBeatmapParser
{
    public static Beatmap Parse(string path)
    {
        var beatmap = new Beatmap();
        string section = string.Empty;
        var hitObjects = new List<HitObject>();

        foreach (string rawLine in File.ReadLines(path))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("osu file format v", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(line["osu file format v".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int formatVersion))
            {
                beatmap.FormatVersion = formatVersion;
                continue;
            }

            if (line[0] == '[' && line[^1] == ']')
            {
                section = line[1..^1];
                continue;
            }

            if (section.Equals("General", StringComparison.OrdinalIgnoreCase))
            {
                ParseGeneral(beatmap, line);
            }
            else if (section.Equals("Difficulty", StringComparison.OrdinalIgnoreCase))
            {
                ParseDifficulty(beatmap.Difficulty, line);
            }
            else if (section.Equals("TimingPoints", StringComparison.OrdinalIgnoreCase))
            {
                ParseTimingPoint(beatmap.ControlPoints, line);
            }
            else if (section.Equals("HitObjects", StringComparison.OrdinalIgnoreCase))
            {
                HitObject? obj = ParseHitObject(line);
                if (obj is not null)
                {
                    hitObjects.Add(obj);
                }
            }
        }

        beatmap.HitObjects = new BeatmapHitObjects(hitObjects.OrderBy(static obj => obj.StartTime).ToArray());
        return beatmap;
    }

    private static void ParseGeneral(Beatmap beatmap, string line)
    {
        int separator = line.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 0)
        {
            return;
        }

        string key = line[..separator].Trim();
        if (key.Equals("StackLeniency", StringComparison.OrdinalIgnoreCase))
        {
            beatmap.StackLeniency = ParseDouble(line[(separator + 1)..].Trim());
        }
    }

    private static void ParseDifficulty(ReferenceBeatmapDifficulty difficulty, string line)
    {
        int separator = line.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 0)
        {
            return;
        }

        string key = line[..separator].Trim();
        string value = line[(separator + 1)..].Trim();
        switch (key)
        {
            case "CircleSize":
                difficulty.DifficultyCircleSize = ParseFloat(value);
                difficulty.GameplayCircleSize = difficulty.DifficultyCircleSize;
                break;
            case "ApproachRate":
                difficulty.ApproachRate = ParseFloat(value);
                break;
            case "OverallDifficulty":
                difficulty.OverallDifficulty = ParseFloat(value);
                break;
            case "HPDrainRate":
                difficulty.HealthDrainRate = ParseFloat(value);
                break;
            case "SliderMultiplier":
                difficulty.SliderMultiplier = ParseDouble(value);
                break;
            case "SliderTickRate":
                difficulty.SliderTickRate = ParseDouble(value);
                break;
            default:
                break;
        }
    }

    private static void ParseTimingPoint(BeatmapControlPoints controlPoints, string line)
    {
        string[] fields = line.Split(',');
        if (fields.Length < 2)
        {
            return;
        }

        double time = ParseDouble(fields[0]);
        double beatLength = ParseDouble(fields[1]);
        bool uninherited = fields.Length < 7 || ParseInt(fields[6]) == 1;

        if (uninherited)
        {
            controlPoints.Timing.Add(new TimingControlPoint(time, beatLength, fields.Length > 2 ? ParseInt(fields[2]) : 4));
        }
        else if (beatLength < 0)
        {
            controlPoints.Difficulty.Add(new DifficultyControlPoint(time, System.Math.Clamp(-100.0 / beatLength, 0.1, 10.0), true));
        }
    }

    private static HitObject? ParseHitObject(string line)
    {
        string[] fields = line.Split(',');
        if (fields.Length < 4)
        {
            return null;
        }

        var position = new ReferenceVector2(ParseFloat(fields[0]), ParseFloat(fields[1]));
        double time = ParseDouble(fields[2]);
        int type = ParseInt(fields[3]);

        if ((type & 2) != 0 && fields.Length > 7)
        {
            var path = ParseSliderPath(position, fields[5], ParseDouble(fields[7]));
            return new Slider(time, position, System.Math.Max(1, ParseInt(fields[6])), path);
        }

        return (type & 8) != 0 && fields.Length > 5 ? new Spinner(time, ParseDouble(fields[5]), position) : new HitCircle(time, position);
    }

    private static SliderPath ParseSliderPath(ReferenceVector2 startPosition, string encoded, double expectedDistance)
    {
        string[] parts = encoded.Split('|', StringSplitOptions.RemoveEmptyEntries);
        SliderPathType pathType = parts.Length == 0 ? SliderPathType.Bezier : parts[0] switch
        {
            "L" => SliderPathType.Linear,
            "P" => SliderPathType.PerfectCurve,
            "C" => SliderPathType.Catmull,
            _ => SliderPathType.Bezier,
        };

        var points = new List<ReferenceVector2> { new(0, 0) };
        foreach (string part in parts.Skip(1))
        {
            string[] xy = part.Split(':');
            if (xy.Length == 2)
            {
                points.Add(new ReferenceVector2(ParseFloat(xy[0]) - startPosition.X, ParseFloat(xy[1]) - startPosition.Y));
            }
        }

        return new SliderPath(pathType, points, expectedDistance);
    }

    private static int ParseInt(string? text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : 0;

    private static float ParseFloat(string? text) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : 0f;

    private static double ParseDouble(string? text) => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) ? parsed : 0d;
}
