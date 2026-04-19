namespace OsuDroid.Game.UI;

public sealed record UiFrameSnapshot(
    VirtualViewport Viewport,
    IReadOnlyList<UiElementSnapshot> Elements,
    UiAssetManifest AssetManifest)
{
    public UiElementSnapshot? HitTest(UiPoint point)
    {
        for (var index = Elements.Count - 1; index >= 0; index--)
        {
            var element = Elements[index];
            if (element.Action != UiAction.None && element.Bounds.Contains(point))
                return element;
        }

        return null;
    }
}
