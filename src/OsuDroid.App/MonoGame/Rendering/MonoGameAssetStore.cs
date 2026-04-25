#if ANDROID || IOS
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class MonoGameAssetStore(GraphicsDevice graphicsDevice, ContentManager contentManager)
{
    private readonly Dictionary<string, Texture2D> textures = new(StringComparer.Ordinal);
    private readonly ExternalTextureCache externalTextures = new(graphicsDevice);

    public Texture2D? GetTexture(UiAssetManifest manifest, string logicalName, RenderCacheMetrics? metrics = null)
    {
        if (textures.TryGetValue(logicalName, out var cachedTexture))
            return cachedTexture;

        metrics?.AddSpriteMiss();
        var start = PerfDiagnostics.Start();
        var entry = manifest.Get(logicalName);
        var texture = contentManager.Load<Texture2D>(entry.ContentName);
        textures.Add(logicalName, texture);
        PerfDiagnostics.Log("asset.contentTextureLoad", start, $"asset=\"{logicalName}\"");
        return texture;
    }

    public Texture2D? TryGetExternalTexture(string path) => externalTextures.TryGet(path);

    public void RequestExternalTexture(string path, int maxWidth, int maxHeight, RenderCacheMetrics? metrics = null) =>
        externalTextures.Request(path, maxWidth, maxHeight, metrics);

    public void UploadReadyExternalTextures(RenderCacheMetrics? metrics = null) =>
        externalTextures.UploadReady(metrics);

    public void Preload(UiAssetManifest manifest, RenderCacheMetrics? metrics = null)
    {
        foreach (var entry in manifest.Entries)
        {
            if (entry.Kind == UiAssetKind.Texture)
                _ = GetTexture(manifest, entry.LogicalName, metrics);
        }
    }

    public void Dispose()
    {
        externalTextures.Dispose();
        textures.Clear();
        contentManager.Unload();
    }
}
#endif
