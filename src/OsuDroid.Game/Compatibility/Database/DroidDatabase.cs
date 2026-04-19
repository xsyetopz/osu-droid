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
    }

    public SqliteConnection OpenConnection()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = Path };
        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        return connection;
    }
}
