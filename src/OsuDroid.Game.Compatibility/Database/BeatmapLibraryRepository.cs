using Microsoft.Data.Sqlite;
using OsuDroid.Game.Beatmaps;

namespace OsuDroid.Game.Compatibility.Database;

public sealed partial class BeatmapLibraryRepository(DroidDatabase database)
    : IBeatmapLibraryRepository
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
        ExecuteSetDirectoryDelete(
            connection,
            transaction,
            "DELETE FROM BeatmapInfo WHERE setDirectory = $setDirectory",
            directory
        );
        ExecuteSetDirectoryDelete(
            connection,
            transaction,
            "DELETE FROM BeatmapOptions WHERE setDirectory = $setDirectory",
            directory
        );
        ExecuteSetDirectoryDelete(
            connection,
            transaction,
            "DELETE FROM BeatmapSetCollection_BeatmapSetInfo WHERE setDirectory = $setDirectory",
            directory
        );
        transaction.Commit();
    }

    public void ClearBeatmapCache()
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();
        ExecuteDelete(connection, transaction, "DELETE FROM BeatmapInfo");
        ExecuteDelete(connection, transaction, "DELETE FROM BeatmapDifficultyMetadata");
        ExecuteDelete(connection, transaction, "DELETE FROM BeatmapSetCollection_BeatmapSetInfo");
        transaction.Commit();
    }

    public void ClearBeatmapOptions()
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM BeatmapOptions";
        command.ExecuteNonQuery();
    }

    public bool IsBeatmapSetImported(string directory)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            "SELECT EXISTS(SELECT setDirectory FROM BeatmapInfo WHERE setDirectory = $setDirectory LIMIT 1)";
        command.Parameters.AddWithValue("$setDirectory", directory);
        return Convert.ToInt32(
                command.ExecuteScalar(),
                System.Globalization.CultureInfo.InvariantCulture
            ) != 0;
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
            .OrderBy(
                set => set.Beatmaps.FirstOrDefault()?.Artist ?? string.Empty,
                StringComparer.OrdinalIgnoreCase
            )
            .ThenBy(
                set => set.Beatmaps.FirstOrDefault()?.Title ?? string.Empty,
                StringComparer.OrdinalIgnoreCase
            )
            .ToArray();

        return new BeatmapLibrarySnapshot(sets);
    }
}
