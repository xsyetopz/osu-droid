using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private void AddFilterPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float y = BarHeight;
        elements.Add(
            Fill(
                "downloader-filter-dismiss",
                new UiRect(0f, y, viewport.VirtualWidth, viewport.VirtualHeight - y),
                s_background,
                0f,
                UiAction.DownloaderFilters
            )
        );
        elements.Add(
            Fill(
                "downloader-filter-panel",
                new UiRect(0f, y, viewport.VirtualWidth, 64f * Dp),
                s_filterPanel
            )
        );
        string sortLabel = _localizer["BeatmapDownloader_SortBy"];
        string sortText = SortText(_sort);
        string orderLabel = _localizer["BeatmapDownloader_Order"];
        string orderText =
            _order == BeatmapMirrorOrder.Ascending
                ? _localizer["BeatmapDownloader_Ascending"]
                : _localizer["BeatmapDownloader_Descending"];
        string statusLabel = _localizer["BeatmapDownloader_Status"];
        string statusText = _status is null
            ? _localizer["Common_All"]
            : RankedStatusText(_status.Value);
        float x = 12f * Dp;
        float labelTop = y + 18f * Dp;
        float buttonTop = y + 8f * Dp;
        float groupGap = 8f * Dp;
        float labelGap = 4f * Dp;
        float sortLabelWidth = LabelWidth(sortLabel);
        float sortWidth = ButtonWidth(sortText, hasDropdown: true);
        float orderLabelWidth = LabelWidth(orderLabel);
        float orderWidth = ButtonWidth(orderText, hasDropdown: false);
        float statusLabelWidth = LabelWidth(statusLabel);
        float statusWidth = ButtonWidth(statusText, hasDropdown: true);
        var sortBounds = new UiRect(x + sortLabelWidth + labelGap, buttonTop, sortWidth, 42f * Dp);
        float orderLabelX = sortBounds.Right + groupGap;
        var orderBounds = new UiRect(
            orderLabelX + orderLabelWidth + labelGap,
            buttonTop,
            orderWidth,
            42f * Dp
        );
        float statusLabelX = orderBounds.Right + groupGap;
        var statusBounds = new UiRect(
            statusLabelX + statusLabelWidth + labelGap,
            buttonTop,
            statusWidth,
            42f * Dp
        );
        elements.Add(
            Text(
                "downloader-filter-sort-label",
                sortLabel,
                x,
                labelTop,
                sortLabelWidth,
                22f * Dp,
                13f * Dp,
                s_filterLabel
            )
        );
        AddDropdownButton(
            elements,
            "downloader-filter-sort",
            sortBounds,
            sortText,
            UiAction.DownloaderSort
        );
        elements.Add(
            Text(
                "downloader-filter-order-label",
                orderLabel,
                orderLabelX,
                labelTop,
                orderLabelWidth,
                22f * Dp,
                13f * Dp,
                s_filterLabel
            )
        );
        AddButton(
            elements,
            "downloader-filter-order",
            orderBounds,
            orderText,
            UiAction.DownloaderOrder
        );
        elements.Add(
            Text(
                "downloader-filter-status-label",
                statusLabel,
                statusLabelX,
                labelTop,
                statusLabelWidth,
                22f * Dp,
                13f * Dp,
                s_filterLabel
            )
        );
        AddDropdownButton(
            elements,
            "downloader-filter-status",
            statusBounds,
            statusText,
            UiAction.DownloaderStatus
        );
        if (_sortDropdownOpen)
        {
            AddDropdownOptions(
                elements,
                "downloader-sort-option",
                sortBounds.X,
                sortBounds.Y,
                sortBounds.Width,
                SortOptions(),
                SortAction(_sort),
                _sortDropdownScroll,
                viewport
            );
        }

        if (_statusDropdownOpen)
        {
            AddDropdownOptions(
                elements,
                "downloader-status-option",
                statusBounds.X,
                statusBounds.Y,
                statusBounds.Width,
                StatusOptions(),
                StatusAction(_status),
                _statusDropdownScroll,
                viewport
            );
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
            (
                _localizer["BeatmapDownloader_WorkInProgress"],
                UiAction.DownloaderStatusWorkInProgress
            ),
            (_localizer["BeatmapDownloader_Graveyard"], UiAction.DownloaderStatusGraveyard),
        ];

    private static UiAction SortAction(BeatmapMirrorSort sort) =>
        sort switch
        {
            BeatmapMirrorSort.Title => UiAction.DownloaderSortTitle,
            BeatmapMirrorSort.Artist => UiAction.DownloaderSortArtist,
            BeatmapMirrorSort.Bpm => UiAction.DownloaderSortBpm,
            BeatmapMirrorSort.DifficultyRating => UiAction.DownloaderSortDifficultyRating,
            BeatmapMirrorSort.HitLength => UiAction.DownloaderSortHitLength,
            BeatmapMirrorSort.PassCount => UiAction.DownloaderSortPassCount,
            BeatmapMirrorSort.PlayCount => UiAction.DownloaderSortPlayCount,
            BeatmapMirrorSort.TotalLength => UiAction.DownloaderSortTotalLength,
            BeatmapMirrorSort.FavouriteCount => UiAction.DownloaderSortFavouriteCount,
            BeatmapMirrorSort.LastUpdated => UiAction.DownloaderSortLastUpdated,
            BeatmapMirrorSort.RankedDate => UiAction.DownloaderSortRankedDate,
            BeatmapMirrorSort.SubmittedDate => UiAction.DownloaderSortSubmittedDate,
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null),
        };

    private static UiAction StatusAction(BeatmapRankedStatus? status) =>
        status switch
        {
            BeatmapRankedStatus.Ranked => UiAction.DownloaderStatusRanked,
            BeatmapRankedStatus.Approved => UiAction.DownloaderStatusApproved,
            BeatmapRankedStatus.Qualified => UiAction.DownloaderStatusQualified,
            BeatmapRankedStatus.Loved => UiAction.DownloaderStatusLoved,
            BeatmapRankedStatus.Pending => UiAction.DownloaderStatusPending,
            BeatmapRankedStatus.WorkInProgress => UiAction.DownloaderStatusWorkInProgress,
            BeatmapRankedStatus.Graveyard => UiAction.DownloaderStatusGraveyard,
            _ => UiAction.DownloaderStatusAll,
        };

    private static void AddDropdownOptions(
        List<UiElementSnapshot> elements,
        string id,
        float x,
        float y,
        float width,
        (string Text, UiAction Action)[] actions,
        UiAction selectedAction,
        float scroll,
        VirtualViewport viewport
    )
    {
        float rowHeight = 42f * Dp;
        float padding = 8f * Dp;
        float availableHeight = Math.Max(rowHeight, viewport.VirtualHeight - y);
        float rowStep = rowHeight + 4f * Dp;
        float panelWidth = DropdownWidth(
            width,
            actions,
            Math.Max(width, viewport.VirtualWidth - x)
        );
        float height = Math.Min(actions.Length * rowStep + padding * 2f, availableHeight);
        elements.Add(
            Fill(
                id + "-panel",
                new UiRect(x, y, panelWidth, height),
                s_dropdownPanel,
                1f,
                UiAction.None,
                Radius
            )
        );
        int first = Math.Max(0, (int)MathF.Floor(scroll / rowStep));
        float offsetY = padding;
        for (int i = first; i < actions.Length; i++)
        {
            float rowY = y + offsetY + (i - first) * rowStep;
            if (rowY > y + height - padding)
            {
                break;
            }

            AddDropdownOption(
                elements,
                $"{id}-{i}",
                new UiRect(x + padding, rowY, panelWidth - padding * 2f, rowHeight),
                actions[i].Text,
                actions[i].Action,
                actions[i].Action == selectedAction
            );
        }
    }

    private void AddMirrorDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float width = Math.Min(Math.Max(400f * Dp, 420f * Dp), viewport.VirtualWidth - 48f * Dp);
        float titleHeight = 44f * Dp;
        float dividerHeight = 1f * Dp;
        float padding = 8f * Dp;
        float rowHeight = 58f * Dp;
        float height = titleHeight + dividerHeight + padding * 2f + rowHeight * 2f;
        float x = (viewport.VirtualWidth - width) / 2f;
        float y = (viewport.VirtualHeight - height) / 2f;
        elements.Add(
            Fill(
                "downloader-mirror-scrim",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_dialogScrim,
                1f,
                UiAction.DownloaderMirror
            )
        );
        elements.Add(
            Fill(
                "downloader-mirror-dialog",
                new UiRect(x, y, width, height),
                s_appBar,
                1f,
                UiAction.DownloaderDetailsPanel,
                Radius
            )
        );
        elements.Add(
            TextMiddle(
                "downloader-mirror-title",
                _localizer["BeatmapDownloader_SelectMirror"],
                x,
                y,
                width,
                titleHeight,
                14f * Dp,
                s_secondary,
                UiTextAlignment.Center,
                UiAction.DownloaderDetailsPanel
            )
        );
        elements.Add(
            Fill(
                "downloader-mirror-divider",
                new UiRect(x, y + titleHeight, width, dividerHeight),
                s_background,
                0.45f,
                UiAction.DownloaderDetailsPanel
            )
        );
        float rowX = x + padding;
        float rowWidth = width - padding * 2f;
        float firstRowY = y + titleHeight + dividerHeight + padding;
        AddMirrorOption(
            elements,
            BeatmapMirrorKind.OsuDirect,
            new UiRect(rowX, firstRowY, rowWidth, rowHeight),
            UiAction.DownloaderMirrorOsuDirect
        );
        AddMirrorOption(
            elements,
            BeatmapMirrorKind.Catboy,
            new UiRect(rowX, firstRowY + rowHeight, rowWidth, rowHeight),
            UiAction.DownloaderMirrorCatboy
        );
    }

    private void AddMirrorOption(
        List<UiElementSnapshot> elements,
        BeatmapMirrorKind kind,
        UiRect bounds,
        UiAction action
    )
    {
        BeatmapMirrorDefinition definition = MirrorDefinition(kind);
        if (kind == _mirror)
        {
            elements.Add(
                Fill(
                    $"downloader-mirror-{kind}-selected",
                    bounds,
                    s_dropdownSelected,
                    1f,
                    action,
                    Radius
                )
            );
        }

        elements.Add(
            new UiElementSnapshot(
                $"downloader-mirror-{kind}-logo",
                UiElementKind.Sprite,
                new UiRect(bounds.X + 12f * Dp, bounds.Y + 17f * Dp, 24f * Dp, 24f * Dp),
                s_white,
                1f,
                MirrorLogoAsset(kind),
                action
            )
        );
        elements.Add(
            Text(
                $"downloader-mirror-{kind}-text",
                definition.Description,
                bounds.X + 54f * Dp,
                bounds.Y + 12f * Dp,
                bounds.Width - 102f * Dp,
                20f * Dp,
                14f * Dp,
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
        elements.Add(
            Text(
                $"downloader-mirror-{kind}-url",
                definition.HomeUrl,
                bounds.X + 54f * Dp,
                bounds.Y + 34f * Dp,
                bounds.Width - 102f * Dp,
                18f * Dp,
                13f * Dp,
                s_filterLabel,
                UiTextAlignment.Left,
                action
            )
        );
        if (kind == _mirror)
        {
            elements.Add(
                MaterialIcon(
                    $"downloader-mirror-{kind}-check",
                    UiMaterialIcon.Check,
                    new UiRect(bounds.Right - 36f * Dp, bounds.Y + 17f * Dp, 24f * Dp, 24f * Dp),
                    s_accent,
                    1f,
                    action
                )
            );
        }
    }

    private static float LabelWidth(string text) => EstimateTextWidth(text, 13f * Dp) + 12f * Dp;

    private static float DropdownWidth(
        float callerWidth,
        (string Text, UiAction Action)[] actions,
        float maxWidth
    )
    {
        float widestText =
            actions.Length == 0
                ? 0f
                : actions.Max(action => EstimateTextWidth(action.Text, 14f * Dp));
        float naturalWidth = widestText + 64f * Dp;
        return MathF.Ceiling(
            Math.Clamp(Math.Max(callerWidth, naturalWidth), callerWidth, maxWidth)
        );
    }

    private static float ButtonWidth(string text, bool hasDropdown)
    {
        float width = EstimateTextWidth(text, 14f * Dp) + 32f * Dp;
        if (hasDropdown)
        {
            width += 34f * Dp;
        }

        return MathF.Ceiling(width);
    }
}
