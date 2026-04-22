#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer(GraphicsDevice graphicsDevice, ContentManager contentManager)
{
    private const int DiagnosticsBorderThickness = 6;
    private const int DiagnosticsMarkerSize = 36;

    private readonly MonoGameAssetStore assetStore = new(graphicsDevice, contentManager);
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
                DrawFill(
                    spriteBatch,
                    destination,
                    color,
                    element.CornerRadius * frame.Viewport.Scale,
                    element.CornerMode,
                    metrics,
                    element.RotationDegrees,
                    element.RotationOriginX,
                    element.RotationOriginY);
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
                var textDestination = element.ClipToBounds
                    ? DrawClippedTextTexture(spriteBatch, texture, destination, element.TextStyle)
                    : FitTextTexture(texture, destination, element.TextStyle);
                if (!element.ClipToBounds)
                    spriteBatch.Draw(texture, textDestination, XnaColor.White);
                if (element.TextStyle.Underline)
                    DrawUnderline(spriteBatch, textDestination, color);
                continue;
            }

            var textureAsset = element.ExternalAssetPath is not null
                ? assetStore.TryGetExternalTexture(element.ExternalAssetPath)
                : element.AssetName is null ? null : assetStore.GetTexture(frame.AssetManifest, element.AssetName, metrics);
            if (textureAsset is not null)
                DrawSprite(spriteBatch, textureAsset, destination, color, element.RotationDegrees, element.SpriteFit, element.RotationOriginX, element.RotationOriginY, element.SpriteSource);
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

        assetStore.UploadReadyExternalTextures(metrics);
        return elementIndex;
    }

    public void PreloadStatic(UiAssetManifest manifest, RenderCacheMetrics? metrics = null) => assetStore.Preload(manifest, metrics);

    private static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, XnaRect destination, XnaColor color, float rotationDegrees, UiSpriteFit fit, float rotationOriginX, float rotationOriginY, UiRect? explicitSource)
    {
        var source = explicitSource is null
            ? fit == UiSpriteFit.Stretch ? null : CalculateSourceRect(texture, destination, fit)
            : new XnaRect(
                (int)MathF.Round(explicitSource.Value.X),
                (int)MathF.Round(explicitSource.Value.Y),
                Math.Max(1, (int)MathF.Round(explicitSource.Value.Width)),
                Math.Max(1, (int)MathF.Round(explicitSource.Value.Height)));

        if (Math.Abs(rotationDegrees) < 0.001f)
        {
            spriteBatch.Draw(texture, destination, source, color);
            return;
        }

        var sourceWidth = source?.Width ?? texture.Width;
        var sourceHeight = source?.Height ?? texture.Height;
        var position = new Vector2(
            destination.X + destination.Width * rotationOriginX,
            destination.Y + destination.Height * rotationOriginY);
        var origin = new Vector2(sourceWidth * rotationOriginX, sourceHeight * rotationOriginY);
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


    private static XnaRect DrawClippedTextTexture(SpriteBatch spriteBatch, Texture2D texture, XnaRect bounds, UiTextStyle style)
    {
        var width = Math.Min(texture.Width, bounds.Width);
        var height = Math.Min(texture.Height, bounds.Height);
        var sourceX = style.Alignment switch
        {
            UiTextAlignment.Center => Math.Max(0, (texture.Width - width) / 2),
            UiTextAlignment.Right => Math.Max(0, texture.Width - width),
            _ => 0,
        };
        var sourceY = style.VerticalAlignment == UiTextVerticalAlignment.Middle
            ? Math.Max(0, (texture.Height - height) / 2)
            : 0;
        var x = style.Alignment switch
        {
            UiTextAlignment.Center => bounds.X + (bounds.Width - width) / 2,
            UiTextAlignment.Right => bounds.Right - width,
            _ => bounds.X,
        };
        var y = style.VerticalAlignment == UiTextVerticalAlignment.Middle
            ? bounds.Y + (bounds.Height - height) / 2
            : bounds.Y;
        var destination = new XnaRect(x, y, width, height);
        var source = new XnaRect(sourceX, sourceY, width, height);
        spriteBatch.Draw(texture, destination, source, XnaColor.White);
        return destination;
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
