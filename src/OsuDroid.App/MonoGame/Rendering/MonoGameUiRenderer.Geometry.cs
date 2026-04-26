#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer
{
    private static XnaRect ToSurfaceRect(UiFrameSnapshot frame, UiElementSnapshot element)
    {
        var bounds = frame.Viewport.ToSurface(element.Bounds);
        return new XnaRect(
            (int)MathF.Round(bounds.X),
            (int)MathF.Round(bounds.Y),
            (int)MathF.Round(bounds.Width),
            (int)MathF.Round(bounds.Height)
        );
    }

    private static XnaRect? ToSurfaceClipRect(UiFrameSnapshot frame, UiElementSnapshot element)
    {
        if (element.ClipBounds is null)
            return null;

        var bounds = frame.Viewport.ToSurface(element.ClipBounds.Value);
        return new XnaRect(
            (int)MathF.Round(bounds.X),
            (int)MathF.Round(bounds.Y),
            Math.Max(0, (int)MathF.Round(bounds.Width)),
            Math.Max(0, (int)MathF.Round(bounds.Height))
        );
    }

    private static XnaRect Intersect(XnaRect a, XnaRect b)
    {
        var x = Math.Max(a.X, b.X);
        var y = Math.Max(a.Y, b.Y);
        var right = Math.Min(a.Right, b.Right);
        var bottom = Math.Min(a.Bottom, b.Bottom);
        return new XnaRect(x, y, Math.Max(0, right - x), Math.Max(0, bottom - y));
    }

    private static XnaColor ToXnaColor(UiColor color, float alpha)
    {
        var alphaByte = (byte)Math.Clamp((int)MathF.Round(color.Alpha * alpha), 0, 255);
        return new XnaColor(color.Red, color.Green, color.Blue, alphaByte);
    }
}
#endif
