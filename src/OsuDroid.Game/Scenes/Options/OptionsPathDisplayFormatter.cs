namespace OsuDroid.Game.Scenes;

internal static class OptionsPathDisplayFormatter
{
    private const int MaxDisplayCharacters = 72;
    private const string Ellipsis = "…";
    private const string OsuDroidLibrarySegment = "/Library/osu-droid";
    private const string MobileContainerPrefix = "/var/mobile/";
    private const string MobileContainerDisplayPrefix = "/var/mobile/…";

    public static string Format(string path)
    {
        if (path.Length <= MaxDisplayCharacters)
            return path;

        var normalized = path.Replace('\\', '/');
        var osuDroidSegmentIndex = normalized.IndexOf(OsuDroidLibrarySegment, StringComparison.Ordinal);
        if (osuDroidSegmentIndex >= 0 && normalized.StartsWith(MobileContainerPrefix, StringComparison.Ordinal))
            return MobileContainerDisplayPrefix + normalized[osuDroidSegmentIndex..];

        return MiddleEllipsize(normalized);
    }

    private static string MiddleEllipsize(string path)
    {
        if (path.Length <= MaxDisplayCharacters)
            return path;

        var tailLength = Math.Min(48, MaxDisplayCharacters - 8);
        var headLength = MaxDisplayCharacters - tailLength - Ellipsis.Length;
        return path[..headLength] + Ellipsis + path[^tailLength..];
    }
}
