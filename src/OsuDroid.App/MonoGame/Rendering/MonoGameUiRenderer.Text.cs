#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer
{
    private static XnaRect DrawClippedTextTexture(
        SpriteBatch spriteBatch,
        Texture2D texture,
        XnaRect bounds,
        UiTextStyle style,
        float scale
    )
    {
        var width = Math.Min(texture.Width, bounds.Width);
        var height = Math.Min(texture.Height, bounds.Height);
        var sourceX = style.AutoScroll is null
            ? style.Alignment switch
            {
                UiTextAlignment.Center => Math.Max(0, (texture.Width - width) / 2),
                UiTextAlignment.Right => Math.Max(0, texture.Width - width),
                _ => 0,
            }
            : CalculateAutoScrollSourceX(
                style.AutoScroll,
                Math.Max(0, texture.Width - width),
                scale
            );
        var sourceY =
            style.VerticalAlignment == UiTextVerticalAlignment.Middle
                ? Math.Max(0, (texture.Height - height) / 2)
                : 0;
        var x = style.Alignment switch
        {
            UiTextAlignment.Center when style.AutoScroll is null => bounds.X
                + (bounds.Width - width) / 2,
            UiTextAlignment.Right when style.AutoScroll is null => bounds.Right - width,
            _ => bounds.X,
        };
        var y =
            style.VerticalAlignment == UiTextVerticalAlignment.Middle
                ? bounds.Y + (bounds.Height - height) / 2
                : bounds.Y;
        var destination = new XnaRect(x, y, width, height);
        var source = new XnaRect(sourceX, sourceY, width, height);
        spriteBatch.Draw(texture, destination, source, XnaColor.White);
        return destination;
    }

    private static int CalculateAutoScrollSourceX(
        UiTextAutoScroll autoScroll,
        int maxScroll,
        float scale
    )
    {
        if (maxScroll <= 0)
            return 0;

        var speed = Math.Max(1f, autoScroll.Speed * scale);
        var timeout = Math.Max(0, autoScroll.TimeoutSeconds);
        var scrollDuration = maxScroll / speed;
        var cycleDuration = timeout + scrollDuration + timeout;
        var cycleSeconds = autoScroll.ElapsedSeconds % cycleDuration;
        if (cycleSeconds < timeout)
            return 0;

        var scrollSeconds = cycleSeconds - timeout;
        return scrollSeconds >= scrollDuration
            ? maxScroll
            : (int)Math.Min(maxScroll, scrollSeconds * speed);
    }

    private XnaRect ApplyMeasuredTextAnchor(
        UiFrameSnapshot frame,
        UiElementSnapshot element,
        XnaRect destination
    )
    {
        if (element.MeasuredTextAnchor is not { } anchor)
            return destination;

        var textTexture = textStore.GetTexture(
            anchor.Text,
            anchor.TextStyle,
            UiColor.Opaque(255, 255, 255),
            1f,
            frame.Viewport.Scale
        );
        var right = frame.Viewport.OffsetX + anchor.RightX * frame.Viewport.Scale;
        var leftPadding = anchor.LeftPadding * frame.Viewport.Scale;
        return destination with { X = (int)MathF.Round(right - textTexture.Width - leftPadding) };
    }

    private static XnaRect FitTextTexture(Texture2D texture, XnaRect bounds, UiTextStyle style)
    {
        var width = texture.Width;
        var height = texture.Height;
        if (texture.Width > bounds.Width || texture.Height > bounds.Height)
        {
            var scale = Math.Min(
                (float)bounds.Width / texture.Width,
                (float)bounds.Height / texture.Height
            );
            width = Math.Max(1, (int)MathF.Round(texture.Width * scale));
            height = Math.Max(1, (int)MathF.Round(texture.Height * scale));
        }

        var x = style.Alignment switch
        {
            UiTextAlignment.Center => bounds.X + (bounds.Width - width) / 2,
            UiTextAlignment.Right => bounds.Right - width,
            _ => bounds.X,
        };
        var y =
            style.VerticalAlignment == UiTextVerticalAlignment.Middle
                ? bounds.Y + (bounds.Height - height) / 2
                : bounds.Y;

        return new XnaRect(x, y, width, height);
    }
}
#endif
