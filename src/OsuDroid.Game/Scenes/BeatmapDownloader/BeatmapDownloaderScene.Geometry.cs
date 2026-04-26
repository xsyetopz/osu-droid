using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private float MaxScrollOffset(VirtualViewport viewport)
    {
        int columns = Math.Max(
            1,
            (int)MathF.Floor(viewport.VirtualWidth / (CardWidth + CardMargin * 2f))
        );
        int rows = (int)MathF.Ceiling(_sets.Count / (float)columns);
        float contentHeight = rows * (CardHeight + CardMargin * 2f) + 32f * Dp;
        return Math.Max(0f, contentHeight - (viewport.VirtualHeight - BarHeight));
    }

    private BeatmapMirrorDefinition MirrorDefinition(BeatmapMirrorKind kind) =>
        _mirrorClient.Mirrors.First(m => m.Kind == kind);

    private static string MirrorLogoAsset(BeatmapMirrorKind kind) =>
        kind switch
        {
            BeatmapMirrorKind.OsuDirect => DroidAssets.BeatmapDownloaderOsuDirect,
            BeatmapMirrorKind.Catboy => DroidAssets.BeatmapDownloaderCatboy,
            _ => DroidAssets.BeatmapDownloaderOsuDirect,
        };

    private static UiRect SearchBounds(VirtualViewport viewport, bool isSearching = false)
    {
        float left = DroidUiMetrics.AppBarHeight + 6f * Dp + (isSearching ? 40f * Dp : 0f);
        float right = viewport.VirtualWidth - 12f * Dp;
        float mirrorWidth = 150f * Dp;
        float filtersWidth = 112f * Dp;
        float searchTrailingWidth = 0f;
        float searchRight = right - mirrorWidth - filtersWidth - searchTrailingWidth - 18f * Dp;
        return new UiRect(left, 10f * Dp, Math.Max(200f * Dp, searchRight - left), 36f * Dp);
    }

    private static float MaxDropdownScroll(int optionCount, VirtualViewport viewport)
    {
        float rowHeight = 46f * Dp;
        float contentHeight = optionCount * rowHeight + 16f * Dp;
        float availableHeight = Math.Max(
            rowHeight,
            viewport.VirtualHeight - (BarHeight + 54f * Dp)
        );
        return Math.Max(0f, contentHeight - availableHeight);
    }
}
