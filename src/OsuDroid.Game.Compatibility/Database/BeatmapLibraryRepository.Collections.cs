using Microsoft.Data.Sqlite;
using OsuDroid.Game.Beatmaps;

namespace OsuDroid.Game.Compatibility.Database;

public sealed partial class BeatmapLibraryRepository
{
    public IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.name,
                   COUNT(j.setDirectory) AS beatmapCount,
                   SUM(CASE WHEN j.setDirectory = $selectedSetDirectory THEN 1 ELSE 0 END) AS containsSelected
            FROM BeatmapSetCollection c
            LEFT JOIN BeatmapSetCollection_BeatmapSetInfo j ON j.collectionName = c.name
            GROUP BY c.name
            ORDER BY LOWER(c.name), c.name
            """;
        command.Parameters.AddWithValue(
            "$selectedSetDirectory",
            selectedSetDirectory ?? string.Empty
        );
        using SqliteDataReader reader = command.ExecuteReader();
        var collections = new List<BeatmapCollection>();

        while (reader.Read())
        {
            collections.Add(
                new BeatmapCollection(
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetInt32(2) > 0
                )
            );
        }

        return collections;
    }

    public IReadOnlySet<string> GetCollectionSetDirectories(string name)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            "SELECT setDirectory FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name";
        command.Parameters.AddWithValue("$name", name);
        using SqliteDataReader reader = command.ExecuteReader();
        var directories = new HashSet<string>(StringComparer.Ordinal);

        while (reader.Read())
        {
            directories.Add(reader.GetString(0));
        }

        return directories;
    }

    public bool CollectionExists(string name)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            "SELECT EXISTS(SELECT name FROM BeatmapSetCollection WHERE name = $name LIMIT 1)";
        command.Parameters.AddWithValue("$name", name);
        return Convert.ToInt32(
                command.ExecuteScalar(),
                System.Globalization.CultureInfo.InvariantCulture
            ) != 0;
    }

    public void CreateCollection(string name)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO BeatmapSetCollection (name) VALUES ($name)";
        command.Parameters.AddWithValue("$name", name);
        command.ExecuteNonQuery();
    }

    public void DeleteCollection(string name)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();
        ExecuteCollectionDelete(
            connection,
            transaction,
            "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name",
            name
        );
        ExecuteCollectionDelete(
            connection,
            transaction,
            "DELETE FROM BeatmapSetCollection WHERE name = $name",
            name
        );
        transaction.Commit();
    }

    public void AddBeatmapToCollection(string name, string setDirectory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            "INSERT OR IGNORE INTO BeatmapSetCollection_BeatmapSetInfo (collectionName, setDirectory) VALUES ($name, $setDirectory)";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.ExecuteNonQuery();
    }

    public void RemoveBeatmapFromCollection(string name, string setDirectory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name AND setDirectory = $setDirectory";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.ExecuteNonQuery();
    }
}
