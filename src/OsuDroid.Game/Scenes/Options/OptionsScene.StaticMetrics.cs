using OsuDroid.Game.Localization;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private static float CalculateStaticContentHeight(
        IReadOnlyList<SettingsCategory> categories,
        VirtualViewport viewport
    ) =>
        categories.Sum(category =>
            CategoryTopMargin + CalculateStaticCategoryHeight(category, viewport)
        );

    private static float CalculateStaticCategoryHeight(
        SettingsCategory category,
        VirtualViewport viewport
    ) => CategoryHeaderHeight + category.Rows.Sum(row => GetStaticRowHeight(row, viewport));

    private static float GetStaticRowHeight(SettingsRow row, VirtualViewport viewport)
    {
        float width = StaticRowTextWidth(row, viewport);
        return row.Kind == SettingsRowKind.Input ? InputRowHeight
            : row.Kind == SettingsRowKind.Slider ? StaticSliderBaseHeight(row, width)
            : Math.Max(RowHeight, StaticTextBlockHeight(row, width) + RowPadding);
    }

    private static float StaticSliderBaseHeight(SettingsRow row, float textWidth)
    {
        int summaryLineCount = StaticSummaryLineCount(row, textWidth);
        float summaryHeight =
            Math.Max(1, summaryLineCount) * SliderSummaryLineHeight
            + (summaryLineCount > 1 ? SliderSummaryParagraphGap : 0f);

        return RowPadding * 2f
            + RowTitleSize
            + 4f
            + 6f * DpScale
            + summaryHeight
            + SeekbarTopMargin
            + SeekbarThumbSize;
    }

    private static float StaticTextBlockHeight(SettingsRow row, float width) =>
        RowTitleSize
        + 4f
        + 6f * DpScale
        + Math.Max(1, StaticSummaryLineCount(row, width)) * SummaryLineHeight;

    private static float StaticRowTextWidth(SettingsRow row, VirtualViewport viewport)
    {
        float listWidth = ActiveListWidth(viewport);
        float reservedControlWidth =
            row.Kind is SettingsRowKind.Input or SettingsRowKind.Slider ? 0f
            : row.Kind == SettingsRowKind.Select ? 0f
            : 96f * DpScale;
        return Math.Max(80f * DpScale, listWidth - RowPadding * 3f - reservedControlWidth);
    }

    private static int StaticSummaryLineCount(SettingsRow row, float width)
    {
        string summaryText = new GameLocalizer().Get(row.SummaryKey);
        return string.IsNullOrWhiteSpace(summaryText)
            ? 1
            : Math.Max(
                1,
                WrapText(summaryText, RowSummarySize, width).Count(line => line.Length > 0)
            );
    }
}
