using Microsoft.Data.Sqlite;
using OsuDroid.Game.Compatibility.Database;

namespace OsuDroid.Game.Beatmaps;

public interface IBeatmapLibraryRepository
{
    void UpsertBeatmaps(IReadOnlyList<BeatmapInfo> beatmaps);

    void DeleteBeatmapSets(IReadOnlyList<string> directories);

    void DeleteBeatmapSetData(string directory);

    void UpdateStarRatings(string md5, string setDirectory, string filename, float? droidStarRating, float? standardStarRating);

    long GetDifficultyMetadata(string key);

    void SetDifficultyMetadata(string key, long value);

    void ResetDroidStarRatings();

    void ResetStandardStarRatings();

    bool IsBeatmapSetImported(string directory);

    IReadOnlyList<string> GetBeatmapSetDirectories();

    BeatmapLibrarySnapshot LoadLibrary();

    BeatmapOptions GetBeatmapOptions(string setDirectory);

    void UpsertBeatmapOptions(BeatmapOptions options);

    IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null);

    IReadOnlySet<string> GetCollectionSetDirectories(string name);

    bool CollectionExists(string name);

    void CreateCollection(string name);

    void DeleteCollection(string name);

    void AddBeatmapToCollection(string name, string setDirectory);

    void RemoveBeatmapFromCollection(string name, string setDirectory);
}

public sealed class BeatmapLibraryRepository(DroidDatabase database) : IBeatmapLibraryRepository
{
    public void UpsertBeatmaps(IReadOnlyList<BeatmapInfo> beatmaps)
    {
        if (beatmaps.Count == 0)
            return;

        using var connection = database.OpenConnection();
        using var transaction = connection.BeginTransaction();

        foreach (var beatmap in beatmaps)
            UpsertBeatmap(connection, transaction, beatmap);

        transaction.Commit();
    }

