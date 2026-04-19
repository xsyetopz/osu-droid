#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class MonoGameUiRenderer(GraphicsDevice graphicsDevice)
{
    private const int DiagnosticsBorderThickness = 6;
    private const int DiagnosticsMarkerSize = 36;

    private readonly MonoGameAssetStore assetStore = new(graphicsDevice);
    private readonly MonoGameIconStore iconStore = new(graphicsDevice);
    private readonly MonoGameShapeStore shapeStore = new(graphicsDevice);
    private readonly MonoGameTextStore textStore = new(graphicsDevice);
    private Texture2D? pixel;

    public void Draw(SpriteBatch spriteBatch, UiFrameSnapshot frame)
    {
        pixel ??= CreatePixel(graphicsDevice);

        foreach (var element in frame.Elements)
        {
            var bounds = frame.Viewport.ToSurface(element.Bounds);
            var destination = new XnaRect(
                (int)MathF.Round(bounds.X),
                (int)MathF.Round(bounds.Y),
                (int)MathF.Round(bounds.Width),
                (int)MathF.Round(bounds.Height));
            var color = ToXnaColor(element.Color, element.Alpha);

            if (element.Kind == UiElementKind.Fill)
            {
                DrawFill(spriteBatch, destination, color, element.CornerRadius * frame.Viewport.Scale, element.CornerMode);
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
                    var iconTexture = iconStore.GetIcon(element.MaterialIcon.Value, destination.Width, destination.Height, element.Color, element.Alpha);
                    spriteBatch.Draw(iconTexture, destination, XnaColor.White);
                }
                continue;
            }

            if (element.Kind == UiElementKind.Text)
            {
                if (element.Text is null || element.TextStyle is null)
                    continue;

                var texture = textStore.GetTexture(element.Text, element.TextStyle, element.Color, element.Alpha, frame.Viewport.Scale);
                var textDestination = FitTextTexture(texture, destination, element.TextStyle.Alignment);
                spriteBatch.Draw(texture, textDestination, XnaColor.White);
                if (element.TextStyle.Underline)
                    DrawUnderline(spriteBatch, textDestination, color);
                continue;
            }

            if (element.AssetName is null)
                continue;

            var textureAsset = assetStore.GetTexture(frame.AssetManifest, element.AssetName);
            if (textureAsset is not null)
                spriteBatch.Draw(textureAsset, destination, color);
        }
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

    private void DrawFill(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color, float radius, UiCornerMode cornerMode = UiCornerMode.All)
    {
        if (pixel is null || bounds.Width <= 0 || bounds.Height <= 0 || color.A == 0)
            return;

        if (radius <= 1f || cornerMode == UiCornerMode.None)
        {
            spriteBatch.Draw(pixel, bounds, color);
            return;
        }

        var texture = shapeStore.GetRoundedFill(bounds.Width, bounds.Height, radius, cornerMode, color);
        spriteBatch.Draw(texture, bounds, XnaColor.White);
    }

    private void DrawIcon(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color, UiIcon icon)
    {
        switch (icon)
        {
            case UiIcon.BackArrow:
                DrawLine(spriteBatch, bounds.Right - bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.15f, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, color, bounds.Width * 0.12f);
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.Right - bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.15f, color, bounds.Width * 0.12f);
                break;

            case UiIcon.Check:
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.18f, bounds.Y + bounds.Height * 0.52f, bounds.X + bounds.Width * 0.42f, bounds.Bottom - bounds.Height * 0.22f, color, bounds.Width * 0.11f);
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.42f, bounds.Bottom - bounds.Height * 0.22f, bounds.Right - bounds.Width * 0.14f, bounds.Y + bounds.Height * 0.2f, color, bounds.Width * 0.11f);
                break;

            case UiIcon.CheckboxChecked:
                DrawFill(spriteBatch, bounds, color, bounds.Width * 0.08f);
                DrawIcon(spriteBatch, Inset(bounds, bounds.Width * 0.12f), XnaColor.Black * 0.75f, UiIcon.Check);
                break;

            case UiIcon.CheckboxUnchecked:
                DrawBorder(spriteBatch, bounds, color, Math.Max(2, bounds.Width / 12));
                break;

            case UiIcon.ChevronRight:
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.35f, bounds.Y + bounds.Height * 0.2f, bounds.Right - bounds.Width * 0.28f, bounds.Y + bounds.Height * 0.5f, color, bounds.Width * 0.12f);
                DrawLine(spriteBatch, bounds.Right - bounds.Width * 0.28f, bounds.Y + bounds.Height * 0.5f, bounds.X + bounds.Width * 0.35f, bounds.Bottom - bounds.Height * 0.2f, color, bounds.Width * 0.12f);
                break;

            case UiIcon.ChevronDown:
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.18f, bounds.Y + bounds.Height * 0.35f, bounds.X + bounds.Width * 0.5f, bounds.Bottom - bounds.Height * 0.25f, color, bounds.Width * 0.12f);
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.5f, bounds.Bottom - bounds.Height * 0.25f, bounds.Right - bounds.Width * 0.18f, bounds.Y + bounds.Height * 0.35f, color, bounds.Width * 0.12f);
                break;

            case UiIcon.Grid:
                DrawGrid(spriteBatch, bounds, color);
                break;

            case UiIcon.Square:
                DrawBorder(spriteBatch, CenterSquare(bounds), color, Math.Max(2, bounds.Width / 10));
                break;

            case UiIcon.Display:
                DrawDisplay(spriteBatch, bounds, color);
                break;

            case UiIcon.Headphones:
                DrawHeadphones(spriteBatch, bounds, color);
                break;

            case UiIcon.MusicLibrary:
                DrawMusic(spriteBatch, bounds, color);
                break;

            case UiIcon.Input:
                DrawInput(spriteBatch, bounds, color);
                break;

            case UiIcon.Gear:
                DrawGear(spriteBatch, bounds, color);
                break;
        }
    }

    private void DrawGrid(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var cell = Math.Max(3, bounds.Width / 5);
        var gap = Math.Max(2, bounds.Width / 8);
        var startX = bounds.X + (bounds.Width - cell * 3 - gap * 2) / 2;
        var startY = bounds.Y + (bounds.Height - cell * 3 - gap * 2) / 2;
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            DrawFill(spriteBatch, new XnaRect(startX + x * (cell + gap), startY + y * (cell + gap), cell, cell), color, 0f);
    }

    private void DrawDisplay(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var screen = new XnaRect(bounds.X + bounds.Width / 8, bounds.Y + bounds.Height / 6, bounds.Width * 3 / 4, bounds.Height / 2);
        DrawBorder(spriteBatch, screen, color, Math.Max(2, bounds.Width / 12));
        DrawFill(spriteBatch, new XnaRect(bounds.X + bounds.Width * 2 / 5, screen.Bottom, bounds.Width / 5, bounds.Height / 6), color, 0f);
        DrawFill(spriteBatch, new XnaRect(bounds.X + bounds.Width / 3, bounds.Bottom - bounds.Height / 8, bounds.Width / 3, Math.Max(2, bounds.Height / 10)), color, 0f);
    }

    private void DrawHeadphones(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var thickness = Math.Max(2, bounds.Width / 10);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.X + bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.18f, color, thickness);
        DrawLine(spriteBatch, bounds.Right - bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.Right - bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.18f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.22f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.22f, bounds.Right - bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, color, thickness);
    }

    private void DrawMusic(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var thickness = Math.Max(2, bounds.Width / 10);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.62f, bounds.Y + bounds.Height * 0.18f, bounds.X + bounds.Width * 0.62f, bounds.Bottom - bounds.Height * 0.28f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.62f, bounds.Y + bounds.Height * 0.18f, bounds.Right - bounds.Width * 0.2f, bounds.Y + bounds.Height * 0.26f, color, thickness);
        DrawCircle(spriteBatch, bounds.X + bounds.Width * 0.45f, bounds.Bottom - bounds.Height * 0.25f, Math.Max(4, bounds.Width / 6), color);
    }

    private void DrawInput(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var thickness = Math.Max(2, bounds.Width / 12);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.15f, bounds.Bottom - bounds.Height * 0.25f, bounds.Right - bounds.Width * 0.15f, bounds.Bottom - bounds.Height * 0.25f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.25f, bounds.X + bounds.Width * 0.35f, bounds.Y + bounds.Height * 0.3f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.35f, bounds.Y + bounds.Height * 0.3f, bounds.Right - bounds.Width * 0.2f, bounds.Y + bounds.Height * 0.3f, color, thickness);
    }

    private void DrawGear(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        DrawBorder(spriteBatch, Inset(bounds, bounds.Width * 0.2f), color, Math.Max(2, bounds.Width / 10));
        var center = CenterSquare(Inset(bounds, bounds.Width * 0.35f));
        DrawFill(spriteBatch, center, color, center.Width / 2f);
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

    private static XnaRect FitTextTexture(Texture2D texture, XnaRect bounds, UiTextAlignment alignment)
    {
        var width = texture.Width;
        var height = texture.Height;
        if (texture.Width > bounds.Width || texture.Height > bounds.Height)
        {
            var scale = Math.Min((float)bounds.Width / texture.Width, (float)bounds.Height / texture.Height);
            width = Math.Max(1, (int)MathF.Round(texture.Width * scale));
            height = Math.Max(1, (int)MathF.Round(texture.Height * scale));
        }

        var x = alignment switch
        {
            UiTextAlignment.Center => bounds.X + (bounds.Width - width) / 2,
            UiTextAlignment.Right => bounds.Right - width,
            _ => bounds.X,
        };

        return new XnaRect(x, bounds.Y, width, height);
    }

    private static XnaColor ToXnaColor(UiColor color, float alpha)
    {
        var alphaByte = (byte)Math.Clamp((int)MathF.Round(color.Alpha * alpha), 0, 255);
        return new XnaColor(color.Red, color.Green, color.Blue, alphaByte);
    }
}
#endif
