using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.UI.Scrolling;

public static class DroidScrollIndicator
{
    public const float Thickness = 6f;
    public const float Radius = Thickness / 2f;
    public const float Alpha = 0.5f;

    public static UiElementSnapshot? Horizontal(
        string id,
        UiRect viewportBounds,
        float scrollOffset,
        float maxScrollOffset,
        UiColor color)
    {
        if (maxScrollOffset <= 0f || viewportBounds.Width <= 0f)
        {
            return null;
        }

        float contentWidth = viewportBounds.Width + maxScrollOffset;
        float width = viewportBounds.Width * Math.Clamp(viewportBounds.Width / contentWidth, 0f, 1f);
        float x = viewportBounds.X + (viewportBounds.Width - width) * Math.Clamp(scrollOffset / maxScrollOffset, 0f, 1f);
        return UiElementFactory.Fill(
            id,
            new UiRect(x, viewportBounds.Bottom - Thickness, width, Thickness),
            color,
            Alpha,
            cornerRadius: Radius) with
        {
            ClipBounds = viewportBounds,
        };
    }

    public static UiElementSnapshot? Vertical(
        string id,
        UiRect viewportBounds,
        float scrollOffset,
        float maxScrollOffset,
        UiColor color)
    {
        if (maxScrollOffset <= 0f || viewportBounds.Height <= 0f)
        {
            return null;
        }

        float contentHeight = viewportBounds.Height + maxScrollOffset;
        float height = viewportBounds.Height * Math.Clamp(viewportBounds.Height / contentHeight, 0f, 1f);
        float y = viewportBounds.Y + (viewportBounds.Height - height) * Math.Clamp(scrollOffset / maxScrollOffset, 0f, 1f);
        return UiElementFactory.Fill(
            id,
            new UiRect(viewportBounds.Right - Thickness, y, Thickness, height),
            color,
            Alpha,
            cornerRadius: Radius) with
        {
            ClipBounds = viewportBounds,
        };
    }
}
