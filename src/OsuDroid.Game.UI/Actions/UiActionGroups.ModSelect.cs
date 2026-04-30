namespace OsuDroid.Game.UI.Actions;

public static partial class UiActionGroups
{
    private static readonly UiIndexedActionMap s_modSelectPresetSlots = new(
        UiAction.ModSelectPresetSlot0,
        UiAction.ModSelectPresetSlot15
    );
    private static readonly UiIndexedActionMap s_modSelectCatalogModToggles = new(
        UiAction.ModSelectCatalogModToggle0,
        UiAction.ModSelectCatalogModToggle31
    );
    private static readonly UiIndexedActionMap s_modSelectCustomizeSettingDecreases = new(
        UiAction.ModSelectCustomizeSettingDecrease0,
        UiAction.ModSelectCustomizeSettingDecrease31
    );
    private static readonly UiIndexedActionMap s_modSelectCustomizeSettingIncreases = new(
        UiAction.ModSelectCustomizeSettingIncrease0,
        UiAction.ModSelectCustomizeSettingIncrease31
    );

    public static bool TryGetModSelectPresetSlotIndex(UiAction action, out int index) =>
        s_modSelectPresetSlots.TryGetIndex(action, out index);

    public static bool TryGetModSelectPresetSlotAction(int index, out UiAction action) =>
        s_modSelectPresetSlots.TryGetAction(index, out action);

    public static bool TryGetModSelectCatalogModToggleIndex(UiAction action, out int index) =>
        s_modSelectCatalogModToggles.TryGetIndex(action, out index);

    public static bool TryGetModSelectCatalogModToggleAction(int index, out UiAction action) =>
        s_modSelectCatalogModToggles.TryGetAction(index, out action);

    public static bool TryGetModSelectCustomizeSettingDecreaseIndex(
        UiAction action,
        out int index
    ) => s_modSelectCustomizeSettingDecreases.TryGetIndex(action, out index);

    public static bool TryGetModSelectCustomizeSettingDecreaseAction(
        int index,
        out UiAction action
    ) => s_modSelectCustomizeSettingDecreases.TryGetAction(index, out action);

    public static bool TryGetModSelectCustomizeSettingIncreaseIndex(
        UiAction action,
        out int index
    ) => s_modSelectCustomizeSettingIncreases.TryGetIndex(action, out index);

    public static bool TryGetModSelectCustomizeSettingIncreaseAction(
        int index,
        out UiAction action
    ) => s_modSelectCustomizeSettingIncreases.TryGetAction(index, out action);
}
