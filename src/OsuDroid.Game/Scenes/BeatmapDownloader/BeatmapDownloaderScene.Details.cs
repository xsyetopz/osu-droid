using System.Globalization;
using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private void AddDetails(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (_selectedSetIndex is not int index || index < 0 || index >= _sets.Count)
        {
            return;
        }

        BeatmapMirrorSet set = _sets[index];
        IReadOnlyList<BeatmapMirrorBeatmap> beatmaps = set.Beatmaps;
        if (beatmaps.Count == 0)
        {
            return;
        }

        _selectedDifficultyIndex = Math.Clamp(_selectedDifficultyIndex, 0, beatmaps.Count - 1);
        BeatmapMirrorBeatmap beatmap = beatmaps[_selectedDifficultyIndex];
        elements.Add(Fill("downloader-details-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_modalShade, 1f, UiAction.DownloaderDetailsClose));
        float panelWidth = Math.Min(500f * Dp, viewport.VirtualWidth - 80f * Dp);
        float x = (viewport.VirtualWidth - panelWidth) / 2f;
        float y = Math.Max(20f * Dp, (viewport.VirtualHeight - 348f * Dp) / 2f);
        elements.Add(Fill("downloader-details-panel", new UiRect(x, y, panelWidth, 348f * Dp), s_panel, 1f, UiAction.DownloaderDetailsPanel, Radius));
        AddCover(elements, "downloader-details-cover", set, new UiRect(x, y, panelWidth, 100f * Dp), UiAction.DownloaderDetailsPanel);
        elements.Add(Fill("downloader-details-cover-dim", new UiRect(x, y, panelWidth, 100f * Dp), s_coverFallback, 0.2f, UiAction.DownloaderDetailsPanel, Radius));
        elements.Add(Text("downloader-details-title", DisplayTitle(set), x + 16f * Dp, y + 18f * Dp, panelWidth - 120f * Dp, 26f * Dp, 17f * Dp, s_white));
        elements.Add(Text("downloader-details-artist", DisplayArtist(set), x + 16f * Dp, y + 44f * Dp, panelWidth - 120f * Dp, 22f * Dp, 14f * Dp, s_secondary));
        elements.Add(Text("downloader-details-creator", _localizer.Format("BeatmapDownloader_MappedBy", set.Creator), x + 16f * Dp, y + 68f * Dp, panelWidth - 120f * Dp, 20f * Dp, 13f * Dp, s_muted));
        AddStatusPill(elements, "downloader-details-status", set.Status, x + panelWidth - StatusPillWidth(set.Status) - 16f * Dp, y + 16f * Dp, UiAction.DownloaderDetailsPanel);
        AddDifficultyDots(elements, "downloader-details-diff", beatmaps, x + 12f * Dp, y + 108f * Dp, panelWidth - 24f * Dp, _selectedDifficultyIndex, true, UiAction.None);
        float bodyY = y + 146f * Dp;
        elements.Add(Fill("downloader-details-body", new UiRect(x, bodyY, panelWidth, 202f * Dp), s_footer, 1f, UiAction.DownloaderDetailsPanel));
        elements.Add(Text("downloader-details-version", beatmap.Version, x + 16f * Dp, bodyY + 12f * Dp, panelWidth - 32f * Dp, 24f * Dp, 15f * Dp, s_white));
        elements.Add(Text("downloader-details-stats", _localizer.Format(
            "BeatmapDownloader_DetailsStats",
            beatmap.StarRating.ToString("0.##", CultureInfo.InvariantCulture),
            beatmap.ApproachRate.ToString("0.##", CultureInfo.InvariantCulture),
            beatmap.OverallDifficulty.ToString("0.##", CultureInfo.InvariantCulture),
            beatmap.CircleSize.ToString("0.##", CultureInfo.InvariantCulture),
            beatmap.HpDrainRate.ToString("0.##", CultureInfo.InvariantCulture),
            beatmap.CircleCount,
            beatmap.SliderCount,
            beatmap.SpinnerCount,
            TimeSpan.FromSeconds(beatmap.HitLength).ToString("m\\:ss", CultureInfo.InvariantCulture),
            beatmap.Bpm.ToString("0.##", CultureInfo.InvariantCulture)), x + 16f * Dp, bodyY + 38f * Dp, panelWidth - 32f * Dp, 92f * Dp, 13f * Dp, s_secondary));
        var buttons = new List<DownloaderButtonSpec>
        {
            new("downloader-details-preview", _previewingSetIndex == index ? UiMaterialIcon.Pause : UiMaterialIcon.PlayArrow, string.Empty, 52f * Dp, UiAction.DownloaderDetailsPreview),
            new("downloader-details-download", UiMaterialIcon.Download, _localizer["BeatmapDownloader_Download"], 112f * Dp, UiAction.DownloaderDetailsDownload),
        };
        if (MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo)
        {
            buttons.Add(new DownloaderButtonSpec("downloader-details-no-video", UiMaterialIcon.Download, _localizer["BeatmapDownloader_DownloadNoVideo"], 174f * Dp, UiAction.DownloaderDetailsDownloadNoVideo));
        }

        AddButtonGroup(elements, buttons, x + panelWidth - 180f * Dp, bodyY + 138f * Dp);
    }

    private void AddDownloadOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        BeatmapDownloadState state = _downloadService.State;
        if (!state.IsActive)
        {
            return;
        }

        BeatmapDownloadProgress? progress = state.Progress;
        string text = progress?.Phase == BeatmapDownloadPhase.Importing
            ? _localizer.Format("BeatmapDownloader_Importing", state.Filename ?? _localizer["BeatmapDownloader_Beatmap"])
            : _localizer.Format("BeatmapDownloader_Downloading", state.Filename ?? _localizer["BeatmapDownloader_Beatmap"]);
        float textWidth = Math.Min(400f * Dp, EstimateTextWidth(text, 13f * Dp));
        float width = Math.Clamp(textWidth + 110f * Dp, 300f * Dp, Math.Min(500f * Dp, viewport.VirtualWidth - 80f * Dp));
        float titleHeight = 44f * Dp;
        float bodyHeight = 58f * Dp;
        float cancelHeight = 42f * Dp;
        float height = titleHeight + bodyHeight + cancelHeight;
        float x = (viewport.VirtualWidth - width) / 2f;
        float y = (viewport.VirtualHeight - height) / 2f;
        elements.Add(Fill("downloader-download-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), new UiColor(0, 0, 0, 128)));
        elements.Add(Fill("downloader-download-panel", new UiRect(x, y, width, height), s_appBar, 1f, UiAction.None, Radius));
        elements.Add(TextMiddle("downloader-download-title", _localizer["BeatmapDownloader_Download"], x, y, width, titleHeight, 14f * Dp, s_secondary, UiTextAlignment.Center));
        elements.Add(Fill("downloader-download-divider-title", new UiRect(x, y + titleHeight, width, 1f * Dp), s_background, 0.45f));

        float bodyY = y + titleHeight;
        float contentWidth = Math.Min(width - 32f * Dp, 42f * Dp + textWidth + 12f * Dp);
        float contentX = x + (width - contentWidth) / 2f;
        elements.Add(TextMiddle("downloader-download-spinner", "◌", contentX, bodyY, 34f * Dp, bodyHeight, 30f * Dp, s_accent, UiTextAlignment.Center));
        elements.Add(TextMiddle("downloader-download-text", text, contentX + 46f * Dp, bodyY, contentWidth - 46f * Dp, bodyHeight, 13f * Dp, s_white, UiTextAlignment.Left));
        elements.Add(Fill("downloader-download-divider-cancel", new UiRect(x, bodyY + bodyHeight, width, 1f * Dp), s_background, 0.45f));

        float cancelY = y + titleHeight + bodyHeight;
        elements.Add(Fill("downloader-download-cancel-hit", new UiRect(x, cancelY, width, cancelHeight), s_field, 1f, UiAction.DownloaderDownloadCancel, Radius));
        elements.Add(TextMiddle("downloader-download-cancel-text", _localizer["BeatmapDownloader_Cancel"], x, cancelY, width, cancelHeight, 14f * Dp, UiColor.Opaque(255, 191, 191), UiTextAlignment.Center, UiAction.DownloaderDownloadCancel));
    }
}
