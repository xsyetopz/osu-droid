namespace OsuDroid.Game.Compatibility.Database;

public static class DroidDatabaseConstants
{
    public const int CurrentVersion = 4;
    public const string DatabaseDirectory = "databases";

    public static string GetDatabaseFileName(string buildType) => $"room-{buildType}.db";

    public static string GetDatabasePath(string corePath, string buildType) => Path.Combine(
        corePath,
        DatabaseDirectory,
        GetDatabaseFileName(buildType));
}
