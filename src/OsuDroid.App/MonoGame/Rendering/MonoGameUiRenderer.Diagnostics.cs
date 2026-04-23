#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer
{
    private void WarmUpElement(UiFrameSnapshot frame, UiElementSnapshot element, RenderCacheMetrics metrics)
    {
        if (element.Alpha <= 0f)
            return;

        var destination = ToSurfaceRect(frame, element);
        if (destination.Width <= 0 || destination.Height <= 0)
            return;

        if (element.MeasuredTextAnchor is { } anchor)
            _ = textStore.GetTexture(anchor.Text, anchor.TextStyle, UiColor.Opaque(255, 255, 255), 1f, frame.Viewport.Scale, metrics);

        switch (element.Kind)
        {
            case UiElementKind.Fill:
                if (element.CornerRadius > 1f && element.CornerMode != UiCornerMode.None)
                    _ = shapeStore.GetRoundedFill(destination.Width, destination.Height, element.CornerRadius * frame.Viewport.Scale, element.CornerMode, ToXnaColor(element.Color, element.Alpha), metrics);
                break;

            case UiElementKind.MaterialIcon:
                if (element.MaterialIcon is not null)
                    _ = iconStore.GetIcon(element.MaterialIcon.Value, destination.Width, destination.Height, element.Color, element.Alpha, metrics);
                break;

            case UiElementKind.Text:
                if (element.Text is not null && element.TextStyle is not null)
                    _ = textStore.GetTexture(element.Text, element.TextStyle, element.Color, element.Alpha, frame.Viewport.Scale, metrics);
                break;

            case UiElementKind.Sprite:
                if (element.ExternalAssetPath is not null)
                    assetStore.RequestExternalTexture(element.ExternalAssetPath, destination.Width, destination.Height, metrics);
                else if (element.AssetName is not null)
                    _ = assetStore.GetTexture(frame.AssetManifest, element.AssetName, metrics);
                break;
        }
    }

    private void DrawFill(
        SpriteBatch spriteBatch,
        XnaRect bounds,
        XnaColor color,
        float radius,
        UiCornerMode cornerMode = UiCornerMode.All,
        RenderCacheMetrics? metrics = null,
        float rotationDegrees = 0f,
        float rotationOriginX = 0.5f,
        float rotationOriginY = 0.5f)
    {
        if (pixel is null || bounds.Width <= 0 || bounds.Height <= 0 || color.A == 0)
            return;

        if (Math.Abs(rotationDegrees) > 0.001f)
        {
            var position = new Vector2(
                bounds.X + bounds.Width * rotationOriginX,
                bounds.Y + bounds.Height * rotationOriginY);
            var origin = new Vector2(rotationOriginX, rotationOriginY);
            var scale = new Vector2(bounds.Width, bounds.Height);
            spriteBatch.Draw(pixel, position, null, color, MathHelper.ToRadians(rotationDegrees), origin, scale, SpriteEffects.None, 0f);
            return;
        }

        if (radius <= 1f || cornerMode == UiCornerMode.None)
        {
            spriteBatch.Draw(pixel, bounds, color);
            return;
        }

        var texture = shapeStore.GetRoundedFill(bounds.Width, bounds.Height, radius, cornerMode, color, metrics);
        spriteBatch.Draw(texture, bounds, XnaColor.White);
    }

    private void DrawLine(SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, XnaColor color, float thickness)
    {
        if (pixel is null)
            return;

        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = MathF.Sqrt(dx * dx + dy * dy);
        if (length <= 0f)
            return;

        var rotation = MathF.Atan2(dy, dx);
        spriteBatch.Draw(pixel, new Vector2(x1, y1), null, color, rotation, new Vector2(0f, 0.5f), new Vector2(length, Math.Max(1f, thickness)), SpriteEffects.None, 0f);
    }

    private void DrawCircle(SpriteBatch spriteBatch, float centerX, float centerY, int radius, XnaColor color)
    {
        if (pixel is null || radius <= 0)
            return;

        for (var y = -radius; y <= radius; y++)
        {
            var width = (int)MathF.Sqrt(radius * radius - y * y) * 2;
            spriteBatch.Draw(pixel, new XnaRect((int)MathF.Round(centerX - width / 2f), (int)MathF.Round(centerY + y), width, 1), color);
        }
    }

    private static XnaRect Inset(XnaRect bounds, float amount)
    {
        var inset = (int)MathF.Round(amount);
        return new XnaRect(bounds.X + inset, bounds.Y + inset, Math.Max(1, bounds.Width - inset * 2), Math.Max(1, bounds.Height - inset * 2));
    }

    private static XnaRect CenterSquare(XnaRect bounds)
    {
        var size = Math.Min(bounds.Width, bounds.Height);
        return new XnaRect(bounds.X + (bounds.Width - size) / 2, bounds.Y + (bounds.Height - size) / 2, size, size);
    }

    private void DrawBorder(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color, int thickness)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 || pixel is null)
            return;

        var horizontalThickness = Math.Min(thickness, bounds.Height);
        var verticalThickness = Math.Min(thickness, bounds.Width);
        spriteBatch.Draw(pixel, new XnaRect(bounds.X, bounds.Y, bounds.Width, horizontalThickness), color);
        spriteBatch.Draw(pixel, new XnaRect(bounds.X, bounds.Bottom - horizontalThickness, bounds.Width, horizontalThickness), color);
        spriteBatch.Draw(pixel, new XnaRect(bounds.X, bounds.Y, verticalThickness, bounds.Height), color);
        spriteBatch.Draw(pixel, new XnaRect(bounds.Right - verticalThickness, bounds.Y, verticalThickness, bounds.Height), color);
    }

    private void DrawCornerMarkers(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        if (pixel is null)
            return;

        var markerSize = Math.Min(DiagnosticsMarkerSize, Math.Min(bounds.Width, bounds.Height));
        if (markerSize <= 0)
            return;

        spriteBatch.Draw(pixel, new XnaRect(bounds.Left, bounds.Top, markerSize, markerSize), color);
        spriteBatch.Draw(pixel, new XnaRect(bounds.Right - markerSize, bounds.Top, markerSize, markerSize), color);
        spriteBatch.Draw(pixel, new XnaRect(bounds.Left, bounds.Bottom - markerSize, markerSize, markerSize), color);
        spriteBatch.Draw(pixel, new XnaRect(bounds.Right - markerSize, bounds.Bottom - markerSize, markerSize, markerSize), color);
    }

    private static XnaRect ClampToViewport(XnaRect bounds, XnaRect viewportBounds)
    {
        var left = Math.Clamp(bounds.Left, viewportBounds.Left, viewportBounds.Right);
        var top = Math.Clamp(bounds.Top, viewportBounds.Top, viewportBounds.Bottom);
        var right = Math.Clamp(bounds.Right, viewportBounds.Left, viewportBounds.Right);
        var bottom = Math.Clamp(bounds.Bottom, viewportBounds.Top, viewportBounds.Bottom);
        return new XnaRect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
    }

    private void DrawUnderline(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        if (pixel is null || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var thickness = Math.Max(1, bounds.Height / 16);
        spriteBatch.Draw(pixel, new XnaRect(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
    }
}
#endif
