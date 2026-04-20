#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer(GraphicsDevice graphicsDevice)
{
    private const int DiagnosticsBorderThickness = 6;
    private const int DiagnosticsMarkerSize = 36;

    private readonly MonoGameAssetStore assetStore = new(graphicsDevice);
    private readonly MonoGameIconStore iconStore = new(graphicsDevice);
    private readonly MonoGameShapeStore shapeStore = new(graphicsDevice);
    private readonly MonoGameTextStore textStore = new(graphicsDevice);
    private Texture2D? pixel;

    public void Draw(SpriteBatch spriteBatch, UiFrameSnapshot frame, RenderCacheMetrics? metrics = null)
    {
        pixel ??= CreatePixel(graphicsDevice);

        foreach (var element in frame.Elements)
        {
            var destination = ToSurfaceRect(frame, element);
            var color = ToXnaColor(element.Color, element.Alpha);

            if (element.Kind == UiElementKind.Fill)
            {
                DrawFill(spriteBatch, destination, color, element.CornerRadius * frame.Viewport.Scale, element.CornerMode, metrics);
                continue;
            }

            if (element.Kind == UiElementKind.Icon)
            {
                if (element.Icon is not null)
                    DrawIcon(spriteBatch, destination, color, element.Icon.Value);
                continue;
            }

            if (element.Kind == UiElementKind.MaterialIcon)
            {
                if (element.MaterialIcon is not null)
                {
                    var iconTexture = iconStore.GetIcon(element.MaterialIcon.Value, destination.Width, destination.Height, element.Color, element.Alpha, metrics);
                    spriteBatch.Draw(iconTexture, destination, XnaColor.White);
                }
                continue;
            }

            if (element.Kind == UiElementKind.Text)
            {
                if (element.Text is null || element.TextStyle is null)
                    continue;

                var texture = textStore.GetTexture(element.Text, element.TextStyle, element.Color, element.Alpha, frame.Viewport.Scale, metrics);
                var textDestination = FitTextTexture(texture, destination, element.TextStyle);
                spriteBatch.Draw(texture, textDestination, XnaColor.White);
                if (element.TextStyle.Underline)
                    DrawUnderline(spriteBatch, textDestination, color);
                continue;
            }

            var textureAsset = element.ExternalAssetPath is not null
                ? assetStore.GetFileTexture(element.ExternalAssetPath, metrics)
                : element.AssetName is null ? null : assetStore.GetTexture(frame.AssetManifest, element.AssetName, metrics);
            if (textureAsset is not null)
                DrawSprite(spriteBatch, textureAsset, destination, color, element.RotationDegrees, element.SpriteFit);
        }
    }

    public int WarmUp(UiFrameSnapshot frame, int startElementIndex, DateTime deadline, RenderCacheMetrics metrics)
    {
        pixel ??= CreatePixel(graphicsDevice);
        var elementIndex = Math.Max(0, startElementIndex);
        var processedElements = 0;

        while (elementIndex < frame.Elements.Count)
        {
            WarmUpElement(frame, frame.Elements[elementIndex], metrics);
            metrics.AddWarmupElement();
            elementIndex++;
            processedElements++;

            if (processedElements > 0 && DateTime.UtcNow >= deadline)
                break;
        }

        return elementIndex;
    }

    private static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, XnaRect destination, XnaColor color, float rotationDegrees, UiSpriteFit fit)
    {
        var source = fit == UiSpriteFit.Stretch ? null : CalculateSourceRect(texture, destination, fit);

        if (Math.Abs(rotationDegrees) < 0.001f)
        {
            spriteBatch.Draw(texture, destination, source, color);
            return;
        }

        var position = new Vector2(destination.X + destination.Width / 2f, destination.Y + destination.Height / 2f);
        var origin = new Vector2(source?.Width / 2f ?? texture.Width / 2f, source?.Height / 2f ?? texture.Height / 2f);
        var sourceWidth = source?.Width ?? texture.Width;
        var sourceHeight = source?.Height ?? texture.Height;
        var scale = new Vector2(destination.Width / (float)sourceWidth, destination.Height / (float)sourceHeight);
        spriteBatch.Draw(texture, position, source, color, MathHelper.ToRadians(rotationDegrees), origin, scale, SpriteEffects.None, 0f);
    }

    private static XnaRect? CalculateSourceRect(Texture2D texture, XnaRect destination, UiSpriteFit fit)
    {
        var textureRatio = texture.Width / (float)texture.Height;
        var destinationRatio = destination.Width / (float)destination.Height;

        if (fit == UiSpriteFit.Contain)
            return null;

        if (destinationRatio > textureRatio)
        {
            var sourceHeight = Math.Max(1, (int)MathF.Round(texture.Width / destinationRatio));
            return new XnaRect(0, (texture.Height - sourceHeight) / 2, texture.Width, sourceHeight);
        }

        var sourceWidth = Math.Max(1, (int)MathF.Round(texture.Height * destinationRatio));
        return new XnaRect((texture.Width - sourceWidth) / 2, 0, sourceWidth, texture.Height);
    }

    public void DrawDiagnostics(SpriteBatch spriteBatch, RenderBoundsDiagnostics diagnostics)
    {
        pixel ??= CreatePixel(graphicsDevice);

        var viewportBounds = diagnostics.ViewportBounds;
        DrawBorder(spriteBatch, viewportBounds, XnaColor.Red, DiagnosticsBorderThickness);
        DrawBorder(spriteBatch, ClampToViewport(diagnostics.ClientBounds, viewportBounds), XnaColor.Lime, DiagnosticsBorderThickness);
        DrawCornerMarkers(spriteBatch, viewportBounds, XnaColor.Blue);
    }

    public void Dispose()
    {
        pixel?.Dispose();
        assetStore.Dispose();
        iconStore.Dispose();
        shapeStore.Dispose();
        textStore.Dispose();
    }

    private static Texture2D CreatePixel(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData([XnaColor.White]);
        return texture;
    }

    private void WarmUpElement(UiFrameSnapshot frame, UiElementSnapshot element, RenderCacheMetrics metrics)
    {
        if (element.Alpha <= 0f)
            return;

        var destination = ToSurfaceRect(frame, element);
        if (destination.Width <= 0 || destination.Height <= 0)
            return;

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
                    _ = assetStore.GetFileTexture(element.ExternalAssetPath, metrics);
                else if (element.AssetName is not null)
                    _ = assetStore.GetTexture(frame.AssetManifest, element.AssetName, metrics);
                break;
        }
    }

    private void DrawFill(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color, float radius, UiCornerMode cornerMode = UiCornerMode.All, RenderCacheMetrics? metrics = null)
    {
        if (pixel is null || bounds.Width <= 0 || bounds.Height <= 0 || color.A == 0)
            return;

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

    private static XnaRect FitTextTexture(Texture2D texture, XnaRect bounds, UiTextStyle style)
    {
        var width = texture.Width;
        var height = texture.Height;
        if (texture.Width > bounds.Width || texture.Height > bounds.Height)
        {
            var scale = Math.Min((float)bounds.Width / texture.Width, (float)bounds.Height / texture.Height);
            width = Math.Max(1, (int)MathF.Round(texture.Width * scale));
            height = Math.Max(1, (int)MathF.Round(texture.Height * scale));
        }

        var x = style.Alignment switch
        {
            UiTextAlignment.Center => bounds.X + (bounds.Width - width) / 2,
            UiTextAlignment.Right => bounds.Right - width,
            _ => bounds.X,
        };
        var y = style.VerticalAlignment == UiTextVerticalAlignment.Middle
            ? bounds.Y + (bounds.Height - height) / 2
            : bounds.Y;

        return new XnaRect(x, y, width, height);
    }

    private static XnaRect ToSurfaceRect(UiFrameSnapshot frame, UiElementSnapshot element)
    {
        var bounds = frame.Viewport.ToSurface(element.Bounds);
        return new XnaRect(
            (int)MathF.Round(bounds.X),
            (int)MathF.Round(bounds.Y),
            (int)MathF.Round(bounds.Width),
            (int)MathF.Round(bounds.Height));
    }

    private static XnaColor ToXnaColor(UiColor color, float alpha)
    {
        var alphaByte = (byte)Math.Clamp((int)MathF.Round(color.Alpha * alpha), 0, 255);
        return new XnaColor(color.Red, color.Green, color.Blue, alphaByte);
    }
}
#endif
