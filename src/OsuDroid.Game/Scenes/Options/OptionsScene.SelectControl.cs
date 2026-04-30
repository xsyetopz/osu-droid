using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private void AddSelectControl(
        List<UiElementSnapshot> elements,
        SettingsRow row,
        int index,
        UiRect bounds
    )
    {
        var chevron = new UiRect(
            bounds.Right - RowPadding - SectionIconSize,
            bounds.Y + (bounds.Height - SectionIconSize) / 2f,
            SectionIconSize,
            SectionIconSize
        );
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.65f : 0.9f) : 0.45f;
        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        string value = GetSelectValue(row);
        if (!string.IsNullOrEmpty(value))
        {
            float valueWidth = 86f * DpScale;
            elements.Add(
                Text(
                    $"options-row-{index}-value",
                    value,
                    chevron.X - 12f * DpScale - valueWidth,
                    bounds.Y + (bounds.Height - RowTitleSize - 4f) / 2f,
                    valueWidth,
                    RowTitleSize + 4f,
                    RowTitleSize,
                    s_secondaryText,
                    alpha,
                    false,
                    rowAction,
                    isInteractive,
                    UiTextAlignment.Right,
                    clipToBounds: true
                )
            );
        }

        elements.Add(
            MaterialIcon(
                $"options-row-{index}-dropdown",
                UiMaterialIcon.ArrowDropDown,
                chevron,
                s_secondaryText,
                alpha,
                rowAction,
                isInteractive
            )
        );
    }
}
