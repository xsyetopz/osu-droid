namespace OsuDroid.Game.UI.Actions;

public static partial class UiActionGroups
{
    private static readonly UiIndexedActionMap s_optionsActiveRows = new(
        UiAction.OptionsActiveRow0,
        UiAction.OptionsActiveRow63
    );
    private static readonly UiIndexedActionMap s_optionsSections = new(
        UiAction.OptionsSectionGeneral,
        UiAction.OptionsSectionAdvanced
    );
    private static readonly UiIndexedActionMap s_optionsToggles = new(
        UiAction.OptionsToggleServerConnection,
        UiAction.OptionsToggleBeatmapSounds
    );

    public static bool TryGetOptionsActiveRowIndex(UiAction action, out int index) =>
        s_optionsActiveRows.TryGetIndex(action, out index);

    public static bool TryGetOptionsActiveRowAction(int index, out UiAction action) =>
        s_optionsActiveRows.TryGetAction(index, out action);

    public static bool IsOptionsSection(UiAction action) => s_optionsSections.Contains(action);

    public static bool IsOptionsToggle(UiAction action) => s_optionsToggles.Contains(action);

    public static bool IsOptionsActiveRow(UiAction action) => s_optionsActiveRows.Contains(action);
}
