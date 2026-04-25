namespace OsuDroid.Game.Runtime.Paths;

public sealed record DroidPathRoots(string CoreRoot, string CacheRoot)
{
    public const string CoreDirectoryName = "osu!droid";
    public const string HyphenatedCoreDirectoryName = "osu-droid";

    public static DroidPathRoots FromCoreRoot(string coreRoot) => new(coreRoot, Path.Combine(coreRoot, "Cache"));

    public static DroidPathRoots FromAppDataDirectory(string appDataDirectory, string cacheRoot)
    {
        string coreRoot = Path.Combine(appDataDirectory, CoreDirectoryName);
        MigrateHyphenatedCoreRoot(Path.Combine(appDataDirectory, HyphenatedCoreDirectoryName), coreRoot);
        return new DroidPathRoots(coreRoot, cacheRoot);
    }

    private static void MigrateHyphenatedCoreRoot(string hyphenatedCoreRoot, string coreRoot)
    {
        if (!Directory.Exists(hyphenatedCoreRoot) || Directory.Exists(coreRoot))
        {
            return;
        }

        string? parent = Path.GetDirectoryName(coreRoot);
        if (!string.IsNullOrWhiteSpace(parent))
        {
            Directory.CreateDirectory(parent);
        }

        Directory.Move(hyphenatedCoreRoot, coreRoot);
    }
}
