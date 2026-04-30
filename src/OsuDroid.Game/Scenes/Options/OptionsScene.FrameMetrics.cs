using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private float CalculateContentHeight(IReadOnlyList<SettingsCategory> categories) =>
        categories.Sum(category => CategoryTopMargin + CalculateCategoryHeight(category));

    private static float CalculateSectionHeight() =>
        32f * DpScale + s_sections.Length * SectionStep + 32f * DpScale;

    private float CalculateCategoryHeight(SettingsCategory category) =>
        CategoryHeaderHeight + category.Rows.Sum(GetRowHeight);

    private static bool IsLongSummarySlider(SettingsRow row) =>
        row.Key == "gameAudioSynchronizationThreshold";

    private float GetRowHeight(SettingsRow row) =>
        row.Kind == SettingsRowKind.Input ? InputRowHeight
        : row.Kind == SettingsRowKind.Slider ? SliderBaseHeight(row, RowTextWidth(row))
        : Math.Max(RowHeight, TextBlockHeight(row, RowTextWidth(row)) + RowPadding);

    private float SliderBaseHeight(SettingsRow row, float textWidth)
    {
        int summaryLineCount = SummaryLineCount(row, textWidth);
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

    private float ExtraSummaryHeight(SettingsRow row, float width)
    {
        int lines = SummaryLineCount(row, width);
        return Math.Max(0, lines - 1) * SummaryLineHeight;
    }

    private float TextBlockHeight(SettingsRow row, float width) =>
        RowTitleSize
        + 4f
        + 6f * DpScale
        + Math.Max(1, SummaryLineCount(row, width)) * SummaryLineHeight;

    private float RowTextWidth(SettingsRow row)
    {
        float listWidth = ActiveListWidth(_lastViewport);
        float reservedControlWidth =
            row.Kind == SettingsRowKind.Input || row.Kind == SettingsRowKind.Slider ? 0f
            : row.Kind == SettingsRowKind.Select ? 150f * DpScale
            : 96f * DpScale;
        return Math.Max(80f * DpScale, listWidth - RowPadding * 3f - reservedControlWidth);
    }

    private static float ActiveListWidth(VirtualViewport viewport)
    {
        float listX = ContentPaddingX + SectionRailWidth + ListGap;
        return viewport.VirtualWidth - listX - ContentPaddingX;
    }

    private int SummaryLineCount(SettingsRow row, float width)
    {
        string summaryText = GetSummaryText(row);
        return string.IsNullOrWhiteSpace(summaryText)
            ? 1
            : Math.Max(
                1,
                WrapText(summaryText, RowSummarySize, width).Count(line => line.Length > 0)
            );
    }

    private const float SummaryLineHeight = RowSummarySize + 4f;

    private static UiAction RowAction(SettingsRow row, int index) =>
        row.Action == UiAction.None
            ? (UiAction)((int)UiAction.OptionsActiveRow0 + index)
            : row.Action;

    private static float VisibleContentHeight(VirtualViewport viewport) =>
        Math.Max(0f, viewport.VirtualHeight - ContentTop);

    private static bool IsVisible(UiRect bounds, VirtualViewport viewport) =>
        bounds.Bottom >= AppBarHeight && bounds.Y <= viewport.VirtualHeight;

    private static UiElementSnapshot Fill(
        string id,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        float cornerRadius = 0f,
        bool enabled = true,
        UiCornerMode cornerMode = UiCornerMode.All
    ) => UiElementFactory.Fill(id, bounds, color, alpha, action, cornerRadius, enabled, cornerMode);

    private static UiElementSnapshot Sprite(
        string id,
        string assetName,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        bool enabled = true
    ) => UiElementFactory.Sprite(id, assetName, bounds, color, alpha, action, enabled);

    private static UiElementSnapshot MaterialIcon(
        string id,
        UiMaterialIcon icon,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        bool enabled = true
    ) => UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action, enabled);

    private static UiElementSnapshot Icon(
        string id,
        UiIcon icon,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        bool enabled = true
    ) => UiElementFactory.Icon(id, icon, bounds, color, alpha, action, enabled);

    private static UiElementSnapshot Text(
        string id,
        string value,
        float x,
        float y,
        float width,
        float height,
        float size,
        UiColor color,
        float alpha = 1f,
        bool bold = false,
        UiAction action = UiAction.None,
        bool enabled = true,
        UiTextAlignment alignment = UiTextAlignment.Left,
        bool clipToBounds = false
    ) =>
        UiElementFactory.Text(
            id,
            value,
            new UiRect(x, y, width, height),
            size,
            color,
            action,
            enabled,
            bold,
            alignment,
            alpha: alpha,
            clipToBounds: clipToBounds
        );
}
