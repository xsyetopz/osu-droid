namespace OsuDroid.Game.Compatibility.Database;

public sealed record DroidImportPlan(string CorePath, string AppFilesPath)
{
    public string PropertiesPath => Path.Combine(AppFilesPath, "properties");
    public string MigratedPropertiesPath => Path.Combine(AppFilesPath, "properties_old");
    public string FavoritesPath => Path.Combine(CorePath, "json", "favorite.json");
    public string MigratedFavoritesPath => Path.Combine(CorePath, "json", "favorite_old.json");
    public string ScoreDatabasePath => Path.Combine(CorePath, "databases", "osudroid_test.db");
    public string MigratedScoreDatabasePath =>
        Path.Combine(CorePath, "databases", "osudroid_old.db");
}
