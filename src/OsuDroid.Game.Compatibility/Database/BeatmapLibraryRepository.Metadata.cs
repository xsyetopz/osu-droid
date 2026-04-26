using Microsoft.Data.Sqlite;

namespace OsuDroid.Game.Compatibility.Database;

public sealed partial class BeatmapLibraryRepository
{
    public void UpdateStarRatings(
        string md5,
        string setDirectory,
        string filename,
        float? droidStarRating,
        float? standardStarRating
    )
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
        command.Parameters.AddWithValue(
            "$droidStarRating",
            droidStarRating is null ? DBNull.Value : droidStarRating.Value
        );
        command.Parameters.AddWithValue(
            "$standardStarRating",
            standardStarRating is null ? DBNull.Value : standardStarRating.Value
        );
        command.Parameters.AddWithValue("$md5", md5);
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.Parameters.AddWithValue("$filename", filename);
        command.ExecuteNonQuery();
    }

    public void UpdateOnlineMetadata(
        string setDirectory,
        long beatmapId,
        string version,
        int? status,
        float? droidStarRating,
        float? standardStarRating
    )
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            UPDATE BeatmapInfo
            SET status = $status,
                droidStarRating = COALESCE($droidStarRating, droidStarRating),
                standardStarRating = COALESCE($standardStarRating, standardStarRating)
            WHERE setDirectory = $setDirectory
              AND (
                    id = $beatmapId
                    OR ($version <> '' AND version = $version)
                  )
            """;
        command.Parameters.AddWithValue("$status", status is null ? DBNull.Value : status.Value);
        command.Parameters.AddWithValue(
            "$droidStarRating",
            droidStarRating is null ? DBNull.Value : droidStarRating.Value
        );
        command.Parameters.AddWithValue(
            "$standardStarRating",
            standardStarRating is null ? DBNull.Value : standardStarRating.Value
        );
        command.Parameters.AddWithValue("$setDirectory", setDirectory);
        command.Parameters.AddWithValue("$beatmapId", beatmapId);
        command.Parameters.AddWithValue("$version", version);
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
}
