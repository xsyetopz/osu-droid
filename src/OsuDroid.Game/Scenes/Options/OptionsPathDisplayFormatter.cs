namespace OsuDroid.Game.Scenes.Options;

internal static class OptionsPathDisplayFormatter
{
    private const int MaxDisplayCharacters = 72;
    private const string Ellipsis = "…";
    private const string OsuDroidLibrarySegment = "/Library/osu!droid";
    private const string MobileContainerPrefix = "/var/mobile/";
    private const string MobileContainerDisplayPrefix = "/var/mobile/…";

    public static string Format(string path)
    {
        if (path.Length <= MaxDisplayCharacters)
        {
            return path;
        }

        string normalized = path.Replace('\\', '/');
        int osuDroidSegmentIndex = normalized.IndexOf(OsuDroidLibrarySegment, StringComparison.Ordinal);
        return osuDroidSegmentIndex >= 0 && normalized.StartsWith(MobileContainerPrefix, StringComparison.Ordinal)
            ? MobileContainerDisplayPrefix + normalized[osuDroidSegmentIndex..]
            : MiddleEllipsize(normalized);
    }

    private static string MiddleEllipsize(string path)
    {
        if (path.Length <= MaxDisplayCharacters)
        {
            return path;
        }

        int tailLength = Math.Min(48, MaxDisplayCharacters - 8);
        int headLength = MaxDisplayCharacters - tailLength - Ellipsis.Length;
        return path[..headLength] + Ellipsis + path[^tailLength..];
    }
}
