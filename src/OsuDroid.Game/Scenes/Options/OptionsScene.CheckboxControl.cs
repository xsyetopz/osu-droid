using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private void AddCheckbox(
        List<UiElementSnapshot> elements,
        SettingsRow row,
        int index,
        UiRect bounds
    )
    {
        var checkbox = new UiRect(
            bounds.Right - RowPadding - SectionIconSize,
            bounds.Y + (bounds.Height - SectionIconSize) / 2f,
            SectionIconSize,
            SectionIconSize
        );
        bool isChecked = GetBoolValue(row.Key);
        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.55f;
        if (isChecked)
        {
            elements.Add(
                Fill(
                    $"options-row-{index}-checkbox-box",
                    checkbox,
                    s_checkboxAccent,
                    alpha,
                    rowAction,
                    2f * DpScale,
                    isInteractive
                )
            );
            elements.Add(
                MaterialIcon(
                    $"options-row-{index}-checkbox",
                    UiMaterialIcon.Check,
                    checkbox,
                    DroidUiColors.DarkText,
                    alpha,
                    rowAction,
                    isInteractive
                )
            );
        }
        else
        {
            elements.Add(
                MaterialIcon(
                    $"options-row-{index}-checkbox",
                    UiMaterialIcon.CheckboxBlankOutline,
                    checkbox,
                    s_secondaryText,
                    alpha,
                    rowAction,
                    isInteractive
                )
            );
        }
    }
}
