using Microsoft.Data.Sqlite;
using OsuDroid.Game.Compatibility.Database;

namespace OsuDroid.Game.Beatmaps;


public sealed partial class BeatmapLibraryRepository(DroidDatabase database) : IBeatmapLibraryRepository
{
    public void UpsertBeatmaps(IReadOnlyList<BeatmapInfo> beatmaps)
    {
        if (beatmaps.Count == 0)
        {
            return;
        }

        using SqliteConnection connection = database.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        foreach (BeatmapInfo beatmap in beatmaps)
        {
            UpsertBeatmap(connection, transaction, beatmap);
        }

        transaction.Commit();
    }

    public void DeleteBeatmapSets(IReadOnlyList<string> directories)
    {
        if (directories.Count == 0)
        {
            return;
        }

        using SqliteConnection connection = database.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM BeatmapInfo WHERE setDirectory = $setDirectory";
        SqliteParameter parameter = command.CreateParameter();
        parameter.ParameterName = "$setDirectory";
        command.Parameters.Add(parameter);

        foreach (string directory in directories)
        {
            parameter.Value = directory;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void DeleteBeatmapSetData(string directory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();
        ExecuteSetDirectoryDelete(connection, transaction, "DELETE FROM BeatmapInfo WHERE setDirectory = $setDirectory", directory);
        ExecuteSetDirectoryDelete(connection, transaction, "DELETE FROM BeatmapOptions WHERE setDirectory = $setDirectory", directory);
        ExecuteSetDirectoryDelete(connection, transaction, "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE setDirectory = $setDirectory", directory);
        transaction.Commit();
    }

    public bool IsBeatmapSetImported(string directory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT setDirectory FROM BeatmapInfo WHERE setDirectory = $setDirectory LIMIT 1)";
        command.Parameters.AddWithValue("$setDirectory", directory);
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) != 0;
    }

    public void UpdateStarRatings(string md5, string setDirectory, string filename, float? droidStarRating, float? standardStarRating)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            UPDATE BeatmapInfo
            SET droidStarRating = $droidStarRating,
                standardStarRating = $standardStarRating
            WHERE md5 = $md5
               OR (setDirectory = $setDirectory AND filename = $filename)
            """;
        command.Parameters.AddWithValue("$droidStarRating", droidStarRating is null ? DBNull.Value : droidStarRating.Value);
        command.Parameters.AddWithValue("$standardStarRating", standardStarRating is null ? DBNull.Value : standardStarRating.Value);
        command.Parameters.AddWithValue("$md5", md5);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.Parameters.AddWithValue("$filename", filename);
        command.ExecuteNonQuery();
    }

    public long GetDifficultyMetadata(string key)
    {
        using SqliteConnection connection = database.OpenConnection();
        EnsureDifficultyMetadataTable(connection);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM BeatmapDifficultyMetadata WHERE key = $key";
        command.Parameters.AddWithValue("$key", key);
        return command.ExecuteScalar() is { } value
            ? Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture)
            : 0L;
    }

    public void SetDifficultyMetadata(string key, long value)
    {
        using SqliteConnection connection = database.OpenConnection();
        EnsureDifficultyMetadataTable(connection);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO BeatmapDifficultyMetadata (key, value)
            VALUES ($key, $value)
            ON CONFLICT(key) DO UPDATE SET value = excluded.value
            """;
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }

    public void ResetDroidStarRatings()
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE BeatmapInfo SET droidStarRating = NULL";
        command.ExecuteNonQuery();
    }

    public void ResetStandardStarRatings()
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE BeatmapInfo SET standardStarRating = NULL";
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<string> GetBeatmapSetDirectories()
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT setDirectory FROM BeatmapInfo";
        using SqliteDataReader reader = command.ExecuteReader();
        var directories = new List<string>();

        while (reader.Read())
        {
            directories.Add(reader.GetString(0));
        }

        return directories;
    }

    public BeatmapLibrarySnapshot LoadLibrary()
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM BeatmapInfo ORDER BY artist, title, version";
        using SqliteDataReader reader = command.ExecuteReader();
        var beatmaps = new List<BeatmapInfo>();

        while (reader.Read())
        {
            beatmaps.Add(ReadBeatmap(reader));
        }

        BeatmapSetInfo[] sets = beatmaps
            .GroupBy(beatmap => beatmap.SetDirectory, StringComparer.Ordinal)
            .Select(group => new BeatmapSetInfo(group.First().SetId, group.Key, group.ToArray()))
            .OrderBy(set => set.Beatmaps.FirstOrDefault()?.Artist ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(set => set.Beatmaps.FirstOrDefault()?.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new BeatmapLibrarySnapshot(sets);
    }

    public BeatmapOptions GetBeatmapOptions(string setDirectory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT setDirectory, isFavorite, offset FROM BeatmapOptions WHERE setDirectory = $setDirectory";
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        using SqliteDataReader reader = command.ExecuteReader();

        return reader.Read()
            ? new BeatmapOptions(reader.GetString(0), reader.GetInt32(1) != 0, reader.GetInt32(2))
            : new BeatmapOptions(setDirectory);
    }

    public void UpsertBeatmapOptions(BeatmapOptions options)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO BeatmapOptions (setDirectory, isFavorite, offset)
            VALUES ($setDirectory, $isFavorite, $offset)
            ON CONFLICT(setDirectory) DO UPDATE SET
                isFavorite = excluded.isFavorite,
                offset = excluded.offset
            """;
        command.Parameters.AddWithValue("$setDirectory", options.SetDirectory);
        command.Parameters.AddWithValue("$isFavorite", options.IsFavorite ? 1 : 0);
        command.Parameters.AddWithValue("$offset", Math.Clamp(options.Offset, -250, 250));
        command.ExecuteNonQuery();
    }

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
        command.Parameters.AddWithValue("$selectedSetDirectory", selectedSetDirectory ?? string.Empty);
        using SqliteDataReader reader = command.ExecuteReader();
        var collections = new List<BeatmapCollection>();

        while (reader.Read())
        {
            collections.Add(new BeatmapCollection(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2) > 0));
        }

        return collections;
    }

    public IReadOnlySet<string> GetCollectionSetDirectories(string name)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT setDirectory FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name";
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
        command.CommandText = "SELECT EXISTS(SELECT name FROM BeatmapSetCollection WHERE name = $name LIMIT 1)";
        command.Parameters.AddWithValue("$name", name);
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) != 0;
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
        ExecuteCollectionDelete(connection, transaction, "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name", name);
        ExecuteCollectionDelete(connection, transaction, "DELETE FROM BeatmapSetCollection WHERE name = $name", name);
        transaction.Commit();
    }

    public void AddBeatmapToCollection(string name, string setDirectory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO BeatmapSetCollection_BeatmapSetInfo (collectionName, setDirectory) VALUES ($name, $setDirectory)";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.ExecuteNonQuery();
    }

    public void RemoveBeatmapFromCollection(string name, string setDirectory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name AND setDirectory = $setDirectory";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.ExecuteNonQuery();
    }
}
