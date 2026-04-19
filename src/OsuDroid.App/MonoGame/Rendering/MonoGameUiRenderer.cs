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
                spriteBatch.Draw(pixel, destination, color);
                continue;
            }

            if (element.AssetName is null)
                continue;

            var texture = assetStore.GetTexture(frame.AssetManifest, element.AssetName);
            if (texture is not null)
                spriteBatch.Draw(texture, destination, color);
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
    }

    private static Texture2D CreatePixel(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData([XnaColor.White]);
        return texture;
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

    private static XnaColor ToXnaColor(UiColor color, float alpha)
    {
        var alphaByte = (byte)Math.Clamp((int)MathF.Round(color.Alpha * alpha), 0, 255);
        return new XnaColor(color.Red, color.Green, color.Blue, alphaByte);
    }
}
#endif
