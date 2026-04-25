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
            destination = ApplyMeasuredTextAnchor(frame, element, destination);
            var clip = ToSurfaceClipRect(frame, element);
            var color = ToXnaColor(element.Color, element.Alpha);

            if (element.Kind == UiElementKind.Fill)
            {
                DrawFillClipped(
                    spriteBatch,
                    destination,
                    clip,
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

            if (element.Kind == UiElementKind.ProgressRing)
            {
                if (element.ProgressRing is not null)
                {
                    var ringTexture = shapeStore.GetProgressRing(
                        destination.Width,
                        destination.Height,
                        element.ProgressRing.StrokeWidth * frame.Viewport.Scale,
                        element.ProgressRing.SweepDegrees,
                        color,
                        metrics);
                    DrawSprite(spriteBatch, ringTexture, destination, XnaColor.White, element.RotationDegrees, UiSpriteFit.Stretch, element.RotationOriginX, element.RotationOriginY, null);
                }
                continue;
            }

            if (element.Kind == UiElementKind.Text)
            {
                if (element.Text is null || element.TextStyle is null)
                    continue;

                if (clip is { } textClip)
                {
                    destination = Intersect(destination, textClip);
                    if (destination.Width <= 0 || destination.Height <= 0)
                        continue;
                }

                var texture = textStore.GetTexture(element.Text, element.TextStyle, element.Color, element.Alpha, frame.Viewport.Scale, metrics);
                var textDestination = element.ClipToBounds
                    ? DrawClippedTextTexture(spriteBatch, texture, destination, element.TextStyle, frame.Viewport.Scale)
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
                DrawSpriteClipped(spriteBatch, textureAsset, destination, clip, color, element.RotationDegrees, element.SpriteFit, element.RotationOriginX, element.RotationOriginY, element.SpriteSource);
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

    private void DrawFillClipped(
        SpriteBatch spriteBatch,
        XnaRect bounds,
        XnaRect? clip,
        XnaColor color,
        float radius,
        UiCornerMode cornerMode = UiCornerMode.All,
        RenderCacheMetrics? metrics = null,
        float rotationDegrees = 0f,
        float rotationOriginX = 0.5f,
        float rotationOriginY = 0.5f)
    {
        if (clip is null || Math.Abs(rotationDegrees) > 0.001f)
        {
            DrawFill(spriteBatch, bounds, color, radius, cornerMode, metrics, rotationDegrees, rotationOriginX, rotationOriginY);
            return;
        }

        var clipped = Intersect(bounds, clip.Value);
        if (pixel is null || clipped.Width <= 0 || clipped.Height <= 0 || color.A == 0)
            return;

        if (radius <= 1f || cornerMode == UiCornerMode.None)
        {
            spriteBatch.Draw(pixel, clipped, color);
            return;
        }

        var texture = shapeStore.GetRoundedFill(bounds.Width, bounds.Height, radius, cornerMode, color, metrics);
        var source = new XnaRect(clipped.X - bounds.X, clipped.Y - bounds.Y, clipped.Width, clipped.Height);
        spriteBatch.Draw(texture, clipped, source, XnaColor.White);
    }

    private static void DrawSpriteClipped(SpriteBatch spriteBatch, Texture2D texture, XnaRect destination, XnaRect? clip, XnaColor color, float rotationDegrees, UiSpriteFit fit, float rotationOriginX, float rotationOriginY, UiRect? explicitSource)
    {
        if (clip is null || Math.Abs(rotationDegrees) > 0.001f)
        {
            DrawSprite(spriteBatch, texture, destination, color, rotationDegrees, fit, rotationOriginX, rotationOriginY, explicitSource);
            return;
        }

        var clipped = Intersect(destination, clip.Value);
        if (clipped.Width <= 0 || clipped.Height <= 0)
            return;

        var source = explicitSource is null
            ? fit == UiSpriteFit.Stretch ? new XnaRect(0, 0, texture.Width, texture.Height) : CalculateSourceRect(texture, destination, fit) ?? new XnaRect(0, 0, texture.Width, texture.Height)
            : new XnaRect(
                (int)MathF.Round(explicitSource.Value.X),
                (int)MathF.Round(explicitSource.Value.Y),
                Math.Max(1, (int)MathF.Round(explicitSource.Value.Width)),
                Math.Max(1, (int)MathF.Round(explicitSource.Value.Height)));

        var left = (clipped.X - destination.X) / (float)destination.Width;
        var top = (clipped.Y - destination.Y) / (float)destination.Height;
        var right = (destination.Right - clipped.Right) / (float)destination.Width;
        var bottom = (destination.Bottom - clipped.Bottom) / (float)destination.Height;
        var sourceLeft = (int)MathF.Round(source.Width * left);
        var sourceTop = (int)MathF.Round(source.Height * top);
        var sourceRight = (int)MathF.Round(source.Width * right);
        var sourceBottom = (int)MathF.Round(source.Height * bottom);
        var clippedSource = new XnaRect(
            source.X + sourceLeft,
            source.Y + sourceTop,
            Math.Max(1, source.Width - sourceLeft - sourceRight),
            Math.Max(1, source.Height - sourceTop - sourceBottom));
        spriteBatch.Draw(texture, clipped, clippedSource, color);
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


    private static XnaRect DrawClippedTextTexture(SpriteBatch spriteBatch, Texture2D texture, XnaRect bounds, UiTextStyle style, float scale)
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
            : CalculateAutoScrollSourceX(style.AutoScroll, Math.Max(0, texture.Width - width), scale);
        var sourceY = style.VerticalAlignment == UiTextVerticalAlignment.Middle
            ? Math.Max(0, (texture.Height - height) / 2)
            : 0;
        var x = style.Alignment switch
        {
            UiTextAlignment.Center when style.AutoScroll is null => bounds.X + (bounds.Width - width) / 2,
            UiTextAlignment.Right when style.AutoScroll is null => bounds.Right - width,
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

    private static int CalculateAutoScrollSourceX(UiTextAutoScroll autoScroll, int maxScroll, float scale)
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
        return scrollSeconds >= scrollDuration ? maxScroll : (int)Math.Min(maxScroll, scrollSeconds * speed);
    }

    private XnaRect ApplyMeasuredTextAnchor(UiFrameSnapshot frame, UiElementSnapshot element, XnaRect destination)
    {
        if (element.MeasuredTextAnchor is not { } anchor)
            return destination;

        var textTexture = textStore.GetTexture(anchor.Text, anchor.TextStyle, UiColor.Opaque(255, 255, 255), 1f, frame.Viewport.Scale);
        var right = frame.Viewport.OffsetX + anchor.RightX * frame.Viewport.Scale;
        var leftPadding = anchor.LeftPadding * frame.Viewport.Scale;
        return destination with
        {
            X = (int)MathF.Round(right - textTexture.Width - leftPadding),
        };
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

    private static XnaRect? ToSurfaceClipRect(UiFrameSnapshot frame, UiElementSnapshot element)
    {
        if (element.ClipBounds is null)
            return null;

        var bounds = frame.Viewport.ToSurface(element.ClipBounds.Value);
        return new XnaRect(
            (int)MathF.Round(bounds.X),
            (int)MathF.Round(bounds.Y),
            Math.Max(0, (int)MathF.Round(bounds.Width)),
            Math.Max(0, (int)MathF.Round(bounds.Height)));
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
