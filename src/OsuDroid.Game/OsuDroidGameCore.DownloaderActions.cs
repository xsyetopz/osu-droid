using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Scenes.BeatmapDownloader;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
#pragma warning disable IDE0072 // UiAction has cross-scene members; Downloader handles only its own actions.
    private bool HandleDownloaderUiAction(UiAction action, VirtualViewport viewport) =>
        action switch
        {
            UiAction.DownloaderBack => BackFromDownloader(),
            UiAction.DownloaderSearchBox => Do(() => _beatmapDownloader.FocusSearch(viewport)),
            UiAction.DownloaderSearchSubmit => Do(() =>
                _beatmapDownloader.SubmitSearch(_beatmapDownloader.Query)
            ),
            UiAction.DownloaderRefresh => Do(_beatmapDownloader.Refresh),
            UiAction.DownloaderFilters => Do(_beatmapDownloader.ToggleFilters),
            UiAction.DownloaderMirror => Do(_beatmapDownloader.ToggleMirrorSelector),
            UiAction.DownloaderMirrorOsuDirect => SelectMirror(BeatmapMirrorKind.OsuDirect),
            UiAction.DownloaderMirrorCatboy => SelectMirror(BeatmapMirrorKind.Catboy),
            UiAction.DownloaderSort => Do(_beatmapDownloader.ToggleSortDropdown),
            UiAction.DownloaderSortTitle => SelectDownloaderSort(BeatmapMirrorSort.Title),
            UiAction.DownloaderSortArtist => SelectDownloaderSort(BeatmapMirrorSort.Artist),
            UiAction.DownloaderSortBpm => SelectDownloaderSort(BeatmapMirrorSort.Bpm),
            UiAction.DownloaderSortDifficultyRating => SelectDownloaderSort(
                BeatmapMirrorSort.DifficultyRating
            ),
            UiAction.DownloaderSortHitLength => SelectDownloaderSort(BeatmapMirrorSort.HitLength),
            UiAction.DownloaderSortPassCount => SelectDownloaderSort(BeatmapMirrorSort.PassCount),
            UiAction.DownloaderSortPlayCount => SelectDownloaderSort(BeatmapMirrorSort.PlayCount),
            UiAction.DownloaderSortTotalLength => SelectDownloaderSort(
                BeatmapMirrorSort.TotalLength
            ),
            UiAction.DownloaderSortFavouriteCount => SelectDownloaderSort(
                BeatmapMirrorSort.FavouriteCount
            ),
            UiAction.DownloaderSortLastUpdated => SelectDownloaderSort(
                BeatmapMirrorSort.LastUpdated
            ),
            UiAction.DownloaderSortRankedDate => SelectDownloaderSort(BeatmapMirrorSort.RankedDate),
            UiAction.DownloaderSortSubmittedDate => SelectDownloaderSort(
                BeatmapMirrorSort.SubmittedDate
            ),
            UiAction.DownloaderOrder => Do(_beatmapDownloader.ToggleOrder),
            UiAction.DownloaderStatus => Do(_beatmapDownloader.ToggleStatusDropdown),
            UiAction.DownloaderStatusAll => SelectDownloaderStatus(null),
            UiAction.DownloaderStatusRanked => SelectDownloaderStatus(BeatmapRankedStatus.Ranked),
            UiAction.DownloaderStatusApproved => SelectDownloaderStatus(
                BeatmapRankedStatus.Approved
            ),
            UiAction.DownloaderStatusQualified => SelectDownloaderStatus(
                BeatmapRankedStatus.Qualified
            ),
            UiAction.DownloaderStatusLoved => SelectDownloaderStatus(BeatmapRankedStatus.Loved),
            UiAction.DownloaderStatusPending => SelectDownloaderStatus(BeatmapRankedStatus.Pending),
            UiAction.DownloaderStatusWorkInProgress => SelectDownloaderStatus(
                BeatmapRankedStatus.WorkInProgress
            ),
            UiAction.DownloaderStatusGraveyard => SelectDownloaderStatus(
                BeatmapRankedStatus.Graveyard
            ),
            UiAction.DownloaderDetailsClose => Do(_beatmapDownloader.CloseDetails),
            UiAction.DownloaderDetailsPanel => true,
            UiAction.DownloaderDetailsPreview => Do(_beatmapDownloader.PreviewDetails),
            UiAction.DownloaderDetailsDownload => Do(() =>
                _beatmapDownloader.DownloadDetails(true)
            ),
            UiAction.DownloaderDetailsDownloadNoVideo => Do(() =>
                _beatmapDownloader.DownloadDetails(false)
            ),
            UiAction.DownloaderDownloadCancel => Do(_beatmapDownloader.CancelDownload),
            _ when IsDownloaderDownloadAction(action) => DownloadVisible(action),
            _ => false,
        };
#pragma warning restore IDE0072

    private bool BackFromDownloader()
    {
        _textInputService.HideTextInput();
        BackToMainMenu();
        return true;
    }

    private bool DownloadVisible(UiAction action) =>
        Do(() =>
            _beatmapDownloader.DownloadVisible(
                BeatmapDownloaderScene.DownloadIndex(action),
                !BeatmapDownloaderScene.IsNoVideoAction(action)
            )
        );

    private bool SelectMirror(BeatmapMirrorKind mirror) =>
        Do(() => _beatmapDownloader.SelectMirror(mirror));

    private bool SelectDownloaderSort(BeatmapMirrorSort sort) =>
        Do(() => _beatmapDownloader.SetSort(sort));

    private bool SelectDownloaderStatus(BeatmapRankedStatus? status) =>
        Do(() => _beatmapDownloader.SetStatus(status));

    private static bool IsDownloaderDownloadAction(UiAction action) =>
        action
            is UiAction.DownloaderDownloadFirst
                or UiAction.DownloaderDownloadFirstNoVideo
                or UiAction.DownloaderResultDownloadSlot0
                or UiAction.DownloaderResultDownloadSlot1
                or UiAction.DownloaderResultDownloadSlot2
                or UiAction.DownloaderResultDownloadSlot3
                or UiAction.DownloaderResultDownloadSlot4
                or UiAction.DownloaderResultDownloadSlot5
                or UiAction.DownloaderResultDownloadSlot6
                or UiAction.DownloaderResultDownloadSlot7
                or UiAction.DownloaderResultDownloadWithoutVideoSlot0
                or UiAction.DownloaderResultDownloadWithoutVideoSlot1
                or UiAction.DownloaderResultDownloadWithoutVideoSlot2
                or UiAction.DownloaderResultDownloadWithoutVideoSlot3
                or UiAction.DownloaderResultDownloadWithoutVideoSlot4
                or UiAction.DownloaderResultDownloadWithoutVideoSlot5
                or UiAction.DownloaderResultDownloadWithoutVideoSlot6
                or UiAction.DownloaderResultDownloadWithoutVideoSlot7;

    private static bool Do(Action action)
    {
        action();
        return true;
    }
}
