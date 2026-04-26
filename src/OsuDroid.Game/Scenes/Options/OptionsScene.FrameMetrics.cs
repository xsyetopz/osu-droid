using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private static float CalculateContentHeight(IReadOnlyList<SettingsCategory> categories) =>
        categories.Sum(category => CategoryTopMargin + CalculateCategoryHeight(category));

    private static float CalculateSectionHeight() =>
        32f * DpScale + s_sections.Length * SectionStep + 32f * DpScale;

    private static float CalculateCategoryHeight(SettingsCategory category) =>
        CategoryHeaderHeight + category.Rows.Sum(GetRowHeight);

    private static bool IsLongSummarySlider(SettingsRow row) =>
        row.Key == "gameAudioSynchronizationThreshold";

    private static float GetRowHeight(SettingsRow row) =>
        row.Kind == SettingsRowKind.Input ? InputRowHeight
        : row.Kind == SettingsRowKind.Slider
            ? (IsLongSummarySlider(row) ? LongSliderRowHeight : SliderRowHeight)
        : RowHeight;

    private static UiAction RowAction(SettingsRow row, int index) =>
        row.Action == UiAction.None ? (UiAction)((int)UiAction.OptionsRow0 + index) : row.Action;

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
        UiTextAlignment alignment = UiTextAlignment.Left
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
            alpha: alpha
        );
}
