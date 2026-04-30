using System.Globalization;
using OsuDroid.Game.Beatmaps;

namespace OsuDroid.Game.Scenes.ModSelect;

public static class ModStatCalculator
{
    private const double ApproachPreemptMax = 1800d;
    private const double ApproachPreemptMid = 1200d;
    private const double ApproachPreemptMin = 450d;

    public static ModStatSnapshot FromBeatmap(BeatmapInfo? beatmap, ModSelectionState state)
    {
        float ar = beatmap?.ApproachRate ?? 0f;
        float od = beatmap?.OverallDifficulty ?? 0f;
        float cs = beatmap?.CircleSize ?? 0f;
        float hp = beatmap?.HpDrainRate ?? 0f;
        float? droidStars = beatmap?.DroidStarRating;
        float? standardStars = beatmap?.StandardStarRating;
        float originalAr = ar;
        float originalOd = od;
        float originalCs = cs;
        float originalHp = hp;
        float bpmMax = beatmap?.BpmMax ?? 0f;
        float bpmMin = beatmap?.BpmMin ?? 0f;
        float bpm = beatmap?.MostCommonBpm ?? 0f;
        float originalBpm = bpm;
        long length = beatmap?.Length ?? 0L;
        float rate = CombinedRate(state);
        float scoreMultiplier = 1f;
        bool hasDifficultyAdjust = false;

        foreach (string acronym in state.Acronyms)
        {
            ModCatalogEntry? entry = Entry(acronym);
            if (entry is null)
            {
                continue;
            }

            if (!IsRateMod(acronym))
            {
                scoreMultiplier *= entry.ScoreMultiplier;
            }

            switch (acronym.ToUpperInvariant())
            {
                case "EZ":
                case "RE":
                    ar *= 0.5f;
                    od *= 0.5f;
                    cs *= 0.5f;
                    hp *= 0.5f;
                    break;
                case "HR":
                    ar = Math.Min(10f, ar * 1.4f);
                    od = Math.Min(10f, od * 1.4f);
                    cs = Math.Min(10f, cs * 1.3f);
                    hp = Math.Min(10f, hp * 1.4f);
                    break;
                case "DA":
                    hasDifficultyAdjust = true;
                    ar = (float)(NullableNumber(acronym, "ar", state) ?? ar);
                    od = (float)(NullableNumber(acronym, "od", state) ?? od);
                    cs = (float)(NullableNumber(acronym, "cs", state) ?? cs);
                    hp = (float)(NullableNumber(acronym, "hp", state) ?? hp);
                    break;
                default:
                    break;
            }
        }

        scoreMultiplier *= RateScoreMultiplier(rate);
        if (Math.Abs(rate - 1f) > 0.001f)
        {
            bpmMax *= rate;
            bpmMin *= rate;
            bpm *= rate;
            length = rate <= 0f ? length : (long)Math.Round(length / rate);
            ApplyRateToDifficulty(ref ar, ref od, rate, state);
            droidStars *= MathF.Pow(rate, 0.35f);
            standardStars *= MathF.Pow(rate, 0.35f);
        }

        if (state.Acronyms.Count > 0)
        {
            float difficultyScale = 1f + ((ar + od + cs + hp) / 40f - 0.5f) * 0.08f;
            droidStars *= difficultyScale;
            standardStars *= difficultyScale;
        }

        return new ModStatSnapshot(
            ar,
            od,
            cs,
            hp,
            droidStars,
            standardStars,
            bpmMax,
            bpmMin,
            bpm,
            length,
            scoreMultiplier,
            state.Acronyms.Count == 0
                || state
                    .Acronyms.Select(Entry)
                    .Where(entry => entry is not null)
                    .All(entry => entry!.IsRanked),
            state.Acronyms.Count > 0,
            Direction(originalAr, ar, hasDifficultyAdjust),
            Direction(originalOd, od, hasDifficultyAdjust),
            Direction(originalCs, cs, hasDifficultyAdjust),
            Direction(originalHp, hp, hasDifficultyAdjust),
            Direction(originalBpm, bpm),
            DifficultyDirection(
                originalAr,
                ar,
                originalOd,
                od,
                originalCs,
                cs,
                originalHp,
                hp,
                hasDifficultyAdjust
            )
        );
    }

