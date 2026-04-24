namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
#pragma warning disable IDE0072 // UiAction has cross-scene members; Mod Select handles only its own actions.
    private bool HandleModSelectUiAction(UiAction action, VirtualViewport viewport) =>
        _activeScene == ActiveScene.ModSelect && action switch
        {
            UiAction.ModSelectBack => Do(BackFromModSelect),
            UiAction.ModSelectClear => Do(_modSelect.Clear),
            UiAction.ModSelectCustomize => true,
            UiAction.ModSelectSearchBox => Do(() => _modSelect.FocusSearch(viewport)),
            _ => false,
        };
#pragma warning restore IDE0072

    private void BackFromModSelect()
    {
        _textInputService.HideTextInput();
        _activeScene = ActiveScene.SongSelect;
    }
}
