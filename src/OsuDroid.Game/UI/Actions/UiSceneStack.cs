namespace OsuDroid.Game.UI;

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
