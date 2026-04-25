using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Compatibility.Database;

public static class DroidDatabaseConstants
{
    public const int CurrentVersion = 5;
    public const string DatabaseDirectory = "databases";

    public static string GetDatabaseFileName(string buildType) => $"room-{buildType}.db";

    public static string GetDatabasePath(string corePath, string buildType) =>
        new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(corePath)).GetDatabasePath(buildType);
}
