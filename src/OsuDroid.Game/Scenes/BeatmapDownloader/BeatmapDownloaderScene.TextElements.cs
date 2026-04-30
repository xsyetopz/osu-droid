using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private static float EstimateTextWidth(string text, float size) =>
        MathF.Max(1f, text.Length * size * 0.55f);

    private static UiElementSnapshot Fill(
        string id,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        float radius = 0f
    ) => UiElementFactory.Fill(id, bounds, color, alpha, action, radius);

    private static UiElementSnapshot Text(
        string id,
        string text,
        float x,
        float y,
        float width,
        float height,
        float size,
        UiColor color,
        UiTextAlignment alignment = UiTextAlignment.Left,
        UiAction action = UiAction.None
    ) =>
        UiElementFactory.Text(
            id,
            text,
            new UiRect(x, y, width, height),
            size,
            color,
            action,
            alignment: alignment
        );

    private static UiElementSnapshot TextMiddle(
        string id,
        string text,
        float x,
        float y,
        float width,
        float height,
        float size,
        UiColor color,
        UiTextAlignment alignment = UiTextAlignment.Left,
        UiAction action = UiAction.None
    ) =>
        UiElementFactory.Text(
            id,
            text,
            new UiRect(x, y, width, height),
            size,
            color,
            action,
            alignment: alignment,
            verticalAlignment: UiTextVerticalAlignment.Middle
        );

    private static UiElementSnapshot MaterialIcon(
        string id,
        UiMaterialIcon icon,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None
    ) => UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action);

    private static UiElementSnapshot ProgressRing(
        string id,
        UiRect bounds,
        UiColor color,
        float strokeWidth,
        float sweepDegrees,
        float rotationDegrees = 0f
    ) =>
        UiElementFactory.ProgressRing(
            id,
            bounds,
            color,
            strokeWidth,
            sweepDegrees,
            rotationDegrees
        );
}
