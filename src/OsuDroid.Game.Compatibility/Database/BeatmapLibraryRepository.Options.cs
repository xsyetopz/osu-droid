using Microsoft.Data.Sqlite;
using OsuDroid.Game.Beatmaps;

namespace OsuDroid.Game.Compatibility.Database;

public sealed partial class BeatmapLibraryRepository
{
    public BeatmapOptions GetBeatmapOptions(string setDirectory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            "SELECT setDirectory, isFavorite, offset FROM BeatmapOptions WHERE setDirectory = $setDirectory";
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
}
