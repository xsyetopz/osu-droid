#if ANDROID || IOS
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using SkiaSharp;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class MonoGameShapeStore(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly Dictionary<ShapeCacheKey, Texture2D> cache = new();
    private readonly Dictionary<ProgressRingCacheKey, Texture2D> progressRingCache = new();

    public Texture2D GetRoundedFill(
        int width,
        int height,
        float radius,
        UiCornerMode cornerMode,
        XnaColor color,
        RenderCacheMetrics? metrics = null
    )
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        var roundedRadius = Math.Max(0, (int)MathF.Round(radius));
        var key = new ShapeCacheKey(
            width,
            height,
            roundedRadius,
            cornerMode,
            color.R,
            color.G,
            color.B,
            color.A
        );
        if (cache.TryGetValue(key, out var texture))
            return texture;

        metrics?.AddShapeMiss();
        texture = CreateRoundedFillTexture(width, height, roundedRadius, cornerMode, color);
        cache[key] = texture;
        return texture;
    }

    public Texture2D GetProgressRing(
        int width,
        int height,
        float strokeWidth,
        float sweepDegrees,
        XnaColor color,
        RenderCacheMetrics? metrics = null
    )
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        int roundedStrokeWidth = Math.Max(1, (int)MathF.Round(strokeWidth));
        int roundedSweepDegrees = Math.Clamp((int)MathF.Round(sweepDegrees), 0, 360);
        var key = new ProgressRingCacheKey(
            width,
            height,
            roundedStrokeWidth,
            roundedSweepDegrees,
            color.R,
            color.G,
            color.B,
            color.A
        );
        if (progressRingCache.TryGetValue(key, out var texture))
            return texture;

        metrics?.AddShapeMiss();
        texture = CreateProgressRingTexture(
            width,
            height,
            roundedStrokeWidth,
            roundedSweepDegrees,
            color
        );
        progressRingCache[key] = texture;
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in cache.Values)
            texture.Dispose();
        cache.Clear();
        foreach (var texture in progressRingCache.Values)
            texture.Dispose();
        progressRingCache.Clear();
    }

    private Texture2D CreateRoundedFillTexture(
        int width,
        int height,
        int radius,
        UiCornerMode cornerMode,
        XnaColor color
    )
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = new SKColor(color.R, color.G, color.B, color.A),
        };

        using var path = CreateRoundedPath(
            width,
            height,
            Math.Min(radius, Math.Min(width, height) / 2),
            cornerMode
        );
        canvas.DrawPath(path, paint);
        canvas.Flush();

        var pixels = new XnaColor[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            pixels[y * width + x] = new XnaColor(pixel.Red, pixel.Green, pixel.Blue, pixel.Alpha);
        }

        var texture = new Texture2D(graphicsDevice, width, height);
        texture.SetData(pixels);
        return texture;
    }

    private Texture2D CreateProgressRingTexture(
        int width,
        int height,
        int strokeWidth,
        int sweepDegrees,
        XnaColor color
    )
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        if (sweepDegrees > 0)
        {
            float inset = strokeWidth / 2f + 1f;
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                StrokeCap = SKStrokeCap.Round,
                Color = new SKColor(color.R, color.G, color.B, color.A),
            };
            var oval = new SKRect(inset, inset, width - inset, height - inset);
            canvas.DrawArc(oval, -90f, Math.Min(359.9f, sweepDegrees), false, paint);
        }

        canvas.Flush();

        var pixels = new XnaColor[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            pixels[y * width + x] = new XnaColor(pixel.Red, pixel.Green, pixel.Blue, pixel.Alpha);
        }

        var texture = new Texture2D(graphicsDevice, width, height);
        texture.SetData(pixels);
        return texture;
    }

    private static SKPath CreateRoundedPath(
        int width,
        int height,
        int radius,
        UiCornerMode cornerMode
    )
    {
        var path = new SKPath();
        var roundTop = cornerMode is UiCornerMode.All or UiCornerMode.Top;
        var roundBottom = cornerMode is UiCornerMode.All or UiCornerMode.Bottom;
        var topLeft = roundTop ? radius : 0f;
        var topRight = roundTop ? radius : 0f;
        var bottomRight = roundBottom ? radius : 0f;
        var bottomLeft = roundBottom ? radius : 0f;

        path.MoveTo(topLeft, 0f);
        path.LineTo(width - topRight, 0f);
        if (topRight > 0f)
            path.QuadTo(width, 0f, width, topRight);

        path.LineTo(width, height - bottomRight);
        if (bottomRight > 0f)
            path.QuadTo(width, height, width - bottomRight, height);

        path.LineTo(bottomLeft, height);
        if (bottomLeft > 0f)
            path.QuadTo(0f, height, 0f, height - bottomLeft);

        path.LineTo(0f, topLeft);
        if (topLeft > 0f)
            path.QuadTo(0f, 0f, topLeft, 0f);

        path.Close();
        return path;
    }

    private readonly record struct ShapeCacheKey(
        int Width,
        int Height,
        int Radius,
        UiCornerMode CornerMode,
        byte R,
        byte G,
        byte B,
        byte A
    );

    private readonly record struct ProgressRingCacheKey(
        int Width,
        int Height,
        int StrokeWidth,
        int SweepDegrees,
        byte R,
        byte G,
        byte B,
        byte A
    );
}
#endif
