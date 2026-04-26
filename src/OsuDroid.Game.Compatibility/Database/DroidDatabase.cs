using Microsoft.Data.Sqlite;

namespace OsuDroid.Game.Compatibility.Database;

public sealed class DroidDatabase(string path)
{
    public string Path { get; } = path;

    public void EnsureCreated()
    {
        string? directory = System.IO.Path.GetDirectoryName(Path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using SqliteConnection connection = OpenConnection();

        foreach (string statement in DroidDatabaseSchema.CreateStatements)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = statement;
            command.ExecuteNonQuery();
        }

        EnsureBeatmapInfoColumns(connection);
    }

    public SqliteConnection OpenConnection()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = Path };
        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        return connection;
    }

    private static void EnsureBeatmapInfoColumns(SqliteConnection connection)
    {
        using SqliteCommand readCommand = connection.CreateCommand();
        readCommand.CommandText = "PRAGMA table_info(BeatmapInfo)";
        using SqliteDataReader reader = readCommand.ExecuteReader();
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            existingColumns.Add(reader.GetString(1));
        }

        reader.Close();

        foreach (DatabaseColumn column in DroidDatabaseSchema.BeatmapInfoColumns)
        {
            if (existingColumns.Contains(column.Name))
            {
                continue;
            }

            using SqliteCommand alterCommand = connection.CreateCommand();
            alterCommand.CommandText =
                $"ALTER TABLE BeatmapInfo ADD COLUMN {column.Name} {column.Definition}";
            alterCommand.ExecuteNonQuery();
        }
    }
}
