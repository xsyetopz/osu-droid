using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private static void AddDifficultyDots(
        List<UiElementSnapshot> elements,
        string id,
        IReadOnlyList<BeatmapMirrorBeatmap> beatmaps,
        float x,
        float y,
        float width,
        int? selectedIndex,
        bool isCentered,
        UiAction fallbackAction
    )
    {
        const float cardCellWidth = 32f * Dp;
        const float detailsCellWidth = 56f * Dp;
        const float rowHeight = DifficultyGlyphRowHeight;
        const float margin = 6f * Dp;
        int count = Math.Min(beatmaps.Count, 16);
        if (count == 0)
        {
            return;
        }

        bool hasSelection = selectedIndex is not null;
        float gap = hasSelection ? margin : 0f;
        float preferredCellWidth = hasSelection ? detailsCellWidth : cardCellWidth;
        float availableWidth = Math.Max(0f, width - Math.Max(0, count - 1) * gap);
        float cellWidth = MathF.Min(preferredCellWidth, availableWidth / count);
        float glyphSize = MathF.Min(24f * Dp, MathF.Max(14f * Dp, cellWidth * 0.75f));
        float totalWidth = count * cellWidth + Math.Max(0, count - 1) * gap;
        float startX = isCentered ? x + (width - totalWidth) / 2f : x;
        float cursorX = startX;
        for (int i = 0; i < count; i++)
        {
            UiAction dotAction = hasSelection ? DifficultyAction(i) : fallbackAction;
            bool selected = selectedIndex == i;
            var hitBounds = new UiRect(cursorX, y, cellWidth, rowHeight);
            if (selected)
            {
                elements.Add(Fill($"{id}-{i}-selected", hitBounds, s_field, 1f, dotAction, Radius));
            }

            elements.Add(
                TextMiddle(
                    $"{id}-{i}",
                    "⦿",
                    hitBounds.X,
                    hitBounds.Y,
                    hitBounds.Width,
                    hitBounds.Height,
                    glyphSize,
                    StarRatingColor(beatmaps[i].StarRating),
                    UiTextAlignment.Center,
                    dotAction
                )
            );
            cursorX += cellWidth + gap;
        }
    }

    private static UiColor StarRatingColor(float starRating) =>
        OsuDroidColors.StarRating(starRating);
}
