using System.IO.Compression;
using System.Text.RegularExpressions;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Beatmaps.Import;

public interface IBeatmapImportService
{
    BeatmapImportResult ImportOsz(string oszPath, bool deleteArchiveAfterImport = true);
}

public sealed record BeatmapImportResult(bool IsSuccess, string? SetDirectory, string? ErrorMessage)
{
    public static BeatmapImportResult Success(string setDirectory) => new(true, setDirectory, null);

    public static BeatmapImportResult Failed(string errorMessage) => new(false, null, errorMessage);
}

public sealed partial class BeatmapImportService(DroidGamePathLayout paths, IBeatmapLibrary library) : IBeatmapImportService
{
    public BeatmapImportResult ImportOsz(string oszPath, bool deleteArchiveAfterImport = true)
    {
        if (!File.Exists(oszPath))
            return BeatmapImportResult.Failed("Beatmap archive not found.");

        Directory.CreateDirectory(paths.Songs);
        var setDirectory = Path.GetFileNameWithoutExtension(oszPath);
        var targetDirectory = Path.Combine(paths.Songs, setDirectory);

        try
        {
            if (Directory.Exists(targetDirectory))
                Directory.Delete(targetDirectory, true);

            Directory.CreateDirectory(targetDirectory);
            ExtractZipSafely(oszPath, targetDirectory);
            if (deleteArchiveAfterImport)
                File.Delete(oszPath);
            library.Scan(new HashSet<string>(StringComparer.Ordinal) { setDirectory });
            return BeatmapImportResult.Success(setDirectory);
        }
        catch (InvalidDataException)
        {
            TryDeleteDirectory(targetDirectory);
            TryMoveBadZip(oszPath);
            return BeatmapImportResult.Failed("Downloaded beatmap archive is invalid.");
        }
        catch (Exception exception)
        {
            TryDeleteDirectory(targetDirectory);
            return BeatmapImportResult.Failed(exception.Message);
        }
    }

    public static string SanitizeArchiveName(string name)
    {
        var sanitized = InvalidArchiveNameCharacters().Replace(name, "_").Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "beatmap" : sanitized;
    }

    private static void ExtractZipSafely(string sourcePath, string targetDirectory)
    {
        using var archive = ZipFile.OpenRead(sourcePath);
        var targetRoot = Path.GetFullPath(targetDirectory) + Path.DirectorySeparatorChar;

        foreach (var entry in archive.Entries)
        {
            var destination = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));
            if (!destination.StartsWith(targetRoot, StringComparison.Ordinal))
                throw new InvalidDataException("Archive entry escapes target directory.");

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destination);
                continue;
            }

            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            entry.ExtractToFile(destination, true);
        }
    }

    private static void TryMoveBadZip(string path)
    {
        try
        {
            var badZipPath = Path.ChangeExtension(path, ".badzip");
            if (File.Exists(badZipPath))
                File.Delete(badZipPath);
            File.Move(path, badZipPath);
        }
        catch (Exception)
        {
        }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
        catch (Exception)
        {
        }
    }

    [GeneratedRegex("[\\\"*/:<>?\\\\|]", RegexOptions.CultureInvariant)]
    private static partial Regex InvalidArchiveNameCharacters();
}
