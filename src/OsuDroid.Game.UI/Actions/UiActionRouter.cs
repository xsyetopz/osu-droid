namespace OsuDroid.Game.UI.Actions;

public static class UiActionRouter
{
#pragma warning disable IDE0072
    public static MainMenuButtonSlot ToMainMenuSlot(UiAction action) =>
        action switch
        {
            UiAction.MainMenuPrimaryButton => MainMenuButtonSlot.First,
            UiAction.MainMenuSecondaryButton => MainMenuButtonSlot.Second,
            UiAction.MainMenuTertiaryButton => MainMenuButtonSlot.Third,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
#pragma warning restore IDE0072
}