    public void DeleteBeatmapSets(IReadOnlyList<string> directories)
    {
        if (directories.Count == 0)
            return;

        using var connection = database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM BeatmapInfo WHERE setDirectory = $setDirectory";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$setDirectory";
        command.Parameters.Add(parameter);

        foreach (var directory in directories)
        {
            parameter.Value = directory;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void DeleteBeatmapSetData(string directory)
    {
        using var connection = database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        ExecuteSetDirectoryDelete(connection, transaction, "DELETE FROM BeatmapInfo WHERE setDirectory = $setDirectory", directory);
        ExecuteSetDirectoryDelete(connection, transaction, "DELETE FROM BeatmapOptions WHERE setDirectory = $setDirectory", directory);
        ExecuteSetDirectoryDelete(connection, transaction, "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE setDirectory = $setDirectory", directory);
        transaction.Commit();
    }

    public bool IsBeatmapSetImported(string directory)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT setDirectory FROM BeatmapInfo WHERE setDirectory = $setDirectory LIMIT 1)";
        command.Parameters.AddWithValue("$setDirectory", directory);
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) != 0;
    }

    public void UpdateStarRatings(string md5, string setDirectory, string filename, float? droidStarRating, float? standardStarRating)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
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
        using var connection = database.OpenConnection();
        EnsureDifficultyMetadataTable(connection);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM BeatmapDifficultyMetadata WHERE key = $key";
        command.Parameters.AddWithValue("$key", key);
        return command.ExecuteScalar() is { } value
            ? Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture)
            : 0L;
    }

    public void SetDifficultyMetadata(string key, long value)
    {
        using var connection = database.OpenConnection();
        EnsureDifficultyMetadataTable(connection);
        using var command = connection.CreateCommand();
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
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE BeatmapInfo SET droidStarRating = NULL";
        command.ExecuteNonQuery();
    }

    public void ResetStandardStarRatings()
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE BeatmapInfo SET standardStarRating = NULL";
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<string> GetBeatmapSetDirectories()
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT setDirectory FROM BeatmapInfo";
        using var reader = command.ExecuteReader();
        var directories = new List<string>();

        while (reader.Read())
            directories.Add(reader.GetString(0));

        return directories;
    }

    public BeatmapLibrarySnapshot LoadLibrary()
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM BeatmapInfo ORDER BY artist, title, version";
        using var reader = command.ExecuteReader();
        var beatmaps = new List<BeatmapInfo>();

        while (reader.Read())
            beatmaps.Add(ReadBeatmap(reader));

        var sets = beatmaps
            .GroupBy(beatmap => beatmap.SetDirectory, StringComparer.Ordinal)
            .Select(group => new BeatmapSetInfo(group.First().SetId, group.Key, group.ToArray()))
            .OrderBy(set => set.Beatmaps.FirstOrDefault()?.Artist ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(set => set.Beatmaps.FirstOrDefault()?.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new BeatmapLibrarySnapshot(sets);
    }

    public BeatmapOptions GetBeatmapOptions(string setDirectory)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT setDirectory, isFavorite, offset FROM BeatmapOptions WHERE setDirectory = $setDirectory";
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        using var reader = command.ExecuteReader();

        return reader.Read()
            ? new BeatmapOptions(reader.GetString(0), reader.GetInt32(1) != 0, reader.GetInt32(2))
            : new BeatmapOptions(setDirectory);
    }

    public void UpsertBeatmapOptions(BeatmapOptions options)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
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
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
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
        using var reader = command.ExecuteReader();
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
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT setDirectory FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name";
        command.Parameters.AddWithValue("$name", name);
        using var reader = command.ExecuteReader();
        var directories = new HashSet<string>(StringComparer.Ordinal);

        while (reader.Read())
            directories.Add(reader.GetString(0));

        return directories;
    }

    public bool CollectionExists(string name)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT name FROM BeatmapSetCollection WHERE name = $name LIMIT 1)";
        command.Parameters.AddWithValue("$name", name);
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) != 0;
    }

    public void CreateCollection(string name)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO BeatmapSetCollection (name) VALUES ($name)";
        command.Parameters.AddWithValue("$name", name);
        command.ExecuteNonQuery();
    }

    public void DeleteCollection(string name)
    {
        using var connection = database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        ExecuteCollectionDelete(connection, transaction, "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name", name);
        ExecuteCollectionDelete(connection, transaction, "DELETE FROM BeatmapSetCollection WHERE name = $name", name);
        transaction.Commit();
    }

    public void AddBeatmapToCollection(string name, string setDirectory)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO BeatmapSetCollection_BeatmapSetInfo (collectionName, setDirectory) VALUES ($name, $setDirectory)";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.ExecuteNonQuery();
    }

    public void RemoveBeatmapFromCollection(string name, string setDirectory)
    {
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE collectionName = $name AND setDirectory = $setDirectory";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.ExecuteNonQuery();
    }

    private static void UpsertBeatmap(SqliteConnection connection, SqliteTransaction transaction, BeatmapInfo beatmap)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT OR REPLACE INTO BeatmapInfo (
                filename, setDirectory, md5, id, audioFilename, backgroundFilename, status, setId,
                title, titleUnicode, artist, artistUnicode, creator, version, tags, source, dateImported,
                approachRate, overallDifficulty, circleSize, hpDrainRate, droidStarRating, standardStarRating,
                bpmMax, bpmMin, mostCommonBPM, length, previewTime, hitCircleCount, sliderCount, spinnerCount,
                maxCombo, epilepsyWarning
            ) VALUES (
                $filename, $setDirectory, $md5, $id, $audioFilename, $backgroundFilename, $status, $setId,
                $title, $titleUnicode, $artist, $artistUnicode, $creator, $version, $tags, $source, $dateImported,
                $approachRate, $overallDifficulty, $circleSize, $hpDrainRate, $droidStarRating, $standardStarRating,
                $bpmMax, $bpmMin, $mostCommonBPM, $length, $previewTime, $hitCircleCount, $sliderCount, $spinnerCount,
                $maxCombo, $epilepsyWarning
            )
            """;

        command.Parameters.AddWithValue("$filename", beatmap.Filename);
        command.Parameters.AddWithValue("$setDirectory", beatmap.SetDirectory);
        command.Parameters.AddWithValue("$md5", beatmap.Md5);
        command.Parameters.AddWithValue("$id", beatmap.Id is null ? DBNull.Value : beatmap.Id.Value);
        command.Parameters.AddWithValue("$audioFilename", beatmap.AudioFilename);
        command.Parameters.AddWithValue("$backgroundFilename", beatmap.BackgroundFilename is null ? DBNull.Value : beatmap.BackgroundFilename);
        command.Parameters.AddWithValue("$status", beatmap.Status is null ? DBNull.Value : beatmap.Status.Value);
        command.Parameters.AddWithValue("$setId", beatmap.SetId is null ? DBNull.Value : beatmap.SetId.Value);
        command.Parameters.AddWithValue("$title", beatmap.Title);
        command.Parameters.AddWithValue("$titleUnicode", beatmap.TitleUnicode);
        command.Parameters.AddWithValue("$artist", beatmap.Artist);
        command.Parameters.AddWithValue("$artistUnicode", beatmap.ArtistUnicode);
        command.Parameters.AddWithValue("$creator", beatmap.Creator);
        command.Parameters.AddWithValue("$version", beatmap.Version);
        command.Parameters.AddWithValue("$tags", beatmap.Tags);
        command.Parameters.AddWithValue("$source", beatmap.Source);
        command.Parameters.AddWithValue("$dateImported", beatmap.DateImported);
        command.Parameters.AddWithValue("$approachRate", beatmap.ApproachRate);
        command.Parameters.AddWithValue("$overallDifficulty", beatmap.OverallDifficulty);
        command.Parameters.AddWithValue("$circleSize", beatmap.CircleSize);
        command.Parameters.AddWithValue("$hpDrainRate", beatmap.HpDrainRate);
        command.Parameters.AddWithValue("$droidStarRating", beatmap.DroidStarRating is null ? DBNull.Value : beatmap.DroidStarRating.Value);
        command.Parameters.AddWithValue("$standardStarRating", beatmap.StandardStarRating is null ? DBNull.Value : beatmap.StandardStarRating.Value);
        command.Parameters.AddWithValue("$bpmMax", beatmap.BpmMax);
        command.Parameters.AddWithValue("$bpmMin", beatmap.BpmMin);
        command.Parameters.AddWithValue("$mostCommonBPM", beatmap.MostCommonBpm);
        command.Parameters.AddWithValue("$length", beatmap.Length);
        command.Parameters.AddWithValue("$previewTime", beatmap.PreviewTime);
        command.Parameters.AddWithValue("$hitCircleCount", beatmap.HitCircleCount);
        command.Parameters.AddWithValue("$sliderCount", beatmap.SliderCount);
        command.Parameters.AddWithValue("$spinnerCount", beatmap.SpinnerCount);
        command.Parameters.AddWithValue("$maxCombo", beatmap.MaxCombo);
        command.Parameters.AddWithValue("$epilepsyWarning", beatmap.EpilepsyWarning ? 1 : 0);
        command.ExecuteNonQuery();
    }

    private static void ExecuteSetDirectoryDelete(SqliteConnection connection, SqliteTransaction transaction, string text, string setDirectory)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = text;
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.ExecuteNonQuery();
    }

    private static void ExecuteCollectionDelete(SqliteConnection connection, SqliteTransaction transaction, string text, string name)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = text;
        command.Parameters.AddWithValue("$name", name);
        command.ExecuteNonQuery();
    }

    private static void EnsureDifficultyMetadataTable(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS BeatmapDifficultyMetadata (
                key TEXT NOT NULL PRIMARY KEY,
                value INTEGER NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }

    private static BeatmapInfo ReadBeatmap(SqliteDataReader reader) => new(
        Filename: reader.GetString(reader.GetOrdinal("filename")),
        SetDirectory: reader.GetString(reader.GetOrdinal("setDirectory")),
        Md5: reader.GetString(reader.GetOrdinal("md5")),
        Id: ReadNullableLong(reader, "id"),
        AudioFilename: reader.GetString(reader.GetOrdinal("audioFilename")),
        BackgroundFilename: ReadNullableString(reader, "backgroundFilename"),
        Status: ReadNullableInt(reader, "status"),
        SetId: ReadNullableInt(reader, "setId"),
        Title: reader.GetString(reader.GetOrdinal("title")),
        TitleUnicode: reader.GetString(reader.GetOrdinal("titleUnicode")),
        Artist: reader.GetString(reader.GetOrdinal("artist")),
        ArtistUnicode: reader.GetString(reader.GetOrdinal("artistUnicode")),
        Creator: reader.GetString(reader.GetOrdinal("creator")),
        Version: reader.GetString(reader.GetOrdinal("version")),
        Tags: reader.GetString(reader.GetOrdinal("tags")),
        Source: reader.GetString(reader.GetOrdinal("source")),
        DateImported: reader.GetInt64(reader.GetOrdinal("dateImported")),
        ApproachRate: reader.GetFloat(reader.GetOrdinal("approachRate")),
        OverallDifficulty: reader.GetFloat(reader.GetOrdinal("overallDifficulty")),
        CircleSize: reader.GetFloat(reader.GetOrdinal("circleSize")),
        HpDrainRate: reader.GetFloat(reader.GetOrdinal("hpDrainRate")),
        DroidStarRating: ReadNullableFloat(reader, "droidStarRating"),
        StandardStarRating: ReadNullableFloat(reader, "standardStarRating"),
        BpmMax: reader.GetFloat(reader.GetOrdinal("bpmMax")),
        BpmMin: reader.GetFloat(reader.GetOrdinal("bpmMin")),
        MostCommonBpm: reader.GetFloat(reader.GetOrdinal("mostCommonBPM")),
        Length: reader.GetInt64(reader.GetOrdinal("length")),
        PreviewTime: reader.GetInt32(reader.GetOrdinal("previewTime")),
        HitCircleCount: reader.GetInt32(reader.GetOrdinal("hitCircleCount")),
        SliderCount: reader.GetInt32(reader.GetOrdinal("sliderCount")),
        SpinnerCount: reader.GetInt32(reader.GetOrdinal("spinnerCount")),
        MaxCombo: reader.GetInt32(reader.GetOrdinal("maxCombo")),
        EpilepsyWarning: reader.GetInt32(reader.GetOrdinal("epilepsyWarning")) != 0);

    private static string? ReadNullableString(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? ReadNullableInt(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static long? ReadNullableLong(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    private static float? ReadNullableFloat(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetFloat(ordinal);
    }
}
