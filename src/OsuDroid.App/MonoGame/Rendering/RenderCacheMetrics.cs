#if ANDROID || IOS
namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class RenderCacheMetrics
{
    public int WarmupFrames { get; private set; }

    public int WarmupElements { get; private set; }

    public int TextMisses { get; private set; }

    public int IconMisses { get; private set; }

    public int ShapeMisses { get; private set; }

    public int SpriteMisses { get; private set; }

    public bool HasCacheMisses =>
        TextMisses > 0 || IconMisses > 0 || ShapeMisses > 0 || SpriteMisses > 0;

    public void AddWarmupFrame() => WarmupFrames++;

    public void AddWarmupElement() => WarmupElements++;

    public void AddTextMiss() => TextMisses++;

    public void AddIconMiss() => IconMisses++;

    public void AddShapeMiss() => ShapeMisses++;

    public void AddSpriteMiss() => SpriteMisses++;

    public override string ToString() =>
        $"frames={WarmupFrames} elements={WarmupElements} text={TextMisses} icons={IconMisses} shapes={ShapeMisses} sprites={SpriteMisses}";
}
#endif
