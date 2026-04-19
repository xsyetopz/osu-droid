using OsuDroid.Game.Scenes;

namespace OsuDroid.Game.UI;

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
