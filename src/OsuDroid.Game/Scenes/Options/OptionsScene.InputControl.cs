using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private void AddInputControl(
        List<UiElementSnapshot> elements,
        SettingsRow row,
        int index,
        UiRect bounds
    )
    {
        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.5f;
        var inputBounds = new UiRect(
            bounds.X + RowPadding,
            bounds.Y
                + RowPadding
                + RowTitleSize
                + 4f
                + 6f * DpScale
                + RowSummarySize
                + 4f
                + InputGap,
            bounds.Width - RowPadding * 2f,
            InputHeight
        );
        elements.Add(
            Fill(
                $"options-row-{index}-input",
                inputBounds,
                s_inputBackground,
                alpha,
                rowAction,
                AndroidRoundedRectRadius,
                isInteractive
            )
        );
        string value = GetInputDisplayValue(row);
        if (!string.IsNullOrEmpty(value))
        {
            elements.Add(
                Text(
                    $"options-row-{index}-input-value",
                    value,
                    inputBounds.X + 14f * DpScale,
                    inputBounds.Y + 8f * DpScale,
                    inputBounds.Width - 28f * DpScale,
                    RowTitleSize + 4f,
                    RowTitleSize,
                    DroidUiColors.TextDisabled,
                    0.85f * alpha,
                    false,
                    rowAction,
                    isInteractive,
                    clipToBounds: true
                )
            );
        }
    }
}
