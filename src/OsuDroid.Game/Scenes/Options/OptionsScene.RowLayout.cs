using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private void AddRow(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        bool isInteractive = IsInteractive(row);
        float rowAlpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.5f;
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        float rowCornerRadius = row.IsBottom ? AndroidRoundedRectRadius : 0f;
        elements.Add(
            Fill(
                $"options-row-{index}",
                bounds,
                s_rowBackground,
                rowAlpha,
                rowAction,
                rowCornerRadius,
                isInteractive,
                row.IsBottom ? UiCornerMode.Bottom : UiCornerMode.None
            )
        );

        if (row.Kind == SettingsRowKind.Slider)
        {
            AddSliderControl(elements, row, index, bounds);
            AddLockOverlay(elements, row, index, bounds);
            return;
        }

        float titleHeight = RowTitleSize + 4f;
        string summaryText = GetSummaryText(row);
        float reservedControlWidth =
            row.Kind == SettingsRowKind.Slider ? 0f
            : row.Kind == SettingsRowKind.Select ? 0f
            : 96f * DpScale;
        float textWidth =
            row.Kind == SettingsRowKind.Input
                ? bounds.Width - RowPadding * 2f
                : Math.Max(80f * DpScale, bounds.Width - RowPadding * 3f - reservedControlWidth);
        float summaryHeight = SummaryLineCount(row, textWidth) * SummaryLineHeight;
        float textBlockHeight = titleHeight + summaryHeight + 6f * DpScale;
        float textTop = row.Kind is SettingsRowKind.Input or SettingsRowKind.Slider
            ? bounds.Y + RowPadding
            : bounds.Y + (bounds.Height - textBlockHeight) / 2f;
        UiColor textColor = row.IsEnabled ? s_disabledWhite : s_secondaryText;
        elements.Add(
            Text(
                $"options-row-{index}-label",
                _localizer[row.TitleKey],
                bounds.X + RowPadding,
                textTop,
                textWidth,
                titleHeight,
                RowTitleSize,
                textColor,
                rowAlpha * 0.94f,
                true,
                rowAction,
                isInteractive,
                clipToBounds: false
            )
        );
        if (!string.IsNullOrEmpty(summaryText))
        {
            if (row.Kind == SettingsRowKind.Input)
            {
                elements.Add(
                    Text(
                        $"options-row-{index}-summary",
                        summaryText,
                        bounds.X + RowPadding,
                        textTop + titleHeight + 6f * DpScale,
                        textWidth,
                        SummaryLineHeight,
                        RowSummarySize,
                        s_secondaryText,
                        rowAlpha * 0.86f,
                        false,
                        rowAction,
                        isInteractive,
                        clipToBounds: true
                    )
                );
            }
            else
            {
                AddWrappedSummary(
                    elements,
                    $"options-row-{index}-summary",
                    summaryText,
                    bounds.X + RowPadding,
                    textTop + titleHeight + 6f * DpScale,
                    textWidth,
                    rowAlpha * 0.86f,
                    rowAction,
                    isInteractive
                );
            }
        }

        if (row.Kind == SettingsRowKind.Checkbox)
        {
            AddCheckbox(elements, row, index, bounds);
        }
        else if (row.Kind == SettingsRowKind.Input)
        {
            AddInputControl(elements, row, index, bounds);
        }
        else if (row.Kind == SettingsRowKind.Button)
        {
            elements.Add(
                MaterialIcon(
                    $"options-row-{index}-chevron",
                    UiMaterialIcon.ChevronRight,
                    new UiRect(
                        bounds.Right - RowPadding - SectionIconSize,
                        bounds.Y + (bounds.Height - SectionIconSize) / 2f,
                        SectionIconSize,
                        SectionIconSize
                    ),
                    s_secondaryText,
                    rowAlpha * 0.8f,
                    rowAction,
                    isInteractive
                )
            );
        }

        AddLockOverlay(elements, row, index, bounds);
    }
}
