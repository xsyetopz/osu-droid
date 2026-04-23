using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private void AddFilterPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float y = BarHeight;
        elements.Add(Fill("downloader-filter-dismiss", new UiRect(0f, y, viewport.VirtualWidth, viewport.VirtualHeight - y), s_background, 0f, UiAction.DownloaderFilters));
        elements.Add(Fill("downloader-filter-panel", new UiRect(0f, y, viewport.VirtualWidth, 64f * Dp), s_filterPanel));
        float x = 12f * Dp;
        elements.Add(Text("downloader-filter-sort-label", _localizer["BeatmapDownloader_SortBy"], x, y + 18f * Dp, 58f * Dp, 22f * Dp, 13f * Dp, s_secondary));
        AddDropdownButton(elements, "downloader-filter-sort", new UiRect(x + 62f * Dp, y + 8f * Dp, 170f * Dp, 42f * Dp), SortText(_sort), UiAction.DownloaderSort);
        elements.Add(Text("downloader-filter-order-label", _localizer["BeatmapDownloader_Order"], x + 250f * Dp, y + 18f * Dp, 52f * Dp, 22f * Dp, 13f * Dp, s_secondary));
        AddButton(elements, "downloader-filter-order", new UiRect(x + 306f * Dp, y + 8f * Dp, 126f * Dp, 42f * Dp), _order == BeatmapMirrorOrder.Ascending ? _localizer["BeatmapDownloader_Ascending"] : _localizer["BeatmapDownloader_Descending"], UiAction.DownloaderOrder);
        elements.Add(Text("downloader-filter-status-label", _localizer["BeatmapDownloader_RankedStatus"], x + 450f * Dp, y + 18f * Dp, 92f * Dp, 22f * Dp, 13f * Dp, s_secondary));
        AddDropdownButton(elements, "downloader-filter-status", new UiRect(x + 546f * Dp, y + 8f * Dp, 150f * Dp, 42f * Dp), _status is null ? _localizer["Common_All"] : RankedStatusText(_status.Value), UiAction.DownloaderStatus);
        if (_sortDropdownOpen)
        {
            AddDropdownOptions(elements, "downloader-sort-option", x + 62f * Dp, y + 54f * Dp, 176f * Dp, SortOptions(), _sortDropdownScroll, viewport);
        }

        if (_statusDropdownOpen)
        {
            AddDropdownOptions(elements, "downloader-status-option", x + 546f * Dp, y + 54f * Dp, 150f * Dp, StatusOptions(), _statusDropdownScroll, viewport);
        }
    }

    private (string Text, UiAction Action)[] SortOptions() =>
    [
        (_localizer["Sort_Title"], UiAction.DownloaderSortTitle),
        (_localizer["Sort_Artist"], UiAction.DownloaderSortArtist),
        (_localizer["Sort_Bpm"], UiAction.DownloaderSortBpm),
        (_localizer["Sort_DifficultyRating"], UiAction.DownloaderSortDifficultyRating),
        (_localizer["Sort_HitLength"], UiAction.DownloaderSortHitLength),
        (_localizer["Sort_PassCount"], UiAction.DownloaderSortPassCount),
        (_localizer["Sort_PlayCount"], UiAction.DownloaderSortPlayCount),
        (_localizer["Sort_TotalLength"], UiAction.DownloaderSortTotalLength),
        (_localizer["Sort_FavouriteCount"], UiAction.DownloaderSortFavouriteCount),
        (_localizer["Sort_LastUpdated"], UiAction.DownloaderSortLastUpdated),
        (_localizer["Sort_RankedDate"], UiAction.DownloaderSortRankedDate),
        (_localizer["Sort_SubmittedDate"], UiAction.DownloaderSortSubmittedDate),
    ];

    private (string Text, UiAction Action)[] StatusOptions() =>
    [
        (_localizer["Common_All"], UiAction.DownloaderStatusAll),
        (_localizer["BeatmapDownloader_Ranked"], UiAction.DownloaderStatusRanked),
        (_localizer["BeatmapDownloader_Approved"], UiAction.DownloaderStatusApproved),
        (_localizer["BeatmapDownloader_Qualified"], UiAction.DownloaderStatusQualified),
        (_localizer["BeatmapDownloader_Loved"], UiAction.DownloaderStatusLoved),
        (_localizer["BeatmapDownloader_Pending"], UiAction.DownloaderStatusPending),
        (_localizer["BeatmapDownloader_WorkInProgress"], UiAction.DownloaderStatusWorkInProgress),
        (_localizer["BeatmapDownloader_Graveyard"], UiAction.DownloaderStatusGraveyard),
    ];

    private static void AddDropdownOptions(List<UiElementSnapshot> elements, string id, float x, float y, float width, (string Text, UiAction Action)[] actions, float scroll, VirtualViewport viewport)
    {
        float rowHeight = 42f * Dp;
        float padding = 8f * Dp;
        float availableHeight = Math.Max(rowHeight, viewport.VirtualHeight - y);
        float height = Math.Min(actions.Length * (rowHeight + 4f * Dp) + padding * 2f, availableHeight);
        elements.Add(Fill(id + "-panel", new UiRect(x, y, width, height), UiColor.Opaque(40, 40, 61), 1f, UiAction.None, Radius));
        int first = Math.Max(0, (int)MathF.Floor(scroll / (rowHeight + 4f * Dp)));
        float offsetY = padding;
        for (int i = first; i < actions.Length; i++)
        {
            float rowY = y + offsetY + (i - first) * (rowHeight + 4f * Dp);
            if (rowY > y + height - padding)
            {
                break;
            }

            AddButton(elements, $"{id}-{i}", new UiRect(x + padding, rowY, width - padding * 2f, rowHeight), actions[i].Text, actions[i].Action);
        }
    }

    private void AddMirrorPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float width = 210f * Dp;
        float x = viewport.VirtualWidth - width - 12f * Dp;
        float y = BarHeight + 4f * Dp;
        elements.Add(Fill("downloader-mirror-panel", new UiRect(x, y, width, 98f * Dp), s_panel, 1f, UiAction.None, Radius));
        AddMirrorOption(elements, BeatmapMirrorKind.OsuDirect, x + 8f * Dp, y + 8f * Dp, UiAction.DownloaderMirrorOsuDirect);
        AddMirrorOption(elements, BeatmapMirrorKind.Catboy, x + 8f * Dp, y + 52f * Dp, UiAction.DownloaderMirrorCatboy);
    }

    private void AddMirrorOption(List<UiElementSnapshot> elements, BeatmapMirrorKind kind, float x, float y, UiAction action)
    {
        BeatmapMirrorDefinition definition = MirrorDefinition(kind);
        elements.Add(Fill($"downloader-mirror-{kind}", new UiRect(x, y, 194f * Dp, 38f * Dp), kind == _mirror ? s_field : s_panel, 1f, action, Radius));
        elements.Add(new UiElementSnapshot($"downloader-mirror-{kind}-logo", UiElementKind.Sprite, new UiRect(x + 10f * Dp, y + 7f * Dp, 24f * Dp, 24f * Dp), s_white, 1f, definition.LogoAssetName, action));
        elements.Add(Text($"downloader-mirror-{kind}-text", definition.Description, x + 44f * Dp, y + 8f * Dp, 120f * Dp, 22f * Dp, 14f * Dp, s_white, UiTextAlignment.Left, action));
    }
}
