namespace OsuDroid.Game.Scenes.MainMenu;

public enum MainMenuAction
{
    Activate,
    Back,
    MoveUp,
    MoveDown,
}

public enum MainMenuRoute
{
    None,
    Solo,
    Multiplayer,
    Settings,
    Exit,
}

public enum MainMenuReturnTransition
{
    None,
    SongSelectBack,
}