    public static ModStatDirection Direction(
        float initialValue,
        float finalValue,
        bool forceDifficultyAdjust = false
    )
    {
        return forceDifficultyAdjust && Math.Abs(initialValue - finalValue) > 0.001f
                ? ModStatDirection.DifficultyAdjust
            : initialValue < finalValue - 0.001f ? ModStatDirection.Increased
            : initialValue > finalValue + 0.001f ? ModStatDirection.Decreased
            : ModStatDirection.Unchanged;
    }

    public static string FormatSettingValue(ModSettingDescriptor setting, string? raw)
    {
        if (setting.Kind is ModSettingKind.Toggle)
        {
            bool boolValue = raw is null
                ? setting.DefaultBoolean
                : string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
            return boolValue ? "On" : "Off";
        }

        if (setting.Kind is ModSettingKind.Choice)
        {
            return string.IsNullOrWhiteSpace(raw)
                ? setting.EnumValues?.FirstOrDefault() ?? ""
                : raw;
        }

        if (setting.IsNullable && string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
        {
            return "None";
        }

        double value = string.IsNullOrWhiteSpace(raw)
            ? setting.DefaultValue
            : ParseDouble(raw, setting.DefaultValue);
        if (
            setting.Key == "rateMultiplier"
            || setting.Key.EndsWith("Rate", StringComparison.Ordinal)
        )
        {
            return $"{value.ToString("0.##", CultureInfo.InvariantCulture)}x";
        }

        if (setting.Key == "areaFollowDelay")
        {
            return $"{Math.Round(value * 1000).ToString("0", CultureInfo.InvariantCulture)}ms";
        }

        if (setting.Key == "sizeMultiplier")
        {
            return $"{value.ToString("0.#", CultureInfo.InvariantCulture)}x";
        }

        if (setting.Kind is ModSettingKind.WholeNumber or ModSettingKind.OptionalWholeNumber)
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }

        string format = setting.Precision <= 0 ? "0" : "0." + new string('#', setting.Precision);
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string DefaultRawValue(ModSettingDescriptor setting) =>
        setting.Kind switch
        {
            ModSettingKind.Toggle => setting.DefaultBoolean ? "true" : "false",
            ModSettingKind.Choice => setting.EnumValues?.FirstOrDefault() ?? string.Empty,
            ModSettingKind.OptionalSlider or ModSettingKind.OptionalWholeNumber => "null",
            ModSettingKind.Slider => setting.DefaultValue.ToString(CultureInfo.InvariantCulture),
            ModSettingKind.WholeNumber => setting.DefaultValue.ToString(
                CultureInfo.InvariantCulture
            ),
            _ => setting.DefaultValue.ToString(CultureInfo.InvariantCulture),
        };

    private static ModStatDirection DifficultyDirection(
        float originalAr,
        float ar,
        float originalOd,
        float od,
        float originalCs,
        float cs,
        float originalHp,
        float hp,
        bool hasDifficultyAdjust
    )
    {
        if (hasDifficultyAdjust)
        {
            return ModStatDirection.DifficultyAdjust;
        }

        if (
            originalAr < ar - 0.001f
            || originalOd < od - 0.001f
            || originalCs < cs - 0.001f
            || originalHp < hp - 0.001f
        )
        {
            return ModStatDirection.Increased;
        }

        bool decreased =
            originalAr > ar + 0.001f
            || originalOd > od + 0.001f
            || originalCs > cs + 0.001f
            || originalHp > hp + 0.001f;
        return decreased ? ModStatDirection.Decreased : ModStatDirection.Unchanged;
    }

    private static void ApplyRateToDifficulty(
        ref float approachRate,
        ref float overallDifficulty,
        float rate,
        ModSelectionState state
    )
    {
        double preempt = DifficultyRange(
            approachRate,
            ApproachPreemptMax,
            ApproachPreemptMid,
            ApproachPreemptMin
        );
        approachRate = (float)InverseDifficultyRange(
            preempt / rate,
            ApproachPreemptMax,
            ApproachPreemptMid,
            ApproachPreemptMin
        );

        bool hasPrecise = state.Acronyms.Any(acronym =>
            acronym.Equals("PR", StringComparison.OrdinalIgnoreCase)
        );
        double greatWindow = hasPrecise
            ? PreciseDroidGreatWindow(overallDifficulty)
            : DroidGreatWindow(overallDifficulty);
        overallDifficulty = (float)(
            hasPrecise
                ? PreciseDroidGreatWindowToOverallDifficulty(greatWindow / rate)
                : DroidGreatWindowToOverallDifficulty(greatWindow / rate)
        );
    }

    private static double DifficultyRange(
        double difficulty,
        double minimum,
        double midpoint,
        double maximum
    ) =>
        difficulty > 5d ? midpoint + (maximum - midpoint) * (difficulty - 5d) / 5d
        : difficulty < 5d ? midpoint + (midpoint - minimum) * (difficulty - 5d) / 5d
        : midpoint;

    private static double InverseDifficultyRange(
        double difficultyValue,
        double difficultyAtZero,
        double difficultyAtFive,
        double difficultyAtTen
    ) =>
        Math.Sign(difficultyValue - difficultyAtFive)
        == Math.Sign(difficultyAtTen - difficultyAtZero)
            ? (difficultyValue - difficultyAtFive) / (difficultyAtTen - difficultyAtFive) * 5d + 5d
            : (difficultyValue - difficultyAtFive) / (difficultyAtFive - difficultyAtZero) * 5d
                + 5d;

    private static double DroidGreatWindow(double overallDifficulty) =>
        75d + 5d * (5d - overallDifficulty);

    private static double DroidGreatWindowToOverallDifficulty(double greatWindow) =>
        5d - (greatWindow - 75d) / 5d;

    private static double PreciseDroidGreatWindow(double overallDifficulty) =>
        55d + 6d * (5d - overallDifficulty);

    private static double PreciseDroidGreatWindowToOverallDifficulty(double greatWindow) =>
        5d - (greatWindow - 55d) / 6d;

    private static float CombinedRate(ModSelectionState state)
    {
        float rate = 1f;
        foreach (string acronym in state.Acronyms.Where(IsRateMod))
        {
            rate *= (float)Number(acronym, "rateMultiplier", state, DefaultRate(acronym));
        }

        foreach (string acronym in state.Acronyms)
        {
            if (string.Equals(acronym, "WD", StringComparison.OrdinalIgnoreCase))
            {
                rate *= (float)Number(acronym, "finalRate", state, 0.75);
            }
            else if (string.Equals(acronym, "WU", StringComparison.OrdinalIgnoreCase))
            {
                rate *= (float)Number(acronym, "finalRate", state, 1.5);
            }
        }

        return Math.Clamp(rate, 0.1f, 4f);
    }

    private static bool IsRateMod(string acronym) =>
        acronym.Equals("CS", StringComparison.OrdinalIgnoreCase)
        || acronym.Equals("DT", StringComparison.OrdinalIgnoreCase)
        || acronym.Equals("HT", StringComparison.OrdinalIgnoreCase)
        || acronym.Equals("NC", StringComparison.OrdinalIgnoreCase);

    private static double DefaultRate(string acronym) =>
        acronym.ToUpperInvariant() switch
        {
            "DT" or "NC" => 1.5,
            "HT" => 0.75,
            _ => 1,
        };

    private static float RateScoreMultiplier(float rate) =>
        rate > 1f ? 1f + (rate - 1f) * 0.24f : MathF.Pow(0.3f, (1f - rate) * 4f);

    private static double Number(
        string acronym,
        string key,
        ModSelectionState state,
        double fallback
    ) =>
        state.Settings.TryGetValue(acronym, out IReadOnlyDictionary<string, string>? settings)
        && settings.TryGetValue(key, out string? value)
            ? ParseDouble(value, fallback)
            : fallback;

    private static double? NullableNumber(string acronym, string key, ModSelectionState state) =>
        state.Settings.TryGetValue(acronym, out IReadOnlyDictionary<string, string>? settings)
        && settings.TryGetValue(key, out string? value)
        && !string.Equals(value, "null", StringComparison.OrdinalIgnoreCase)
            ? ParseDouble(value, 0)
            : null;

    private static double ParseDouble(string? value, double fallback) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
            ? parsed
            : fallback;

    private static ModCatalogEntry? Entry(string acronym) =>
        ModCatalog.Entries.FirstOrDefault(entry =>
            entry.Acronym.Equals(acronym, StringComparison.OrdinalIgnoreCase)
        );
}
