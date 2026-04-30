using System.Globalization;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private void AddSliderControl(
        List<UiElementSnapshot> elements,
        SettingsRow row,
        int index,
        UiRect bounds
    )
    {
        int value = GetIntValue(row.Key);
        float normalized =
            row.Max == row.Min
                ? 0f
                : Math.Clamp((value - row.Min) / (float)(row.Max - row.Min), 0f, 1f);
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.5f;
        float textAlpha = row.IsEnabled ? 1f : 0.5f;
        float titleHeight = RowTitleSize + 4f;
        int summaryLineCount = SummaryLineCount(row, bounds.Width);
        float summaryHeight =
            Math.Max(1, summaryLineCount) * SliderSummaryLineHeight
            + (summaryLineCount > 1 ? SliderSummaryParagraphGap : 0f);
        float containerX = bounds.X + SeekbarContainerMarginX;
        float containerWidth = bounds.Width - SeekbarContainerMarginX * 2f;
        float valueWidth = 72f * DpScale;
        float textWidth = Math.Max(80f * DpScale, containerWidth - valueWidth - ControlGap);
        float textTop = bounds.Y + RowPadding;
        float summaryTop = textTop + titleHeight + 6f * DpScale;
        float valueTop = textTop;
        float trackWidth = bounds.Width - SeekbarTrackMarginX * 2f;
        float trackX = bounds.X + SeekbarTrackMarginX;
        float trackY =
            summaryTop
            + summaryHeight
            + SeekbarTopMargin
            + (SeekbarThumbSize - SeekbarTrackHeight) / 2f;
        float thumbX = trackX + trackWidth * normalized - SeekbarThumbSize / 2f;
        float thumbY = trackY + SeekbarTrackHeight / 2f - SeekbarThumbSize / 2f;

        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        elements.Add(
            Text(
                $"options-row-{index}-label",
                _localizer[row.TitleKey],
                containerX,
                textTop,
                textWidth,
                titleHeight,
                RowTitleSize,
                s_disabledWhite,
                textAlpha * 0.94f,
                true,
                rowAction,
                isInteractive,
                clipToBounds: false
            )
        );
        string summaryText = GetSummaryText(row);
        if (!string.IsNullOrEmpty(summaryText))
        {
            AddSliderSummary(
                elements,
                $"options-row-{index}-summary",
                summaryText,
                containerX,
                summaryTop,
                textWidth,
                textAlpha * 0.86f,
                rowAction,
                isInteractive
            );
        }

        elements.Add(
            Text(
                $"options-row-{index}-value",
                value.ToString(CultureInfo.InvariantCulture),
                containerX + containerWidth - valueWidth,
                valueTop,
                valueWidth,
                titleHeight,
                RowTitleSize,
                s_secondaryText,
                0.9f * textAlpha,
                false,
                rowAction,
                isInteractive,
                UiTextAlignment.Right,
                clipToBounds: true
            )
        );
        elements.Add(
            Fill(
                $"options-row-{index}-slider-track",
                new UiRect(trackX, trackY, trackWidth, SeekbarTrackHeight),
                s_sliderTrack,
                alpha,
                rowAction,
                12f * DpScale,
                isInteractive
            )
        );
        elements.Add(
            Fill(
                $"options-row-{index}-slider-fill",
                new UiRect(trackX, trackY, trackWidth * normalized, SeekbarTrackHeight),
                s_checkboxAccent,
                alpha,
                rowAction,
                12f * DpScale,
                isInteractive
            )
        );
        elements.Add(
            Fill(
                $"options-row-{index}-slider-thumb",
                new UiRect(thumbX, thumbY, SeekbarThumbSize, SeekbarThumbSize),
                s_white,
                alpha,
                rowAction,
                12f * DpScale,
                isInteractive
            )
        );
    }

    private static void AddSliderSummary(
        List<UiElementSnapshot> elements,
        string id,
        string summary,
        float x,
        float y,
        float width,
        float alpha,
        UiAction action,
        bool enabled
    )
    {
        int elementIndex = 0;
        float lineY = y;
        foreach (string line in WrapText(summary, RowSummarySize, width))
        {
            if (line.Length == 0)
            {
                lineY += SliderSummaryParagraphGap;
                continue;
            }

            string lineId = elementIndex == 0 ? id : $"{id}-{elementIndex}";
            elements.Add(
                Text(
                    lineId,
                    line,
                    x,
                    lineY,
                    width,
                    SliderSummaryLineHeight,
                    RowSummarySize,
                    s_secondaryText,
                    alpha,
                    false,
                    action,
                    enabled,
                    clipToBounds: true
                )
            );
            lineY += SliderSummaryLineHeight;
            elementIndex++;
        }
    }
}
