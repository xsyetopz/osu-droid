using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private void AddCards(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (sets.Count == 0)
        {
            var statusText = isSearching ? $"Searching {MirrorDefinition(mirror).Description}..." : message ?? "No beatmaps found";
            elements.Add(Text("downloader-status", statusText, 70f * Dp, BarHeight + 42f * Dp, viewport.VirtualWidth - 140f * Dp, 40f * Dp, 20f * Dp, White, UiTextAlignment.Center));
            return;
        }

        var columns = Math.Max(1, (int)MathF.Floor(viewport.VirtualWidth / (CardWidth + CardMargin * 2f)));
        var gridWidth = columns * (CardWidth + CardMargin * 2f);
        var gridX = MathF.Max(0f, (viewport.VirtualWidth - gridWidth) / 2f);
        var top = BarHeight + 18f * Dp;
        var rowStep = CardHeight + CardMargin * 2f;
        var firstRow = Math.Max(0, (int)MathF.Floor(scrollOffset / rowStep));
        visibleStartIndex = firstRow * columns;
        var count = Math.Min(VisibleSlots, Math.Max(0, sets.Count - visibleStartIndex));

        for (var slot = 0; slot < count; slot++)
        {
            var index = visibleStartIndex + slot;
            var column = index % columns;
            var row = index / columns;
            var x = gridX + column * (CardWidth + CardMargin * 2f) + CardMargin;
            var y = top + row * rowStep - scrollOffset;
            AddCard(elements, sets[index], slot, index, x, y);
        }

        if (isSearching)
            elements.Add(Text("downloader-loading-more", "Loading more...", 0f, viewport.VirtualHeight - 38f * Dp, viewport.VirtualWidth, 24f * Dp, 16f * Dp, Secondary, UiTextAlignment.Center));
        else if (!string.IsNullOrWhiteSpace(message))
            elements.Add(Text("downloader-message", message, 0f, viewport.VirtualHeight - 38f * Dp, viewport.VirtualWidth, 24f * Dp, 16f * Dp, Secondary, UiTextAlignment.Center));
    }

    private void AddCard(List<UiElementSnapshot> elements, BeatmapMirrorSet set, int slot, int absoluteIndex, float x, float y)
    {
        var cardAction = CardAction(slot);
        elements.Add(Fill($"downloader-card-{slot}", new UiRect(x, y, CardWidth, CardHeight), Panel, 1f, cardAction, Radius));
        AddCover(elements, $"downloader-card-{slot}-cover", set, new UiRect(x, y, CardWidth, CardTopHeight), cardAction);
        elements.Add(Fill($"downloader-card-{slot}-cover-dim", new UiRect(x, y, CardWidth, CardTopHeight), CoverFallback, 0.2f, cardAction, Radius));
        AddStatusPill(elements, $"downloader-card-{slot}-status", set.Status, x + 12f * Dp, y + 12f * Dp, cardAction);
        elements.Add(Text($"downloader-card-{slot}-title", DisplayTitle(set), x + 16f * Dp, y + 32f * Dp, CardWidth - 32f * Dp, 30f * Dp, 17f * Dp, White, UiTextAlignment.Center, cardAction));
        elements.Add(Text($"downloader-card-{slot}-artist", DisplayArtist(set), x + 16f * Dp, y + 60f * Dp, CardWidth - 32f * Dp, 24f * Dp, 14f * Dp, Secondary, UiTextAlignment.Center, cardAction));
        AddDifficultyDots(elements, $"downloader-card-{slot}-diff", set.Beatmaps, x + 8f * Dp, y + CardTopHeight + (CardDifficultyHeight - DifficultyGlyphRowHeight) / 2f, CardWidth - 16f * Dp, UiAction.None, null, true, cardAction);

        var footerY = y + CardTopHeight + CardDifficultyHeight;
        elements.Add(Fill($"downloader-card-{slot}-footer", new UiRect(x, footerY, CardWidth, CardFooterHeight), Footer, 1f, cardAction));
        var buttonY = footerY + 12f * Dp;
        var previewAction = PreviewAction(slot);
        var downloadAction = DownloadAction(slot);
        var noVideoAction = NoVideoAction(slot);
        var noVideoVisible = MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo;
        var buttons = new List<DownloaderButtonSpec>
        {
            new($"downloader-card-{slot}-preview", previewingSetIndex == absoluteIndex ? UiMaterialIcon.Pause : UiMaterialIcon.PlayArrow, string.Empty, 52f * Dp, previewAction),
            new($"downloader-card-{slot}-download", UiMaterialIcon.Download, "Download", 116f * Dp, downloadAction),
        };
        if (noVideoVisible)
            buttons.Add(new DownloaderButtonSpec($"downloader-card-{slot}-no-video", UiMaterialIcon.Download, "Download (no video)", 174f * Dp, noVideoAction));
        AddButtonGroup(elements, buttons, x + CardWidth / 2f, buttonY);
        elements.Add(Text($"downloader-card-{slot}-creator", $"Mapped by {set.Creator}", x + 16f * Dp, footerY + 66f * Dp, CardWidth - 32f * Dp, 24f * Dp, 13f * Dp, Muted, UiTextAlignment.Center, cardAction));
    }
}
