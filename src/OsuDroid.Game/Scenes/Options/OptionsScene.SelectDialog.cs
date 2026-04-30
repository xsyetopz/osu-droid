using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private const int MaxSelectDialogOptions = 16;

    private string GetSelectValue(SettingsRow row)
    {
        return row.ValueKeys is { Count: > 0 } valueKeys
                ? _localizer[valueKeys[ClampSelectValue(row, GetIntValue(row.Key))]]
            : row.ValueKey is null ? string.Empty
            : _localizer[row.ValueKey];
    }

    private void OpenSelectDialog(int rowIndex)
    {
        SettingsRow? row = ActiveRowAt(rowIndex);
        if (row?.Kind != SettingsRowKind.Select)
        {
            return;
        }

        _activeSelectRowIndex = rowIndex;
    }

    private bool TryHandleSelectDialogAction(UiAction action)
    {
        if (_activeSelectRowIndex is null)
        {
            return false;
        }

        if (action == UiAction.OptionsSelectDialogBackdrop)
        {
            CloseSelectDialog();
            return true;
        }

        if (!UiActionGroups.TryGetOptionsSelectDialogOptionIndex(action, out int optionIndex))
        {
            return false;
        }

        SettingsRow? row = ActiveSelectRow;
        if (row is not null)
        {
            SelectOption(row, optionIndex);
            _pendingSfxKey = "click-short";
        }

        CloseSelectDialog();
        return true;
    }

    public bool CloseSelectDialog()
    {
        if (_activeSelectRowIndex is null)
        {
            return false;
        }

        _activeSelectRowIndex = null;
        return true;
    }

    private SettingsRow? ActiveSelectRow =>
        _activeSelectRowIndex is int rowIndex ? ActiveRowAt(rowIndex) : null;

    private void AddSelectDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        SettingsRow? row = ActiveSelectRow;
        if (row?.Kind != SettingsRowKind.Select || row.ValueKeys is not { Count: > 0 } valueKeys)
        {
            return;
        }

        elements.Add(
            Fill(
                "options-select-dialog-backdrop",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                DroidUiColors.Black,
                0.62f,
                UiAction.OptionsSelectDialogBackdrop
            )
        );

        int optionCount = Math.Min(valueKeys.Count, MaxSelectDialogOptions);
        float dialogWidth = Math.Min(560f * DpScale, viewport.VirtualWidth - 96f * DpScale);
        float titleHeight = 48f * DpScale;
        float optionHeight = 56f * DpScale;
        float dialogHeight = titleHeight + optionCount * optionHeight;
        var dialogBounds = new UiRect(
            (viewport.VirtualWidth - dialogWidth) / 2f,
            (viewport.VirtualHeight - dialogHeight) / 2f,
            dialogWidth,
            dialogHeight
        );

        elements.Add(
            Fill(
                "options-select-dialog",
                dialogBounds,
                s_selectedSection,
                1f,
                UiAction.None,
                AndroidRoundedRectRadius
            )
        );
        elements.Add(
            Text(
                "options-select-dialog-title",
                _localizer[row.TitleKey],
                dialogBounds.X,
                dialogBounds.Y + (titleHeight - RowSummarySize) / 2f,
                dialogBounds.Width,
                RowSummarySize + 4f,
                RowSummarySize,
                s_secondaryText,
                1f,
                true,
                UiAction.None,
                true,
                UiTextAlignment.Center
            )
        );

        int selectedIndex = ClampSelectValue(row, GetIntValue(row.Key));
        for (int index = 0; index < optionCount; index++)
        {
            AddSelectDialogOption(elements, valueKeys[index], index, selectedIndex, dialogBounds);
        }
    }

    private void AddSelectDialogOption(
        List<UiElementSnapshot> elements,
        string valueKey,
        int optionIndex,
        int selectedIndex,
        UiRect dialogBounds
    )
    {
        UiActionGroups.TryGetOptionsSelectDialogOptionAction(optionIndex, out UiAction action);
        float titleHeight = 48f * DpScale;
        float optionHeight = 56f * DpScale;
        var bounds = new UiRect(
            dialogBounds.X + 8f * DpScale,
            dialogBounds.Y + titleHeight + optionIndex * optionHeight,
            dialogBounds.Width - 16f * DpScale,
            optionHeight
        );
        bool isSelected = optionIndex == selectedIndex;

        elements.Add(
            Fill(
                $"options-select-dialog-option-{optionIndex}",
                bounds,
                isSelected ? s_checkboxAccent : s_selectedSection,
                isSelected ? 0.32f : 0f,
                action,
                isSelected ? AndroidSidebarRadius : 0f
            )
        );
        elements.Add(
            Text(
                $"options-select-dialog-option-{optionIndex}-text",
                _localizer[valueKey],
                bounds.X + 16f * DpScale,
                bounds.Y + (bounds.Height - RowTitleSize) / 2f,
                bounds.Width - 72f * DpScale,
                RowTitleSize + 4f,
                RowTitleSize,
                s_white,
                1f,
                false,
                action,
                true,
                UiTextAlignment.Left,
                clipToBounds: true
            )
        );

        if (!isSelected)
        {
            return;
        }

        var checkBounds = new UiRect(
            bounds.Right - 16f * DpScale - SectionIconSize,
            bounds.Y + (bounds.Height - SectionIconSize) / 2f,
            SectionIconSize,
            SectionIconSize
        );
        elements.Add(
            MaterialIcon(
                $"options-select-dialog-option-{optionIndex}-check",
                UiMaterialIcon.Check,
                checkBounds,
                s_checkboxAccent,
                1f,
                action
            )
        );
    }
}
