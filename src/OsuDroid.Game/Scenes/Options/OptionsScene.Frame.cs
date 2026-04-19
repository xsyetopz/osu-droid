using System.Globalization;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class OptionsScene
{
    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport, SettingsSection sectionData, float activeContentScrollOffset, float activeSectionScrollOffset)
    {
        var elements = new List<UiElementSnapshot>
        {
            Fill("options-root", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), rootBackground),
        };

        AddActiveList(elements, viewport, sectionData, activeContentScrollOffset);
        AddSections(elements, viewport, sectionData.Section, activeSectionScrollOffset);
        AddAppBar(elements, viewport);

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private static void AddAppBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(Fill("options-appbar", new UiRect(0f, 0f, viewport.VirtualWidth, AppBarHeight), appBarBackground));
        elements.Add(Fill("options-back-hit", new UiRect(0f, 0f, AppBarHeight, AppBarHeight), selectedSection, 1f, UiAction.OptionsBack));
        elements.Add(MaterialIcon("options-back", UiMaterialIcon.ArrowBack, new UiRect(16f * AndroidDpScale, 16f * AndroidDpScale, SectionIconSize, SectionIconSize), white, 1f, UiAction.OptionsBack));
    }

    private void AddSections(List<UiElementSnapshot> elements, VirtualViewport viewport, OptionsSection selectedSectionValue, float activeSectionScrollOffset)
    {
        for (var i = 0; i < sections.Length; i++)
        {
            var section = sections[i];
            var isSelected = section.Section == selectedSectionValue;
            var y = ContentTop + i * SectionStep - activeSectionScrollOffset;
            var bounds = new UiRect(ContentPaddingX, y, SectionRailWidth, SectionHeight);
            if (!IsVisible(bounds, viewport))
                continue;

            if (isSelected)
                elements.Add(Fill("options-section-selected", bounds, selectedSection, 1f, section.Action, AndroidSidebarRadius));
            else
                elements.Add(Fill($"options-section-{i}-hit", bounds, UiColor.Opaque(0, 0, 0), 0f, section.Action));

            var textColor = isSelected ? white : disabledWhite;
            var iconBounds = new UiRect(bounds.X + SectionPadding, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
            var textX = iconBounds.Right + SectionDrawablePadding;
            elements.Add(MaterialIcon($"options-section-{i}-icon", section.Icon, iconBounds, textColor, isSelected ? 1f : 0.9f, section.Action));
            elements.Add(Text($"options-section-{i}-text", localizer[section.Key], textX, bounds.Y + (bounds.Height - RowTitleSize) / 2f, bounds.Right - textX - SectionPadding, RowTitleSize + 4f, RowTitleSize, textColor, isSelected ? 1f : 0.9f, true, section.Action));
        }
    }

    private void AddActiveList(List<UiElementSnapshot> elements, VirtualViewport viewport, SettingsSection sectionData, float activeContentScrollOffset)
    {
        var listX = ContentPaddingX + SectionRailWidth + ListGap;
        var listWidth = viewport.VirtualWidth - listX - ContentPaddingX;
        var y = ContentTop - activeContentScrollOffset;
        var rowIndex = 0;

        foreach (var category in sectionData.Categories)
        {
            y += CategoryTopMargin;
            var categoryTop = y;
            var categoryHeight = CalculateCategoryHeight(category);
            var categoryBounds = new UiRect(listX, categoryTop, listWidth, categoryHeight);

            if (IsVisible(categoryBounds, viewport))
            {
                var categoryHeaderBounds = new UiRect(listX, categoryTop, listWidth, CategoryHeaderHeight);
                elements.Add(Fill($"options-category-{rowIndex}-header", categoryHeaderBounds, rowBackground, 1f, UiAction.None, AndroidRoundedRectRadius, true, UiCornerMode.Top));
                elements.Add(Text($"options-category-{rowIndex}-title", localizer[category.TitleKey], listX + RowPadding, categoryTop + (CategoryHeaderHeight - RowSummarySize) / 2f, listWidth - RowPadding * 2f, RowSummarySize + 4f, RowSummarySize, secondaryText, 0.95f, true));
            }

            var rowY = categoryTop + CategoryHeaderHeight;
            foreach (var row in category.Rows)
            {
                var rowHeight = GetRowHeight(row);
                var rowBounds = new UiRect(listX, rowY, listWidth, rowHeight);
                if (IsVisible(rowBounds, viewport))
                    AddRow(elements, row, rowIndex, rowBounds);

                rowY += rowHeight;
                rowIndex++;
            }

            y += categoryHeight;
        }
    }

    private void AddRow(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var rowAlpha = row.IsEnabled ? 1f : 0.5f;
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        var rowCornerRadius = row.IsBottom ? AndroidRoundedRectRadius : 0f;
        elements.Add(Fill($"options-row-{index}", bounds, rowBackground, rowAlpha, rowAction, rowCornerRadius, row.IsEnabled, row.IsBottom ? UiCornerMode.Bottom : UiCornerMode.None));

        if (row.Kind == SettingsRowKind.Slider)
        {
            AddSliderControl(elements, row, index, bounds);
            return;
        }

        var titleHeight = RowTitleSize + 4f;
        var summaryHeight = RowSummarySize + 4f;
        var textBlockHeight = titleHeight + summaryHeight + 6f * AndroidDpScale;
        var textTop = row.Kind is SettingsRowKind.Input or SettingsRowKind.Slider
            ? bounds.Y + RowPadding
            : bounds.Y + (bounds.Height - textBlockHeight) / 2f;
        var reservedControlWidth = row.Kind switch
        {
            SettingsRowKind.Slider => 0f,
            SettingsRowKind.Select => 150f * AndroidDpScale,
            _ => 96f * AndroidDpScale,
        };
        var textWidth = row.Kind == SettingsRowKind.Input
            ? bounds.Width - RowPadding * 2f
            : Math.Max(80f * AndroidDpScale, bounds.Width - RowPadding * 3f - reservedControlWidth);
        var textColor = row.IsEnabled ? disabledWhite : secondaryText;
        elements.Add(Text($"options-row-{index}-label", localizer[row.TitleKey], bounds.X + RowPadding, textTop, textWidth, titleHeight, RowTitleSize, textColor, rowAlpha * 0.94f, true, rowAction, row.IsEnabled));
        if (!string.IsNullOrEmpty(localizer[row.SummaryKey]))
            elements.Add(Text($"options-row-{index}-summary", localizer[row.SummaryKey], bounds.X + RowPadding, textTop + titleHeight + 6f * AndroidDpScale, textWidth, summaryHeight, RowSummarySize, secondaryText, rowAlpha * 0.86f, false, rowAction, row.IsEnabled));

        switch (row.Kind)
        {
            case SettingsRowKind.Checkbox:
                AddCheckbox(elements, row, index, bounds);
                break;

            case SettingsRowKind.Select:
                AddSelectControl(elements, row, index, bounds);
                break;

            case SettingsRowKind.Input:
                AddInputControl(elements, row, index, bounds);
                break;

            case SettingsRowKind.Button:
                elements.Add(MaterialIcon($"options-row-{index}-chevron", UiMaterialIcon.ChevronRight, new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize), secondaryText, rowAlpha * 0.8f, rowAction, row.IsEnabled));
                break;

        }
    }

    private void AddCheckbox(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var checkbox = new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
        var isChecked = GetBoolValue(row.Key);
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        var alpha = row.IsEnabled ? 1f : 0.55f;
        if (isChecked)
        {
            elements.Add(Fill($"options-row-{index}-checkbox-box", checkbox, checkboxAccent, alpha, rowAction, 2f * AndroidDpScale, row.IsEnabled));
            elements.Add(MaterialIcon($"options-row-{index}-checkbox", UiMaterialIcon.Check, checkbox, UiColor.Opaque(32, 32, 46), alpha, rowAction, row.IsEnabled));
        }
        else
        {
            elements.Add(MaterialIcon($"options-row-{index}-checkbox", UiMaterialIcon.CheckboxBlankOutline, checkbox, secondaryText, alpha, rowAction, row.IsEnabled));
        }
    }

    private void AddSelectControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var chevron = new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
        var alpha = row.IsEnabled ? 0.9f : 0.45f;
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        if (row.ValueKey is not null)
        {
            var valueWidth = 86f * AndroidDpScale;
            elements.Add(Text($"options-row-{index}-value", localizer[row.ValueKey], chevron.X - 12f * AndroidDpScale - valueWidth, bounds.Y + (bounds.Height - RowTitleSize - 4f) / 2f, valueWidth, RowTitleSize + 4f, RowTitleSize, secondaryText, alpha, false, rowAction, row.IsEnabled));
        }

        elements.Add(MaterialIcon($"options-row-{index}-dropdown", UiMaterialIcon.ArrowDropDown, chevron, secondaryText, alpha, rowAction, row.IsEnabled));
    }

    private void AddInputControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        var alpha = row.IsEnabled ? 1f : 0.5f;
        var inputBounds = new UiRect(bounds.X + RowPadding, bounds.Y + RowPadding + RowTitleSize + 4f + 6f * AndroidDpScale + RowSummarySize + 4f + InputGap, bounds.Width - RowPadding * 2f, InputHeight);
        elements.Add(Fill($"options-row-{index}-input", inputBounds, inputBackground, alpha, rowAction, AndroidRoundedRectRadius, row.IsEnabled));
        if (row.ValueKey is not null)
            elements.Add(Text($"options-row-{index}-input-value", localizer[row.ValueKey], inputBounds.X + 14f * AndroidDpScale, inputBounds.Y + 8f * AndroidDpScale, inputBounds.Width - 28f * AndroidDpScale, RowTitleSize + 4f, RowTitleSize, UiColor.Opaque(235, 235, 245), 0.85f * alpha, false, rowAction, row.IsEnabled));
    }

    private void AddSliderControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var value = GetIntValue(row.Key);
        var normalized = row.Max == row.Min ? 0f : Math.Clamp((value - row.Min) / (float)(row.Max - row.Min), 0f, 1f);
        var alpha = row.IsEnabled ? 1f : 0.5f;
        var titleHeight = RowTitleSize + 4f;
        var summaryHeight = RowSummarySize + 4f;
        var containerX = bounds.X + SeekbarContainerMarginX;
        var containerWidth = bounds.Width - SeekbarContainerMarginX * 2f;
        var valueWidth = 72f * AndroidDpScale;
        var controlWidth = Math.Min(ControlColumnWidth, Math.Max(96f * AndroidDpScale, containerWidth * 0.44f));
        var textWidth = Math.Max(80f * AndroidDpScale, containerWidth - controlWidth - ControlGap);
        var textTop = bounds.Y + RowPadding;
        var summaryTop = textTop + titleHeight + 6f * AndroidDpScale;
        var valueTop = textTop;
        var trackWidth = controlWidth;
        var trackX = bounds.Right - SeekbarContainerMarginX - trackWidth;
        var trackY = summaryTop + summaryHeight + SeekbarTopMargin + (SeekbarThumbSize - SeekbarTrackHeight) / 2f;
        var thumbX = trackX + trackWidth * normalized - SeekbarThumbSize / 2f;
        var thumbY = trackY + SeekbarTrackHeight / 2f - SeekbarThumbSize / 2f;

        elements.Add(Text($"options-row-{index}-label", localizer[row.TitleKey], containerX, textTop, textWidth, titleHeight, RowTitleSize, disabledWhite, alpha * 0.94f, true, UiAction.None, row.IsEnabled));
        if (!string.IsNullOrEmpty(localizer[row.SummaryKey]))
            elements.Add(Text($"options-row-{index}-summary", localizer[row.SummaryKey], containerX, summaryTop, textWidth, summaryHeight, RowSummarySize, secondaryText, alpha * 0.86f, false, UiAction.None, row.IsEnabled));

        elements.Add(Text($"options-row-{index}-value", value.ToString(CultureInfo.InvariantCulture), trackX + trackWidth - valueWidth, valueTop, valueWidth, titleHeight, RowTitleSize, secondaryText, 0.9f * alpha, false, UiAction.None, row.IsEnabled));
        elements.Add(Fill($"options-row-{index}-slider-track", new UiRect(trackX, trackY, trackWidth, SeekbarTrackHeight), sliderTrack, alpha, UiAction.None, 12f * AndroidDpScale, row.IsEnabled));
        elements.Add(Fill($"options-row-{index}-slider-fill", new UiRect(trackX, trackY, trackWidth * normalized, SeekbarTrackHeight), checkboxAccent, alpha, UiAction.None, 12f * AndroidDpScale, row.IsEnabled));
        elements.Add(Fill($"options-row-{index}-slider-thumb", new UiRect(thumbX, thumbY, SeekbarThumbSize, SeekbarThumbSize), white, alpha, UiAction.None, 12f * AndroidDpScale, row.IsEnabled));
    }

    private static float CalculateContentHeight(IReadOnlyList<SettingsCategory> categories) => categories.Sum(category => CategoryTopMargin + CalculateCategoryHeight(category));

    private static float CalculateSectionHeight() => 32f * AndroidDpScale + sections.Length * SectionStep + 32f * AndroidDpScale;

    private static float CalculateCategoryHeight(SettingsCategory category) => CategoryHeaderHeight + category.Rows.Sum(GetRowHeight);

    private static float GetRowHeight(SettingsRow row) => row.Kind switch
    {
        SettingsRowKind.Input => InputRowHeight,
        SettingsRowKind.Slider => SliderRowHeight,
        _ => RowHeight,
    };

    private static float VisibleContentHeight(VirtualViewport viewport) => Math.Max(0f, viewport.VirtualHeight - ContentTop);

    private static bool IsVisible(UiRect bounds, VirtualViewport viewport) => bounds.Bottom >= AppBarHeight && bounds.Y <= viewport.VirtualHeight;

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float cornerRadius = 0f, bool enabled = true, UiCornerMode cornerMode = UiCornerMode.All) => new(
        id,
        UiElementKind.Fill,
        bounds,
        color,
        alpha,
        null,
        action,
        null,
        null,
        enabled,
        null,
        cornerRadius,
        null,
        cornerMode);

    private static UiElementSnapshot Sprite(string id, string assetName, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) => new(
        id,
        UiElementKind.Sprite,
        bounds,
        color,
        alpha,
        assetName,
        action,
        null,
        null,
        enabled);


    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) => new(
        id,
        UiElementKind.MaterialIcon,
        bounds,
        color,
        alpha,
        null,
        action,
        null,
        null,
        enabled,
        null,
        0f,
        icon);

    private static UiElementSnapshot Icon(string id, UiIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) => new(
        id,
        UiElementKind.Icon,
        bounds,
        color,
        alpha,
        null,
        action,
        null,
        null,
        enabled,
        icon);

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
        bool enabled = true) => new(
            id,
            UiElementKind.Text,
            new UiRect(x, y, width, height),
            color,
            alpha,
            null,
            action,
            value,
            new UiTextStyle(size, bold),
            enabled);
}
