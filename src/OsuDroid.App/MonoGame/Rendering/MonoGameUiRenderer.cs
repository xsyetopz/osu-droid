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


    }
#endif
