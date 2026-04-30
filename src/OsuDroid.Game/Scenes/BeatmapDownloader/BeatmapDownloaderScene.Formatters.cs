using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private string RankedStatusText(BeatmapRankedStatus rankedStatus) =>
        rankedStatus switch
        {
            BeatmapRankedStatus.Ranked => _localizer["BeatmapDownloader_Ranked"],
            BeatmapRankedStatus.Approved => _localizer["BeatmapDownloader_Approved"],
            BeatmapRankedStatus.Qualified => _localizer["BeatmapDownloader_Qualified"],
            BeatmapRankedStatus.Loved => _localizer["BeatmapDownloader_Loved"],
            BeatmapRankedStatus.WorkInProgress => _localizer["BeatmapDownloader_WorkInProgress"],
            BeatmapRankedStatus.Graveyard => _localizer["BeatmapDownloader_Graveyard"],
            BeatmapRankedStatus.Pending => _localizer["BeatmapDownloader_Pending"],
            _ => _localizer["BeatmapDownloader_Pending"],
        };

    private string SortText(BeatmapMirrorSort value) =>
        value switch
        {
            BeatmapMirrorSort.Bpm => _localizer["Sort_Bpm"],
            BeatmapMirrorSort.DifficultyRating => _localizer["Sort_DifficultyRating"],
            BeatmapMirrorSort.HitLength => _localizer["Sort_HitLength"],
            BeatmapMirrorSort.PassCount => _localizer["Sort_PassCount"],
            BeatmapMirrorSort.PlayCount => _localizer["Sort_PlayCount"],
            BeatmapMirrorSort.TotalLength => _localizer["Sort_TotalLength"],
            BeatmapMirrorSort.FavouriteCount => _localizer["Sort_FavouriteCount"],
            BeatmapMirrorSort.LastUpdated => _localizer["Sort_LastUpdated"],
            BeatmapMirrorSort.RankedDate => _localizer["Sort_RankedDate"],
            BeatmapMirrorSort.SubmittedDate => _localizer["Sort_SubmittedDate"],
            BeatmapMirrorSort.Title => _localizer["Sort_Title"],
            BeatmapMirrorSort.Artist => _localizer["Sort_Title"],
            _ => _localizer["Sort_Title"],
        };

    private static BeatmapMirrorSort Next(BeatmapMirrorSort value)
    {
        BeatmapMirrorSort[] values = Enum.GetValues<BeatmapMirrorSort>();
        return values[(Array.IndexOf(values, value) + 1) % values.Length];
    }

    private static string FormatDownloadInfo(BeatmapDownloadProgress? progress)
    {
        if (progress is null)
        {
            return string.Empty;
        }

        string percent = progress.Percent is double p
            ? FormattableString.Invariant($" ({p:0}%)")
            : string.Empty;
        string speed =
            progress.SpeedBytesPerSecond > 0
                ? FormattableString.Invariant(
                    $"\n{progress.SpeedBytesPerSecond / 1024d / 1024d:0.###} mb/s"
                ) + percent
                : percent;
        return speed;
    }

    private string FormatDownloadText(BeatmapDownloadState state)
    {
        string key = state.Progress?.Phase switch
        {
            BeatmapDownloadPhase.Connecting => "BeatmapDownloader_Connecting",
            BeatmapDownloadPhase.Importing => "BeatmapDownloader_Importing",
            BeatmapDownloadPhase.Downloading => "BeatmapDownloader_Downloading",
            null => "BeatmapDownloader_Downloading",
            _ => "BeatmapDownloader_Downloading",
        };

        return !string.IsNullOrWhiteSpace(state.Filename)
            ? _localizer.Format(key, state.Filename)
            : _localizer[key]
                .Replace(" {0}", string.Empty, StringComparison.Ordinal)
                .Replace("{0}", string.Empty, StringComparison.Ordinal);
    }

    private static void TryDelete(string path)
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
}
