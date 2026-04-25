namespace OsuDroid.Game.Runtime.Paths;

public sealed record DroidGamePathLayout
{
    public DroidGamePathLayout(DroidPathRoots roots)
    {
        CoreRoot = NormalizeDirectory(roots.CoreRoot);
        CacheRoot = NormalizeDirectory(roots.CacheRoot);
        Songs = Path.Combine(CoreRoot, "Songs");
        Skin = Path.Combine(CoreRoot, "Skin");
        Scores = Path.Combine(CoreRoot, "Scores");
        Databases = Path.Combine(CoreRoot, "databases");
        Log = Path.Combine(CoreRoot, "Log");
        Downloads = Path.Combine(CacheRoot, "Downloads");
        NoMedia = Path.Combine(CoreRoot, ".nomedia");
    }

    public string CoreRoot { get; }

    public string CacheRoot { get; }

    public string Songs { get; }

    public string Skin { get; }

    public string Scores { get; }

    public string Databases { get; }

    public string Log { get; }

    public string Downloads { get; }

    public string NoMedia { get; }

    public string GetDatabasePath(string buildType) => Path.Combine(Databases, $"room-{buildType}.db");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(CoreRoot);
        Directory.CreateDirectory(Songs);
        Directory.CreateDirectory(Skin);
        Directory.CreateDirectory(Scores);
        Directory.CreateDirectory(Databases);
        Directory.CreateDirectory(Log);
        Directory.CreateDirectory(CacheRoot);
        Directory.CreateDirectory(Downloads);

        if (!File.Exists(NoMedia))
        {
            File.WriteAllText(NoMedia, string.Empty);
        }
    }

    private static string NormalizeDirectory(string path) => string.IsNullOrWhiteSpace(path) ? throw new ArgumentException("Path cannot be empty.", nameof(path)) : Path.GetFullPath(path);
}
