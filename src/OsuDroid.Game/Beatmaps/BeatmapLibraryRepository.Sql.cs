using Microsoft.Data.Sqlite;

namespace OsuDroid.Game.Beatmaps;

public sealed partial class BeatmapLibraryRepository
{
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
