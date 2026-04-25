using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Scenes.Options;

public sealed record OptionsPathDefaults(string CorePath, string SkinTopPath, string SongsDirectory, bool UsesNativeDefaultSummaries = false)
{
    public static OptionsPathDefaults Empty { get; } = new(string.Empty, string.Empty, string.Empty);

    public static OptionsPathDefaults FromPaths(DroidGamePathLayout paths) =>
        new(paths.CoreRoot, paths.Skin, paths.Songs, UsesNativeSummaryDefaults(paths.CoreRoot));

    public string GetDefaultValue(string key) => key switch
    {
        "corePath" => CorePath,
        "skinTopPath" => SkinTopPath,
        "directory" => SongsDirectory,
        _ => string.Empty,
    };

    public string NormalizePathValue(string value)
    {
        string hyphenatedCorePath = GetHyphenatedCorePath();
        if (string.IsNullOrEmpty(hyphenatedCorePath))
        {
            return value;
        }

        string normalizedValue = value.Replace('\\', '/');
        string normalizedHyphenatedCorePath = hyphenatedCorePath.Replace('\\', '/');
        if (string.Equals(normalizedValue, normalizedHyphenatedCorePath, StringComparison.Ordinal))
        {
            return CorePath;
        }

        string hyphenatedChildPrefix = $"{normalizedHyphenatedCorePath}/";
        return normalizedValue.StartsWith(hyphenatedChildPrefix, StringComparison.Ordinal)
            ? CorePath + normalizedValue[normalizedHyphenatedCorePath.Length..]
            : value;
    }

    private static bool UsesNativeSummaryDefaults(string corePath)
    {
        string normalized = corePath.Replace('\\', '/');
        return normalized.Contains("/Library/osu!droid", StringComparison.Ordinal);
    }

    private string GetHyphenatedCorePath()
    {
        string? parent = Path.GetDirectoryName(CorePath);
        return string.IsNullOrWhiteSpace(parent)
            ? string.Empty
            : Path.Combine(parent, DroidPathRoots.HyphenatedCoreDirectoryName);
    }
}
