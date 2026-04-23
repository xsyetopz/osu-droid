using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private void AddDetails(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (selectedSetIndex is not int index || index < 0 || index >= sets.Count)
            return;

        var set = sets[index];
        var beatmaps = set.Beatmaps;
        if (beatmaps.Count == 0)
            return;

        selectedDifficultyIndex = Math.Clamp(selectedDifficultyIndex, 0, beatmaps.Count - 1);
        var beatmap = beatmaps[selectedDifficultyIndex];
        elements.Add(Fill("downloader-details-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), ModalShade, 1f, UiAction.DownloaderDetailsClose));
        var panelWidth = Math.Min(500f * Dp, viewport.VirtualWidth - 80f * Dp);
        var x = (viewport.VirtualWidth - panelWidth) / 2f;
        var y = Math.Max(20f * Dp, (viewport.VirtualHeight - 348f * Dp) / 2f);
        elements.Add(Fill("downloader-details-panel", new UiRect(x, y, panelWidth, 348f * Dp), Panel, 1f, UiAction.DownloaderDetailsPanel, Radius));
        AddCover(elements, "downloader-details-cover", set, new UiRect(x, y, panelWidth, 100f * Dp), UiAction.DownloaderDetailsPanel);
        elements.Add(Fill("downloader-details-cover-dim", new UiRect(x, y, panelWidth, 100f * Dp), CoverFallback, 0.2f, UiAction.DownloaderDetailsPanel, Radius));
        elements.Add(Text("downloader-details-title", DisplayTitle(set), x + 16f * Dp, y + 18f * Dp, panelWidth - 120f * Dp, 26f * Dp, 17f * Dp, White));
        elements.Add(Text("downloader-details-artist", DisplayArtist(set), x + 16f * Dp, y + 44f * Dp, panelWidth - 120f * Dp, 22f * Dp, 14f * Dp, Secondary));
        elements.Add(Text("downloader-details-creator", $"Mapped by {set.Creator}", x + 16f * Dp, y + 68f * Dp, panelWidth - 120f * Dp, 20f * Dp, 13f * Dp, Muted));
        AddStatusPill(elements, "downloader-details-status", set.Status, x + panelWidth - StatusPillWidth(set.Status) - 16f * Dp, y + 16f * Dp, UiAction.DownloaderDetailsPanel);
        AddDifficultyDots(elements, "downloader-details-diff", beatmaps, x + 12f * Dp, y + 108f * Dp, panelWidth - 24f * Dp, UiAction.None, selectedDifficultyIndex, true, UiAction.None);
        var bodyY = y + 146f * Dp;
        elements.Add(Fill("downloader-details-body", new UiRect(x, bodyY, panelWidth, 202f * Dp), Footer, 1f, UiAction.DownloaderDetailsPanel));
        elements.Add(Text("downloader-details-version", beatmap.Version, x + 16f * Dp, bodyY + 12f * Dp, panelWidth - 32f * Dp, 24f * Dp, 15f * Dp, White));
        elements.Add(Text("downloader-details-stats", $"Star rating: {beatmap.StarRating:0.##}\nAR: {beatmap.ApproachRate:0.##} - OD: {beatmap.OverallDifficulty:0.##} - CS: {beatmap.CircleSize:0.##} - HP: {beatmap.HpDrainRate:0.##}\nCircles: {beatmap.CircleCount} - Sliders: {beatmap.SliderCount} - Spinners: {beatmap.SpinnerCount}\nLength: {TimeSpan.FromSeconds(beatmap.HitLength):m\\:ss} - BPM: {beatmap.Bpm:0.##}", x + 16f * Dp, bodyY + 38f * Dp, panelWidth - 32f * Dp, 92f * Dp, 13f * Dp, Secondary));
        var buttons = new List<DownloaderButtonSpec>
        {
            new("downloader-details-preview", previewingSetIndex == index ? UiMaterialIcon.Pause : UiMaterialIcon.PlayArrow, string.Empty, 52f * Dp, UiAction.DownloaderDetailsPreview),
            new("downloader-details-download", UiMaterialIcon.Download, "Download", 112f * Dp, UiAction.DownloaderDetailsDownload),
        };
        if (MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo)
            buttons.Add(new DownloaderButtonSpec("downloader-details-no-video", UiMaterialIcon.Download, "No video", 128f * Dp, UiAction.DownloaderDetailsDownloadNoVideo));
        AddButtonGroup(elements, buttons, x + panelWidth - 180f * Dp, bodyY + 138f * Dp);
    }

    private void AddDownloadOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var state = downloadService.State;
        if (!state.IsActive)
            return;

        var progress = state.Progress;
        var text = progress?.Phase == BeatmapDownloadPhase.Importing
            ? $"Importing {state.Filename}"
            : $"Downloading {state.Filename ?? "beatmap"}{FormatDownloadInfo(progress)}";
        var textWidth = Math.Min(400f * Dp, EstimateTextWidth(text, 13f * Dp));
        var width = Math.Clamp(textWidth + 110f * Dp, 300f * Dp, Math.Min(500f * Dp, viewport.VirtualWidth - 80f * Dp));
        var titleHeight = 44f * Dp;
        var bodyHeight = 58f * Dp;
        var cancelHeight = 42f * Dp;
        var height = titleHeight + bodyHeight + cancelHeight;
        var x = (viewport.VirtualWidth - width) / 2f;
        var y = (viewport.VirtualHeight - height) / 2f;
        elements.Add(Fill("downloader-download-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), new UiColor(0, 0, 0, 128)));
        elements.Add(Fill("downloader-download-panel", new UiRect(x, y, width, height), AppBar, 1f, UiAction.None, Radius));
        elements.Add(TextMiddle("downloader-download-title", "Download", x, y, width, titleHeight, 14f * Dp, Secondary, UiTextAlignment.Center));
        elements.Add(Fill("downloader-download-divider-title", new UiRect(x, y + titleHeight, width, 1f * Dp), Background, 0.45f));

        var bodyY = y + titleHeight;
        var contentWidth = Math.Min(width - 32f * Dp, 42f * Dp + textWidth + 12f * Dp);
        var contentX = x + (width - contentWidth) / 2f;
        elements.Add(TextMiddle("downloader-download-spinner", "◌", contentX, bodyY, 34f * Dp, bodyHeight, 30f * Dp, Accent, UiTextAlignment.Center));
        elements.Add(TextMiddle("downloader-download-text", text, contentX + 46f * Dp, bodyY, contentWidth - 46f * Dp, bodyHeight, 13f * Dp, White, UiTextAlignment.Left));
        elements.Add(Fill("downloader-download-divider-cancel", new UiRect(x, bodyY + bodyHeight, width, 1f * Dp), Background, 0.45f));

        var cancelY = y + titleHeight + bodyHeight;
        elements.Add(Fill("downloader-download-cancel-hit", new UiRect(x, cancelY, width, cancelHeight), Field, 1f, UiAction.DownloaderDownloadCancel, Radius));
        elements.Add(TextMiddle("downloader-download-cancel-text", "× Cancel", x, cancelY, width, cancelHeight, 14f * Dp, UiColor.Opaque(255, 191, 191), UiTextAlignment.Center, UiAction.DownloaderDownloadCancel));
    }
}
