#if ANDROID || IOS
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class ExternalTextureCache(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly Dictionary<string, Texture2D> textures = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, DecodedTexture> decoded = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> requested = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> failed = new(StringComparer.Ordinal);
    private bool disposed;

    public Texture2D? TryGet(string path) => textures.GetValueOrDefault(path);

    public void Request(string path, int maxWidth, int maxHeight, RenderCacheMetrics? metrics = null)
    {
        if (disposed || textures.ContainsKey(path) || decoded.ContainsKey(path) || failed.ContainsKey(path))
            return;

        if (!requested.TryAdd(path, 0))
            return;

        metrics?.AddSpriteMiss();
        var boundedMaxWidth = Math.Max(1, maxWidth);
        var boundedMaxHeight = Math.Max(1, maxHeight);
        _ = Task.Run(() => Decode(path, boundedMaxWidth, boundedMaxHeight));
    }

    public void UploadReady(RenderCacheMetrics? metrics = null)
    {
        if (disposed)
            return;

        foreach (var (path, texture) in decoded)
        {
            if (!decoded.TryRemove(path, out var decodedTexture))
                continue;

            if (textures.ContainsKey(path))
                return;

            var start = PerfDiagnostics.Start();
            var gpuTexture = new Texture2D(graphicsDevice, decodedTexture.Width, decodedTexture.Height, false, SurfaceFormat.Color);
            gpuTexture.SetData(decodedTexture.Rgba);
            textures[path] = gpuTexture;
            metrics?.AddSpriteMiss();
            PerfDiagnostics.Log("externalTexture.upload", start, $"path=\"{Path.GetFileName(path)}\" size={decodedTexture.Width}x{decodedTexture.Height}");
            return;
        }
    }

    public void Dispose()
    {
        disposed = true;
        foreach (var texture in textures.Values)
            texture.Dispose();
        textures.Clear();
        decoded.Clear();
        requested.Clear();
        failed.Clear();
    }

    private void Decode(string path, int maxWidth, int maxHeight)
    {
        var start = PerfDiagnostics.Start();
        try
        {
            if (!File.Exists(path))
            {
                failed.TryAdd(path, 0);
                return;
            }

            using var source = SKBitmap.Decode(path);
            if (source is null || source.Width <= 0 || source.Height <= 0)
            {
                failed.TryAdd(path, 0);
                return;
            }

            var scale = Math.Min(1f, Math.Min(maxWidth / (float)source.Width, maxHeight / (float)source.Height));
            var width = Math.Max(1, (int)MathF.Round(source.Width * scale));
            var height = Math.Max(1, (int)MathF.Round(source.Height * scale));
            using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            if (width == source.Width && height == source.Height && source.ColorType == SKColorType.Rgba8888 && source.AlphaType == SKAlphaType.Unpremul)
                source.CopyTo(bitmap, SKColorType.Rgba8888);
            else
                source.ScalePixels(bitmap, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));

            var rgba = new byte[bitmap.ByteCount];
            Marshal.Copy(bitmap.GetPixels(), rgba, 0, rgba.Length);
            decoded[path] = new DecodedTexture(width, height, rgba);
            PerfDiagnostics.Log("externalTexture.decode", start, $"path=\"{Path.GetFileName(path)}\" size={width}x{height}");
        }
        catch (Exception ex)
        {
            failed.TryAdd(path, 0);
            Debug.WriteLine($"[osu-droid] External texture decode failed: {path}: {ex}");
        }
    }

    private sealed record DecodedTexture(int Width, int Height, byte[] Rgba);
}
#endif
