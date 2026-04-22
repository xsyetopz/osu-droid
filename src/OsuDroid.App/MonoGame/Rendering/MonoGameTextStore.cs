#if ANDROID || IOS
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
#if ANDROID
using Android.Graphics;
using Color = Android.Graphics.Color;
using Paint = Android.Graphics.Paint;
#endif
#if IOS
using CoreGraphics;
using Foundation;
using UIKit;
#endif

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class MonoGameTextStore(GraphicsDevice graphicsDevice)
{
    private readonly Dictionary<TextKey, Texture2D> textures = new();

    public Texture2D GetTexture(string text, UiTextStyle style, UiColor color, float alpha, float renderScale, RenderCacheMetrics? metrics = null)
    {
        var scaledSize = Math.Max(1f, style.Size * renderScale);
        var key = new TextKey(text, scaledSize, style.Bold, color.Red, color.Green, color.Blue, (byte)Math.Clamp((int)MathF.Round(color.Alpha * alpha), 0, 255));
        if (textures.TryGetValue(key, out var texture))
            return texture;

        metrics?.AddTextMiss();
        texture = CreateTexture(key);
        textures.Add(key, texture);
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in textures.Values)
            texture.Dispose();

        textures.Clear();
    }

    private Texture2D CreateTexture(TextKey key)
    {
#if ANDROID
        return CreateAndroidTexture(key);
#elif IOS
        return CreateIosTexture(key);
#else
        throw new PlatformNotSupportedException();
#endif
    }

#if ANDROID
    private Texture2D CreateAndroidTexture(TextKey key)
    {
        using var paint = new Paint(PaintFlags.AntiAlias)
        {
            TextSize = key.Size,
            Color = Color.Argb(key.Alpha, key.Red, key.Green, key.Blue),
            Typeface = key.Bold ? Typeface.DefaultBold : Typeface.Default,
        };
        var width = Math.Max(1, (int)Math.Ceiling(paint.MeasureText(key.Text)) + 4);
        var height = Math.Max(1, (int)Math.Ceiling(Math.Abs(paint.FontMetrics.Ascent) + paint.FontMetrics.Descent) + 4);
        using var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888!);
        using var canvas = new Canvas(bitmap);
        canvas.DrawText(key.Text, 2f, 2f - paint.FontMetrics.Ascent, paint);
        using var stream = new MemoryStream();
        bitmap.Compress(Bitmap.CompressFormat.Png!, 100, stream);
        stream.Position = 0;
        return Texture2D.FromStream(graphicsDevice, stream);
    }
#endif

#if IOS
    private Texture2D CreateIosTexture(TextKey key)
    {
        var safeText = SanitizeForPlatformText(key.Text);
        using var font = key.Bold ? UIFont.BoldSystemFontOfSize(key.Size) : UIFont.SystemFontOfSize(key.Size);
        var attributes = new UIStringAttributes
        {
            Font = font,
            ForegroundColor = UIColor.FromRGBA(key.Red, key.Green, key.Blue, key.Alpha),
        };
        using var nsText = new NSString(safeText);
        var measured = nsText.GetSizeUsingAttributes(attributes);
        var width = Math.Max(1, (int)Math.Ceiling(measured.Width) + 4);
        var height = Math.Max(1, (int)Math.Ceiling(measured.Height) + 4);

        var format = UIGraphicsImageRendererFormat.DefaultFormat;
        format.Scale = 1f;
        using var renderer = new UIGraphicsImageRenderer(new CGSize(width, height), format);
        using var image = renderer.CreateImage(_ =>
            nsText.DrawString(new CGRect(2, 2, width - 4, height - 4), attributes));
        using var data = image.AsPNG();
        if (data is null)
            throw new InvalidOperationException("Failed to encode text texture.");

        using var stream = data.AsStream();
        return Texture2D.FromStream(graphicsDevice, stream);
    }

    private static string SanitizeForPlatformText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var builder = new System.Text.StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var current = text[i];
            if (char.IsHighSurrogate(current))
            {
                if (i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    builder.Append(current);
                    builder.Append(text[i + 1]);
                    i++;
                }
                else
                {
                    builder.Append('\uFFFD');
                }
                continue;
            }

            if (char.IsLowSurrogate(current))
            {
                builder.Append('\uFFFD');
                continue;
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
#endif

    private readonly record struct TextKey(string Text, float Size, bool Bold, byte Red, byte Green, byte Blue, byte Alpha);
}
#endif
