using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.UI.Elements;

public sealed record UiFrameSnapshot(
    VirtualViewport Viewport,
    IReadOnlyList<UiElementSnapshot> Elements,
    UiAssetManifest AssetManifest)
{
    public UiElementSnapshot? HitTest(UiPoint point)
    {
        for (int index = Elements.Count - 1; index >= 0; index--)
        {
            UiElementSnapshot element = Elements[index];
            if (element.Action != UiAction.None && element.Bounds.Contains(point))
            {
                return element;
            }
        }

        return null;
    }
}
