#if ANDROID || IOS
using OsuDroid.Game.UI;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed class RenderWarmupQueue
{
    private readonly Queue<UiFrameSnapshot> pendingFrames = new();
    private UiFrameSnapshot? activeFrame;
    private int activeElementIndex;

    public bool IsComplete => activeFrame is null && pendingFrames.Count == 0;

    public void Reset(IEnumerable<UiFrameSnapshot> frames)
    {
        pendingFrames.Clear();
        foreach (var frame in frames)
            pendingFrames.Enqueue(frame);

        activeFrame = null;
        activeElementIndex = 0;
    }

    public void Run(MonoGameUiRenderer renderer, TimeSpan budget, RenderCacheMetrics metrics)
    {
        var deadline = DateTime.UtcNow + budget;
        do
        {
            activeFrame ??= DequeueFrame(metrics);
            if (activeFrame is null)
                return;

            activeElementIndex = renderer.WarmUp(
                activeFrame,
                activeElementIndex,
                deadline,
                metrics
            );
            if (activeElementIndex < activeFrame.Elements.Count)
                return;

            activeFrame = null;
            activeElementIndex = 0;
        } while (DateTime.UtcNow < deadline);
    }

    private UiFrameSnapshot? DequeueFrame(RenderCacheMetrics metrics)
    {
        if (pendingFrames.Count == 0)
            return null;

        metrics.AddWarmupFrame();
        return pendingFrames.Dequeue();
    }
}
#endif
