using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private float MaxScrollOffset(VirtualViewport viewport)
    {
        var columns = Math.Max(1, (int)MathF.Floor(viewport.VirtualWidth / (CardWidth + CardMargin * 2f)));
        var rows = (int)MathF.Ceiling(sets.Count / (float)columns);
        var contentHeight = rows * (CardHeight + CardMargin * 2f) + 32f * Dp;
        return Math.Max(0f, contentHeight - (viewport.VirtualHeight - BarHeight));
    }

    private BeatmapMirrorDefinition MirrorDefinition(BeatmapMirrorKind kind) => mirrorClient.Mirrors.First(m => m.Kind == kind);

    private static UiRect SearchBounds(VirtualViewport viewport)
    {
        var left = DroidUiMetrics.AppBarHeight + 6f * Dp;
        var right = viewport.VirtualWidth - 12f * Dp;
        var mirrorWidth = 150f * Dp;
        var filtersWidth = 112f * Dp;
        var searchTrailingWidth = 0f;
        var searchRight = right - mirrorWidth - filtersWidth - searchTrailingWidth - 18f * Dp;
        return new UiRect(left, 10f * Dp, Math.Max(200f * Dp, searchRight - left), 36f * Dp);
    }

    private static float MaxDropdownScroll(int optionCount, VirtualViewport viewport)
    {
        var rowHeight = 46f * Dp;
        var contentHeight = optionCount * rowHeight + 16f * Dp;
        var availableHeight = Math.Max(rowHeight, viewport.VirtualHeight - (BarHeight + 54f * Dp));
        return Math.Max(0f, contentHeight - availableHeight);
    }
}
