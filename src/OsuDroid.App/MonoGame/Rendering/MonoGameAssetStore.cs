#if ANDROID || IOS
using Microsoft.Maui.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class MonoGameAssetStore(GraphicsDevice graphicsDevice)
{
    private readonly Dictionary<string, Texture2D> textures = new(StringComparer.Ordinal);

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

        textures.Clear();
    }
}
#endif
