using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
#pragma warning disable IDE0072 // UiAction has cross-scene members; Mod Select handles only its own actions.
    private bool HandleModSelectUiAction(UiAction action, VirtualViewport viewport) =>
        _activeScene == ActiveScene.ModSelect && (action switch
        {
            UiAction.ModSelectBack => Do(BackFromModSelect),
            UiAction.ModSelectClear => Do(_modSelect.Clear),
            UiAction.ModSelectCustomize => true,
            UiAction.ModSelectSearchBox => Do(() => _modSelect.FocusSearch(viewport)),
            UiAction.ModSelectPresetAdd => Do(() => _modSelect.FocusPresetName(viewport)),
            UiAction.ModSelectPresetNameInput => Do(() => _modSelect.FocusPresetDialogName(viewport)),
            UiAction.ModSelectPresetSave => Do(_modSelect.SavePresetDialog),
            UiAction.ModSelectPresetCancel or UiAction.ModSelectPresetDeleteCancel => Do(_modSelect.CancelPresetDialog),
            UiAction.ModSelectPresetDeleteConfirm => Do(_modSelect.ConfirmPresetDelete),
            UiAction.ModSelectPresetBackdrop => true,
            _ => false,
        } || (UiActionGroups.TryGetModSelectPresetIndex(action, out int presetIndex) && Do(() => _modSelect.ActivatePreset(presetIndex))));
#pragma warning restore IDE0072

    private void BackFromModSelect()
    {
        _modSelect.ClosePresetDialog();
        _textInputService.HideTextInput();
        _activeScene = ActiveScene.SongSelect;
    }
}
