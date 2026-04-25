namespace OsuDroid.Game.UI.Actions;

public static class UiActionRouter
{
#pragma warning disable IDE0072
    public static MainMenuButtonSlot ToMainMenuSlot(UiAction action) => action switch
    {
        UiAction.MainMenuFirst => MainMenuButtonSlot.First,
        UiAction.MainMenuSecond => MainMenuButtonSlot.Second,
        UiAction.MainMenuThird => MainMenuButtonSlot.Third,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
    };
#pragma warning restore IDE0072
}
