#if ANDROID || IOS
using Microsoft.Maui.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class MonoGameAssetStore(GraphicsDevice graphicsDevice)
{
    private readonly Dictionary<string, Texture2D> textures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Texture2D> fileTextures = new(StringComparer.Ordinal);

    public Texture2D? GetTexture(UiAssetManifest manifest, string logicalName, RenderCacheMetrics? metrics = null)
    {
        if (textures.TryGetValue(logicalName, out var cachedTexture))
            return cachedTexture;

        metrics?.AddSpriteMiss();
        var entry = manifest.Get(logicalName);
        using var stream = OpenAssetStream(entry.PackagePath);
        var texture = Texture2D.FromStream(graphicsDevice, stream);
        textures.Add(logicalName, texture);
        return texture;
    }

    public Texture2D? GetFileTexture(string path, RenderCacheMetrics? metrics = null)
    {
        if (fileTextures.TryGetValue(path, out var cachedTexture))
            return cachedTexture;

        if (!File.Exists(path))
            return null;

        metrics?.AddSpriteMiss();
        using var stream = File.OpenRead(path);
        var texture = Texture2D.FromStream(graphicsDevice, stream);
        fileTextures.Add(path, texture);
        return texture;
    }

    private static Stream OpenAssetStream(string packagePath)
    {
        try
        {
            return FileSystem.OpenAppPackageFileAsync(packagePath).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return TitleContainer.OpenStream(packagePath);
        }
    }

    public void Dispose()
    {
        foreach (var texture in textures.Values)
            texture.Dispose();

        foreach (var texture in fileTextures.Values)
            texture.Dispose();

        textures.Clear();
        fileTextures.Clear();
    }
}
#endif
