using Microsoft.Data.Sqlite;
using OsuDroid.Game.Compatibility.Database;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class DatabaseCompatibilityTests
{
    [Test]
    public void DatabasePathMatchesLegacyRoomName()
    {
        Assert.That(DroidDatabaseConstants.GetDatabasePath("/core", "debug"), Is.EqualTo(Path.Combine("/core", "databases", "room-debug.db")));
    }

    [Test]
    public void SchemaCreatesRequiredTablesAtCurrentVersion()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"schema-{Guid.NewGuid():N}.db");
        try
        {
            var database = new DroidDatabase(path);
            database.EnsureCreated();

            using var connection = database.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
            using var reader = command.ExecuteReader();
            var tables = new HashSet<string>(StringComparer.Ordinal);

            while (reader.Read())
                tables.Add(reader.GetString(0));

            foreach (var table in DroidDatabaseSchema.RequiredTables)
                Assert.That(tables, Does.Contain(table));

            using var version = connection.CreateCommand();
            version.CommandText = "PRAGMA user_version";
            Assert.That(Convert.ToInt32(version.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(DroidDatabaseConstants.CurrentVersion));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void LegacyImportPlanKeepsOldFileNames()
    {
        var plan = new LegacyImportPlan("/core", "/files");

        Assert.That(plan.PropertiesPath, Is.EqualTo(Path.Combine("/files", "properties")));
        Assert.That(plan.FavoritesPath, Is.EqualTo(Path.Combine("/core", "json", "favorite.json")));
        Assert.That(plan.LegacyScoreDatabasePath, Is.EqualTo(Path.Combine("/core", "databases", "osudroid_test.db")));
    }
}
