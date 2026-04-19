#if ANDROID || IOS
using Material.Icons;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using SkiaSharp;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class MonoGameIconStore(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly Dictionary<IconCacheKey, Texture2D> cache = new();

    public Texture2D GetIcon(UiMaterialIcon icon, int width, int height, UiColor color, float alpha, RenderCacheMetrics? metrics = null)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        var key = new IconCacheKey(icon, width, height, color.Red, color.Green, color.Blue, color.Alpha, alpha);
        if (cache.TryGetValue(key, out var texture))
            return texture;

        metrics?.AddIconMiss();
        texture = CreateIconTexture(icon, width, height, color, alpha);
        cache[key] = texture;
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in cache.Values)
            texture.Dispose();
        cache.Clear();
    }

    private Texture2D CreateIconTexture(UiMaterialIcon icon, int width, int height, UiColor color, float alpha)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        using var path = SKPath.ParseSvgPathData(MaterialIconDataProvider.GetData(ToMaterialIconKind(icon)));
        var scale = Math.Min(width, height) / 24f;
        var offsetX = (width - 24f * scale) / 2f;
        var offsetY = (height - 24f * scale) / 2f;
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = new SKColor(color.Red, color.Green, color.Blue, (byte)Math.Clamp(color.Alpha * alpha, 0f, 255f)),
        };
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

    private static MaterialIconKind ToMaterialIconKind(UiMaterialIcon icon) => icon switch
    {
        UiMaterialIcon.ArrowBack => MaterialIconKind.ArrowBack,
        UiMaterialIcon.ArrowDropDown => MaterialIconKind.ArrowDropDown,
        UiMaterialIcon.ChevronRight => MaterialIconKind.ChevronRight,
        UiMaterialIcon.Check => MaterialIconKind.Check,
        UiMaterialIcon.CheckboxBlankOutline => MaterialIconKind.CheckboxBlankOutline,
        UiMaterialIcon.ViewGridOutline => MaterialIconKind.ViewGridOutline,
        UiMaterialIcon.GamepadVariantOutline => MaterialIconKind.GamepadVariantOutline,
        UiMaterialIcon.MonitorDashboard => MaterialIconKind.MonitorDashboard,
        UiMaterialIcon.Headphones => MaterialIconKind.Headphones,
        UiMaterialIcon.LibraryMusic => MaterialIconKind.LibraryMusic,
        UiMaterialIcon.GestureTapButton => MaterialIconKind.GestureTapButton,
        UiMaterialIcon.Cogs => MaterialIconKind.Cogs,
        _ => throw new ArgumentOutOfRangeException(nameof(icon), icon, null),
    };

    private readonly record struct IconCacheKey(UiMaterialIcon Icon, int Width, int Height, byte R, byte G, byte B, byte A, float Alpha);
}
#endif
