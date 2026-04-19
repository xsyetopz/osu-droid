namespace OsuDroid.Game.Runtime;

public enum MenuMusicCommand
{
    None,
    Previous,
    Play,
    Pause,
    Stop,
    Next,
}

public interface IMenuMusicController
{
    MenuMusicCommand LastCommand { get; }

    void Execute(MenuMusicCommand command);
}

public sealed class NoOpMenuMusicController : IMenuMusicController
{
    public MenuMusicCommand LastCommand { get; private set; }

    public void Execute(MenuMusicCommand command) => LastCommand = command;
}
