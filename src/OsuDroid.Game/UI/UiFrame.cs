using OsuDroid.Game.Scenes;

namespace OsuDroid.Game.UI;

public enum UiElementKind
{
    Fill,
    Sprite,
}

public enum UiAction
{
    None,
    MainMenuCookie,
    MainMenuFirst,
    MainMenuSecond,
    MainMenuThird,
}

public sealed record UiElementSnapshot(
    string Id,
    UiElementKind Kind,
    UiRect Bounds,
    UiColor Color,
    float Alpha,
    string? AssetName = null,
    UiAction Action = UiAction.None);

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

public sealed class UiSceneStack
{
    private readonly Stack<string> sceneNames = new();

    public UiSceneStack(string rootSceneName) => sceneNames.Push(rootSceneName);

    public string Current => sceneNames.Peek();

    public void Push(string sceneName) => sceneNames.Push(sceneName);

    public bool TryPop()
    {
        if (sceneNames.Count == 1)
            return false;

        _ = sceneNames.Pop();
        return true;
    }
}

public static class UiActionRouter
{
    public static MainMenuButtonSlot ToMainMenuSlot(UiAction action) => action switch
    {
        UiAction.MainMenuFirst => MainMenuButtonSlot.First,
        UiAction.MainMenuSecond => MainMenuButtonSlot.Second,
        UiAction.MainMenuThird => MainMenuButtonSlot.Third,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
    };
}
