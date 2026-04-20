using Microsoft.Data.Sqlite;

namespace OsuDroid.Game.Compatibility.Database;

public sealed class DroidDatabase
{
    public DroidDatabase(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public void EnsureCreated()
    {
        var directory = System.IO.Path.GetDirectoryName(Path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var connection = OpenConnection();

        foreach (var statement in DroidDatabaseSchema.CreateStatements)
        {
            using var command = connection.CreateCommand();
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
        using var readCommand = connection.CreateCommand();
        readCommand.CommandText = "PRAGMA table_info(BeatmapInfo)";
        using var reader = readCommand.ExecuteReader();
        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
            existingColumns.Add(reader.GetString(1));

        reader.Close();

        foreach (var column in DroidDatabaseSchema.BeatmapInfoColumns)
        {
            if (existingColumns.Contains(column.Name))
                continue;

            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE BeatmapInfo ADD COLUMN {column.Name} {column.Definition}";
            alterCommand.ExecuteNonQuery();
        }
    }
}
