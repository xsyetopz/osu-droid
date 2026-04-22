using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private bool HandleDownloaderUiAction(UiAction action, VirtualViewport viewport) => action switch
    {
        UiAction.DownloaderBack => BackFromDownloader(),
        UiAction.DownloaderSearchBox => Do(() => beatmapDownloader.FocusSearch(viewport)),
        UiAction.DownloaderSearchSubmit => Do(() => beatmapDownloader.SubmitSearch(beatmapDownloader.Query)),
        UiAction.DownloaderRefresh => Do(beatmapDownloader.Refresh),
        UiAction.DownloaderFilters => Do(beatmapDownloader.ToggleFilters),
        UiAction.DownloaderMirror => Do(beatmapDownloader.ToggleMirrorSelector),
        UiAction.DownloaderMirrorOsuDirect => SelectMirror(BeatmapMirrorKind.OsuDirect),
        UiAction.DownloaderMirrorCatboy => SelectMirror(BeatmapMirrorKind.Catboy),
        UiAction.DownloaderSort => Do(beatmapDownloader.ToggleSortDropdown),
        UiAction.DownloaderSortTitle => SelectDownloaderSort(BeatmapMirrorSort.Title),
        UiAction.DownloaderSortArtist => SelectDownloaderSort(BeatmapMirrorSort.Artist),
        UiAction.DownloaderSortBpm => SelectDownloaderSort(BeatmapMirrorSort.Bpm),
        UiAction.DownloaderSortDifficultyRating => SelectDownloaderSort(BeatmapMirrorSort.DifficultyRating),
        UiAction.DownloaderSortHitLength => SelectDownloaderSort(BeatmapMirrorSort.HitLength),
        UiAction.DownloaderSortPassCount => SelectDownloaderSort(BeatmapMirrorSort.PassCount),
        UiAction.DownloaderSortPlayCount => SelectDownloaderSort(BeatmapMirrorSort.PlayCount),
        UiAction.DownloaderSortTotalLength => SelectDownloaderSort(BeatmapMirrorSort.TotalLength),
        UiAction.DownloaderSortFavouriteCount => SelectDownloaderSort(BeatmapMirrorSort.FavouriteCount),
        UiAction.DownloaderSortLastUpdated => SelectDownloaderSort(BeatmapMirrorSort.LastUpdated),
        UiAction.DownloaderSortRankedDate => SelectDownloaderSort(BeatmapMirrorSort.RankedDate),
        UiAction.DownloaderSortSubmittedDate => SelectDownloaderSort(BeatmapMirrorSort.SubmittedDate),
        UiAction.DownloaderOrder => Do(beatmapDownloader.ToggleOrder),
        UiAction.DownloaderStatus => Do(beatmapDownloader.ToggleStatusDropdown),
        UiAction.DownloaderStatusAll => SelectDownloaderStatus(null),
        UiAction.DownloaderStatusRanked => SelectDownloaderStatus(BeatmapRankedStatus.Ranked),
        UiAction.DownloaderStatusApproved => SelectDownloaderStatus(BeatmapRankedStatus.Approved),
        UiAction.DownloaderStatusQualified => SelectDownloaderStatus(BeatmapRankedStatus.Qualified),
        UiAction.DownloaderStatusLoved => SelectDownloaderStatus(BeatmapRankedStatus.Loved),
        UiAction.DownloaderStatusPending => SelectDownloaderStatus(BeatmapRankedStatus.Pending),
        UiAction.DownloaderStatusWorkInProgress => SelectDownloaderStatus(BeatmapRankedStatus.WorkInProgress),
        UiAction.DownloaderStatusGraveyard => SelectDownloaderStatus(BeatmapRankedStatus.Graveyard),
        UiAction.DownloaderDetailsClose => Do(beatmapDownloader.CloseDetails),
        UiAction.DownloaderDetailsPanel => true,
        UiAction.DownloaderDetailsPreview => Do(beatmapDownloader.PreviewDetails),
        UiAction.DownloaderDetailsDownload => Do(() => beatmapDownloader.DownloadDetails(true)),
        UiAction.DownloaderDetailsDownloadNoVideo => Do(() => beatmapDownloader.DownloadDetails(false)),
        UiAction.DownloaderDownloadCancel => Do(beatmapDownloader.CancelDownload),
        _ when IsDownloaderDownloadAction(action) => DownloadVisible(action),
        _ => false,
    };

    private bool BackFromDownloader()
    {
        textInputService.HideTextInput();
        BackToMainMenu();
        return true;
    }

    private bool DownloadVisible(UiAction action) => Do(() => beatmapDownloader.DownloadVisible(BeatmapDownloaderScene.DownloadIndex(action), !BeatmapDownloaderScene.IsNoVideoAction(action)));

    private bool SelectMirror(BeatmapMirrorKind mirror) => Do(() => beatmapDownloader.SelectMirror(mirror));

    private bool SelectDownloaderSort(BeatmapMirrorSort sort) => Do(() => beatmapDownloader.SetSort(sort));

    private bool SelectDownloaderStatus(BeatmapRankedStatus? status) => Do(() => beatmapDownloader.SetStatus(status));

    private static bool IsDownloaderDownloadAction(UiAction action) => action is
        UiAction.DownloaderDownloadFirst or UiAction.DownloaderDownloadFirstNoVideo or
        UiAction.DownloaderDownload0 or UiAction.DownloaderDownload1 or UiAction.DownloaderDownload2 or UiAction.DownloaderDownload3 or
        UiAction.DownloaderDownload4 or UiAction.DownloaderDownload5 or UiAction.DownloaderDownload6 or UiAction.DownloaderDownload7 or
        UiAction.DownloaderDownloadNoVideo0 or UiAction.DownloaderDownloadNoVideo1 or UiAction.DownloaderDownloadNoVideo2 or UiAction.DownloaderDownloadNoVideo3 or
        UiAction.DownloaderDownloadNoVideo4 or UiAction.DownloaderDownloadNoVideo5 or UiAction.DownloaderDownloadNoVideo6 or UiAction.DownloaderDownloadNoVideo7;

    private static bool Do(Action action)
    {
        action();
        return true;
    }
}
