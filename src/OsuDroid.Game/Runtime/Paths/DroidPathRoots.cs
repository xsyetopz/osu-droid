namespace OsuDroid.Game.Runtime.Paths;

public sealed record DroidPathRoots(string CoreRoot, string CacheRoot)
{
    public static DroidPathRoots FromCoreRoot(string coreRoot) => new(coreRoot, Path.Combine(coreRoot, "Cache"));
}
