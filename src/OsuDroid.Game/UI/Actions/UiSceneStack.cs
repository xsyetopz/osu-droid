namespace OsuDroid.Game.UI.Actions;

public sealed class UiSceneStack
{
    private readonly Stack<string> _sceneNames = new();

    public UiSceneStack(string rootSceneName)
    {
        _sceneNames.Push(rootSceneName);
    }

    public string Current => _sceneNames.Peek();

    public void Push(string sceneName) => _sceneNames.Push(sceneName);

    public bool TryPop()
    {
        if (_sceneNames.Count == 1)
        {
            return false;
        }

        _ = _sceneNames.Pop();
        return true;
    }
}
