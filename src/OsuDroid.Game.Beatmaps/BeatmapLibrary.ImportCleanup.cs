using OsuDroid.Game.Runtime.Settings;

namespace OsuDroid.Game.Beatmaps;

public sealed partial class BeatmapLibrary
{
    private bool DeleteUnimportedBeatmaps() =>
        settingsStore?.GetBool("deleteUnimportedBeatmaps", false) ?? false;

    private bool DeleteUnsupportedVideos() =>
        settingsStore?.GetBool("deleteUnsupportedVideos", true) ?? true;

    private void DeleteUnsupportedVideoIfNeeded(string osuFile)
    {
        if (!DeleteUnsupportedVideos())
        {
            return;
        }

        string? videoFilename = BeatmapFileParser.ParseVideoFilename(osuFile);
        if (string.IsNullOrWhiteSpace(videoFilename) || IsSupportedVideo(videoFilename))
        {
            return;
        }

        string? setDirectory = Path.GetDirectoryName(osuFile);
        if (string.IsNullOrWhiteSpace(setDirectory))
        {
            return;
        }

        TryDeleteFile(Path.Combine(setDirectory, videoFilename));
    }

    private static bool IsSupportedVideo(string filename) =>
        Path.GetExtension(filename).ToLowerInvariant() is ".3gp" or ".mp4" or ".mkv" or ".webm";

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception) { }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (Exception) { }
    }
}
