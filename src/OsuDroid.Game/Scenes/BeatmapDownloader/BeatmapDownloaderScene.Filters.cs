using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private void AddFilterPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var y = BarHeight;
        elements.Add(Fill("downloader-filter-dismiss", new UiRect(0f, y, viewport.VirtualWidth, viewport.VirtualHeight - y), Background, 0f, UiAction.DownloaderFilters));
        elements.Add(Fill("downloader-filter-panel", new UiRect(0f, y, viewport.VirtualWidth, 64f * Dp), FilterPanel));
        var x = 12f * Dp;
        elements.Add(Text("downloader-filter-sort-label", "Sort by", x, y + 18f * Dp, 58f * Dp, 22f * Dp, 13f * Dp, Secondary));
        AddDropdownButton(elements, "downloader-filter-sort", new UiRect(x + 62f * Dp, y + 8f * Dp, 170f * Dp, 42f * Dp), SortText(sort), UiAction.DownloaderSort);
        elements.Add(Text("downloader-filter-order-label", "Order", x + 250f * Dp, y + 18f * Dp, 52f * Dp, 22f * Dp, 13f * Dp, Secondary));
        AddButton(elements, "downloader-filter-order", new UiRect(x + 306f * Dp, y + 8f * Dp, 126f * Dp, 42f * Dp), order == BeatmapMirrorOrder.Ascending ? "Ascending" : "Descending", UiAction.DownloaderOrder);
        elements.Add(Text("downloader-filter-status-label", "Status", x + 450f * Dp, y + 18f * Dp, 52f * Dp, 22f * Dp, 13f * Dp, Secondary));
        AddDropdownButton(elements, "downloader-filter-status", new UiRect(x + 506f * Dp, y + 8f * Dp, 150f * Dp, 42f * Dp), status is null ? "All" : RankedStatusText(status.Value), UiAction.DownloaderStatus);
        if (sortDropdownOpen)
            AddDropdownOptions(elements, "downloader-sort-option", x + 62f * Dp, y + 54f * Dp, 176f * Dp, SortOptions(), sortDropdownScroll, viewport);
        if (statusDropdownOpen)
            AddDropdownOptions(elements, "downloader-status-option", x + 506f * Dp, y + 54f * Dp, 150f * Dp, StatusOptions(), statusDropdownScroll, viewport);
    }

    private static (string Text, UiAction Action)[] SortOptions() =>
    [
        ("Title", UiAction.DownloaderSortTitle),
        ("Artist", UiAction.DownloaderSortArtist),
        ("BPM", UiAction.DownloaderSortBpm),
        ("Difficulty rating", UiAction.DownloaderSortDifficultyRating),
        ("Hit length", UiAction.DownloaderSortHitLength),
        ("Pass count", UiAction.DownloaderSortPassCount),
        ("Play count", UiAction.DownloaderSortPlayCount),
        ("Total length", UiAction.DownloaderSortTotalLength),
        ("Favourite count", UiAction.DownloaderSortFavouriteCount),
        ("Last updated", UiAction.DownloaderSortLastUpdated),
        ("Ranked date", UiAction.DownloaderSortRankedDate),
        ("Submitted date", UiAction.DownloaderSortSubmittedDate),
    ];

    private static (string Text, UiAction Action)[] StatusOptions() =>
    [
        ("All", UiAction.DownloaderStatusAll),
        ("Ranked", UiAction.DownloaderStatusRanked),
        ("Approved", UiAction.DownloaderStatusApproved),
        ("Qualified", UiAction.DownloaderStatusQualified),
        ("Loved", UiAction.DownloaderStatusLoved),
        ("Pending", UiAction.DownloaderStatusPending),
        ("WIP", UiAction.DownloaderStatusWorkInProgress),
        ("Graveyard", UiAction.DownloaderStatusGraveyard),
    ];

    private static void AddDropdownOptions(List<UiElementSnapshot> elements, string id, float x, float y, float width, (string Text, UiAction Action)[] actions, float scroll, VirtualViewport viewport)
    {
        var rowHeight = 42f * Dp;
        var padding = 8f * Dp;
        var availableHeight = Math.Max(rowHeight, viewport.VirtualHeight - y);
        var height = Math.Min(actions.Length * (rowHeight + 4f * Dp) + padding * 2f, availableHeight);
        elements.Add(Fill(id + "-panel", new UiRect(x, y, width, height), UiColor.Opaque(40, 40, 61), 1f, UiAction.None, Radius));
        var first = Math.Max(0, (int)MathF.Floor(scroll / (rowHeight + 4f * Dp)));
        var offsetY = padding;
        for (var i = first; i < actions.Length; i++)
        {
            var rowY = y + offsetY + (i - first) * (rowHeight + 4f * Dp);
            if (rowY > y + height - padding)
                break;

            AddButton(elements, $"{id}-{i}", new UiRect(x + padding, rowY, width - padding * 2f, rowHeight), actions[i].Text, actions[i].Action);
        }
    }

    private void AddMirrorPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var width = 210f * Dp;
        var x = viewport.VirtualWidth - width - 12f * Dp;
        var y = BarHeight + 4f * Dp;
        elements.Add(Fill("downloader-mirror-panel", new UiRect(x, y, width, 98f * Dp), Panel, 1f, UiAction.None, Radius));
        AddMirrorOption(elements, BeatmapMirrorKind.OsuDirect, x + 8f * Dp, y + 8f * Dp, UiAction.DownloaderMirrorOsuDirect);
        AddMirrorOption(elements, BeatmapMirrorKind.Catboy, x + 8f * Dp, y + 52f * Dp, UiAction.DownloaderMirrorCatboy);
    }

    private void AddMirrorOption(List<UiElementSnapshot> elements, BeatmapMirrorKind kind, float x, float y, UiAction action)
    {
        var definition = MirrorDefinition(kind);
        elements.Add(Fill($"downloader-mirror-{kind}", new UiRect(x, y, 194f * Dp, 38f * Dp), kind == mirror ? Field : Panel, 1f, action, Radius));
        elements.Add(new UiElementSnapshot($"downloader-mirror-{kind}-logo", UiElementKind.Sprite, new UiRect(x + 10f * Dp, y + 7f * Dp, 24f * Dp, 24f * Dp), White, 1f, definition.LogoAssetName, action));
        elements.Add(Text($"downloader-mirror-{kind}-text", definition.Description, x + 44f * Dp, y + 8f * Dp, 120f * Dp, 22f * Dp, 14f * Dp, White, UiTextAlignment.Left, action));
    }
}
