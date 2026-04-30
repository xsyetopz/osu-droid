using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
#pragma warning disable IDE0072 // UiAction has cross-scene members; Mod Select handles only its own actions.
    private bool HandleModSelectUiAction(UiAction action, VirtualViewport viewport) =>
        _activeScene == ActiveScene.ModSelect
        && (
            action switch
            {
                UiAction.ModSelectBack => Do(BackFromModSelect),
                UiAction.ModSelectClear => Do(_modSelect.Clear),
                UiAction.ModSelectCustomize => Do(() => _modSelect.ToggleCustomizePanel()),
                UiAction.ModSelectSearchBox => Do(() => _modSelect.FocusSearch(viewport)),
                UiAction.ModSelectPresetAdd => Do(() => _modSelect.FocusPresetName(viewport)),
                UiAction.ModSelectPresetNameInput => Do(() =>
                    _modSelect.FocusPresetDialogName(viewport)
                ),
                UiAction.ModSelectPresetSave => Do(_modSelect.SavePresetDialog),
                UiAction.ModSelectPresetCancel or UiAction.ModSelectPresetDeleteCancel => Do(
                    _modSelect.CancelPresetDialog
                ),
                UiAction.ModSelectPresetDeleteConfirm => Do(_modSelect.ConfirmPresetDelete),
                UiAction.ModSelectPresetBackdrop => Do(_modSelect.CloseCustomizePanel),
                _ => false,
            }
            || (
                UiActionGroups.TryGetModSelectPresetSlotIndex(action, out int presetIndex)
                && Do(() => _modSelect.ActivatePreset(presetIndex))
            )
            || (
                UiActionGroups.TryGetModSelectCustomizeSettingDecreaseIndex(
                    action,
                    out int decreaseIndex
                )
                && Do(() =>
                {
                    _modSelect.AdjustCustomizeSetting(decreaseIndex, -1);
                    _songSelect.SetSelectedModState(_modSelect.CreateSelectionState());
                })
            )
            || (
                UiActionGroups.TryGetModSelectCustomizeSettingIncreaseIndex(
                    action,
                    out int increaseIndex
                )
                && Do(() =>
                {
                    _modSelect.FocusCustomizeSettingInput(increaseIndex, viewport);
                    _songSelect.SetSelectedModState(_modSelect.CreateSelectionState());
                })
            )
        );
#pragma warning restore IDE0072

    private void BackFromModSelect()
    {
        _modSelect.ClosePresetDialog();
        _textInputService.HideTextInput();
        _songSelect.SetSelectedModState(_modSelect.CreateSelectionState());
        _activeScene = ActiveScene.SongSelect;
    }
}
