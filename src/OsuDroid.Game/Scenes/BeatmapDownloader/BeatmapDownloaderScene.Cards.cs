using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private void AddCards(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (_sets.Count == 0)
        {
            if (!_isSearching)
            {
                string statusText = _message ?? _localizer["BeatmapDownloader_NoBeatmapsFound"];
                elements.Add(Text("downloader-status", statusText, 70f * Dp, BarHeight + 42f * Dp, viewport.VirtualWidth - 140f * Dp, 40f * Dp, 20f * Dp, s_white, UiTextAlignment.Center));
            }

            return;
        }

        int columns = Math.Max(1, (int)MathF.Floor(viewport.VirtualWidth / (CardWidth + CardMargin * 2f)));
        float gridWidth = columns * (CardWidth + CardMargin * 2f);
        float gridX = MathF.Max(0f, (viewport.VirtualWidth - gridWidth) / 2f);
        float top = BarHeight + 18f * Dp;
        float rowStep = CardHeight + CardMargin * 2f;
        int firstRow = Math.Max(0, (int)MathF.Floor(_scrollOffset / rowStep));
        _visibleStartIndex = firstRow * columns;
        int count = Math.Min(VisibleSlots, Math.Max(0, _sets.Count - _visibleStartIndex));

        for (int slot = 0; slot < count; slot++)
        {
            int index = _visibleStartIndex + slot;
            int column = index % columns;
            int row = index / columns;
            float x = gridX + column * (CardWidth + CardMargin * 2f) + CardMargin;
            float y = top + row * rowStep - _scrollOffset;
            AddCard(elements, _sets[index], slot, index, x, y);
        }

        if (_isSearching)
        {
            elements.Add(Text("downloader-loading-more", _localizer["BeatmapDownloader_LoadingMore"], 0f, viewport.VirtualHeight - 38f * Dp, viewport.VirtualWidth, 24f * Dp, 16f * Dp, s_secondary, UiTextAlignment.Center));
        }
        else if (!string.IsNullOrWhiteSpace(_message))
        {
            elements.Add(Text("downloader-message", _message, 0f, viewport.VirtualHeight - 38f * Dp, viewport.VirtualWidth, 24f * Dp, 16f * Dp, s_secondary, UiTextAlignment.Center));
        }
    }

    private void AddCard(List<UiElementSnapshot> elements, BeatmapMirrorSet set, int slot, int absoluteIndex, float x, float y)
    {
        UiAction cardAction = CardAction(slot);
        elements.Add(Fill($"downloader-card-{slot}", new UiRect(x, y, CardWidth, CardHeight), s_panel, 1f, cardAction, Radius));
        AddCover(elements, $"downloader-card-{slot}-cover", set, new UiRect(x, y, CardWidth, CardTopHeight), cardAction);
        elements.Add(Fill($"downloader-card-{slot}-cover-dim", new UiRect(x, y, CardWidth, CardTopHeight), s_coverFallback, 0.2f, cardAction, Radius));
        AddStatusPill(elements, $"downloader-card-{slot}-status", set.Status, x + 12f * Dp, y + 12f * Dp, cardAction);
        elements.Add(Text($"downloader-card-{slot}-title", DisplayTitle(set), x + 16f * Dp, y + 32f * Dp, CardWidth - 32f * Dp, 30f * Dp, 17f * Dp, s_white, UiTextAlignment.Center, cardAction));
        elements.Add(Text($"downloader-card-{slot}-artist", DisplayArtist(set), x + 16f * Dp, y + 60f * Dp, CardWidth - 32f * Dp, 24f * Dp, 14f * Dp, s_secondary, UiTextAlignment.Center, cardAction));
        AddDifficultyDots(elements, $"downloader-card-{slot}-diff", set.Beatmaps, x + 8f * Dp, y + CardTopHeight + (CardDifficultyHeight - DifficultyGlyphRowHeight) / 2f, CardWidth - 16f * Dp, null, true, cardAction);

        float footerY = y + CardTopHeight + CardDifficultyHeight;
        elements.Add(Fill($"downloader-card-{slot}-footer", new UiRect(x, footerY, CardWidth, CardFooterHeight), s_footer, 1f, cardAction));
        float buttonY = footerY + 12f * Dp;
        UiAction previewAction = PreviewAction(slot);
        UiAction downloadAction = DownloadAction(slot);
        UiAction noVideoAction = NoVideoAction(slot);
        bool noVideoVisible = MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo;
        var buttons = new List<DownloaderButtonSpec>
        {
            new($"downloader-card-{slot}-preview", _previewingSetIndex == absoluteIndex ? UiMaterialIcon.Pause : UiMaterialIcon.PlayArrow, string.Empty, 52f * Dp, previewAction),
            new($"downloader-card-{slot}-download", UiMaterialIcon.Download, _localizer["BeatmapDownloader_Download"], 116f * Dp, downloadAction),
        };
        if (noVideoVisible)
        {
            buttons.Add(new DownloaderButtonSpec($"downloader-card-{slot}-no-video", UiMaterialIcon.Download, _localizer["BeatmapDownloader_DownloadNoVideo"], 174f * Dp, noVideoAction));
        }

        AddButtonGroup(elements, buttons, x + CardWidth / 2f, buttonY);
        elements.Add(Text($"downloader-card-{slot}-creator", _localizer.Format("BeatmapDownloader_MappedBy", set.Creator), x + 16f * Dp, footerY + 66f * Dp, CardWidth - 32f * Dp, 24f * Dp, 13f * Dp, s_muted, UiTextAlignment.Center, cardAction));
    }
}
