using System.Globalization;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport, SettingsSection sectionData, float activeContentScrollOffset, float activeSectionScrollOffset)
    {
        var elements = new List<UiElementSnapshot>
        {
            Fill("options-root", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_rootBackground),
        };

        AddActiveList(elements, viewport, sectionData, activeContentScrollOffset);
        AddSections(elements, viewport, sectionData.Section, activeSectionScrollOffset);
        AddAppBar(elements, viewport);
        AddStatusMessage(elements, viewport);

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private static void AddAppBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        DroidSceneChrome.AddAppBar(elements, "options", viewport.VirtualWidth, s_appBarBackground);
        DroidSceneChrome.AddBackButton(elements, "options", UiAction.OptionsBack, s_selectedSection, s_white);
    }

    private void AddSections(List<UiElementSnapshot> elements, VirtualViewport viewport, OptionsSection selectedSectionValue, float activeSectionScrollOffset)
    {
        for (int i = 0; i < s_sections.Length; i++)
        {
            SettingsSection section = s_sections[i];
            bool isSelected = section.Section == selectedSectionValue;
            float y = ContentTop + i * SectionStep - activeSectionScrollOffset;
            var bounds = new UiRect(ContentPaddingX, y, SectionRailWidth, SectionHeight);
            if (!IsVisible(bounds, viewport))
            {
                continue;
            }

            if (isSelected)
            {
                elements.Add(Fill("options-section-selected", bounds, s_selectedSection, 1f, section.Action, AndroidSidebarRadius));
            }
            else
            {
                elements.Add(Fill($"options-section-{i}-hit", bounds, DroidUiColors.Black, 0f, section.Action));
            }

            UiColor textColor = isSelected ? s_white : s_disabledWhite;
            var iconBounds = new UiRect(bounds.X + SectionPadding, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
            float textX = iconBounds.Right + SectionDrawablePadding;
            elements.Add(MaterialIcon($"options-section-{i}-icon", section.Icon, iconBounds, textColor, isSelected ? 1f : 0.9f, section.Action));
            elements.Add(Text($"options-section-{i}-text", _localizer[section.Key], textX, bounds.Y + (bounds.Height - RowTitleSize) / 2f, bounds.Right - textX - SectionPadding, RowTitleSize + 4f, RowTitleSize, textColor, isSelected ? 1f : 0.9f, true, section.Action));
        }
    }

    private void AddActiveList(List<UiElementSnapshot> elements, VirtualViewport viewport, SettingsSection sectionData, float activeContentScrollOffset)
    {
        float listX = ContentPaddingX + SectionRailWidth + ListGap;
        float listWidth = viewport.VirtualWidth - listX - ContentPaddingX;
        float y = ContentTop - activeContentScrollOffset;
        int rowIndex = 0;

        foreach (SettingsCategory category in sectionData.Categories)
        {
            y += CategoryTopMargin;
            float categoryTop = y;
            float categoryHeight = CalculateCategoryHeight(category);
            var categoryBounds = new UiRect(listX, categoryTop, listWidth, categoryHeight);

            if (IsVisible(categoryBounds, viewport))
            {
                var categoryHeaderBounds = new UiRect(listX, categoryTop, listWidth, CategoryHeaderHeight);
                elements.Add(Fill($"options-category-{rowIndex}-header", categoryHeaderBounds, s_rowBackground, 1f, UiAction.None, AndroidRoundedRectRadius, true, UiCornerMode.Top));
                elements.Add(Text($"options-category-{rowIndex}-title", _localizer[category.TitleKey], listX + RowPadding, categoryTop + (CategoryHeaderHeight - RowSummarySize) / 2f, listWidth - RowPadding * 2f, RowSummarySize + 4f, RowSummarySize, s_secondaryText, 0.95f, true));
            }

            float rowY = categoryTop + CategoryHeaderHeight;
            foreach (SettingsRow row in category.Rows)
            {
                float rowHeight = GetRowHeight(row);
                var rowBounds = new UiRect(listX, rowY, listWidth, rowHeight);
                if (IsVisible(rowBounds, viewport))
                {
                    AddRow(elements, row, rowIndex, rowBounds);
                }

                rowY += rowHeight;
                rowIndex++;
            }

            y += categoryHeight;
        }
    }

    private void AddRow(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        bool isInteractive = IsInteractive(row);
        float rowAlpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.5f;
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        float rowCornerRadius = row.IsBottom ? AndroidRoundedRectRadius : 0f;
        elements.Add(Fill($"options-row-{index}", bounds, s_rowBackground, rowAlpha, rowAction, rowCornerRadius, isInteractive, row.IsBottom ? UiCornerMode.Bottom : UiCornerMode.None));

        if (row.Kind == SettingsRowKind.Slider)
        {
            AddSliderControl(elements, row, index, bounds);
            AddLockOverlay(elements, row, index, bounds);
            return;
        }

        float titleHeight = RowTitleSize + 4f;
        float summaryHeight = RowSummarySize + 4f;
        float textBlockHeight = titleHeight + summaryHeight + 6f * DpScale;
        float textTop = row.Kind is SettingsRowKind.Input or SettingsRowKind.Slider
            ? bounds.Y + RowPadding
            : bounds.Y + (bounds.Height - textBlockHeight) / 2f;
        float reservedControlWidth = row.Kind == SettingsRowKind.Slider
            ? 0f
            : row.Kind == SettingsRowKind.Select ? 150f * DpScale : 96f * DpScale;
        float textWidth = row.Kind == SettingsRowKind.Input
            ? bounds.Width - RowPadding * 2f
            : Math.Max(80f * DpScale, bounds.Width - RowPadding * 3f - reservedControlWidth);
        UiColor textColor = row.IsEnabled ? s_disabledWhite : s_secondaryText;
        elements.Add(Text($"options-row-{index}-label", _localizer[row.TitleKey], bounds.X + RowPadding, textTop, textWidth, titleHeight, RowTitleSize, textColor, rowAlpha * 0.94f, true, rowAction, isInteractive));
        string summaryText = GetSummaryText(row);
        if (!string.IsNullOrEmpty(summaryText))
        {
            elements.Add(Text($"options-row-{index}-summary", summaryText, bounds.X + RowPadding, textTop + titleHeight + 6f * DpScale, textWidth, summaryHeight, RowSummarySize, s_secondaryText, rowAlpha * 0.86f, false, rowAction, isInteractive));
        }

        if (row.Kind == SettingsRowKind.Checkbox)
        {
            AddCheckbox(elements, row, index, bounds);
        }
        else if (row.Kind == SettingsRowKind.Select)
        {
            AddSelectControl(elements, row, index, bounds);
        }
        else if (row.Kind == SettingsRowKind.Input)
        {
            AddInputControl(elements, row, index, bounds);
        }
        else if (row.Kind == SettingsRowKind.Button)
        {
            elements.Add(MaterialIcon($"options-row-{index}-chevron", UiMaterialIcon.ChevronRight, new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize), s_secondaryText, rowAlpha * 0.8f, rowAction, isInteractive));
        }

        AddLockOverlay(elements, row, index, bounds);
    }

    private void AddCheckbox(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var checkbox = new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
        bool isChecked = GetBoolValue(row.Key);
        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.55f;
        if (isChecked)
        {
            elements.Add(Fill($"options-row-{index}-checkbox-box", checkbox, s_checkboxAccent, alpha, rowAction, 2f * DpScale, isInteractive));
            elements.Add(MaterialIcon($"options-row-{index}-checkbox", UiMaterialIcon.Check, checkbox, DroidUiColors.DarkText, alpha, rowAction, isInteractive));
        }
        else
        {
            elements.Add(MaterialIcon($"options-row-{index}-checkbox", UiMaterialIcon.CheckboxBlankOutline, checkbox, s_secondaryText, alpha, rowAction, isInteractive));
        }
    }

    private void AddSelectControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var chevron = new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.65f : 0.9f) : 0.45f;
        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        string value = GetSelectValue(row);
        if (!string.IsNullOrEmpty(value))
        {
            float valueWidth = 86f * DpScale;
            elements.Add(Text($"options-row-{index}-value", value, chevron.X - 12f * DpScale - valueWidth, bounds.Y + (bounds.Height - RowTitleSize - 4f) / 2f, valueWidth, RowTitleSize + 4f, RowTitleSize, s_secondaryText, alpha, false, rowAction, isInteractive, UiTextAlignment.Right));
        }

        elements.Add(MaterialIcon($"options-row-{index}-dropdown", UiMaterialIcon.ArrowDropDown, chevron, s_secondaryText, alpha, rowAction, isInteractive));
    }

    private void AddInputControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.5f;
        var inputBounds = new UiRect(bounds.X + RowPadding, bounds.Y + RowPadding + RowTitleSize + 4f + 6f * DpScale + RowSummarySize + 4f + InputGap, bounds.Width - RowPadding * 2f, InputHeight);
        elements.Add(Fill($"options-row-{index}-input", inputBounds, s_inputBackground, alpha, rowAction, AndroidRoundedRectRadius, isInteractive));
        string value = GetInputDisplayValue(row);
        if (!string.IsNullOrEmpty(value))
        {
            elements.Add(Text($"options-row-{index}-input-value", value, inputBounds.X + 14f * DpScale, inputBounds.Y + 8f * DpScale, inputBounds.Width - 28f * DpScale, RowTitleSize + 4f, RowTitleSize, DroidUiColors.TextDisabled, 0.85f * alpha, false, rowAction, isInteractive));
        }
    }

    private void AddSliderControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        int value = GetIntValue(row.Key);
        float normalized = row.Max == row.Min ? 0f : Math.Clamp((value - row.Min) / (float)(row.Max - row.Min), 0f, 1f);
        float alpha = row.IsEnabled ? (row.IsLocked ? 0.72f : 1f) : 0.5f;
        float textAlpha = row.IsEnabled ? 1f : 0.5f;
        float titleHeight = RowTitleSize + 4f;
        float summaryHeight = IsLongSummarySlider(row) ? LongSliderSummaryLineCount * SliderSummaryLineHeight + SliderSummaryParagraphGap : SliderSummaryLineHeight;
        float containerX = bounds.X + SeekbarContainerMarginX;
        float containerWidth = bounds.Width - SeekbarContainerMarginX * 2f;
        float valueWidth = 72f * DpScale;
        float textWidth = Math.Max(80f * DpScale, containerWidth - valueWidth - ControlGap);
        float textTop = bounds.Y + RowPadding;
        float summaryTop = textTop + titleHeight + 6f * DpScale;
        float valueTop = textTop;
        float trackWidth = bounds.Width - SeekbarTrackMarginX * 2f;
        float trackX = bounds.X + SeekbarTrackMarginX;
        float trackY = summaryTop + summaryHeight + SeekbarTopMargin + (SeekbarThumbSize - SeekbarTrackHeight) / 2f;
        float thumbX = trackX + trackWidth * normalized - SeekbarThumbSize / 2f;
        float thumbY = trackY + SeekbarTrackHeight / 2f - SeekbarThumbSize / 2f;

        bool isInteractive = IsInteractive(row);
        UiAction rowAction = isInteractive ? RowAction(row, index) : UiAction.None;
        elements.Add(Text($"options-row-{index}-label", _localizer[row.TitleKey], containerX, textTop, textWidth, titleHeight, RowTitleSize, s_disabledWhite, textAlpha * 0.94f, true, rowAction, isInteractive));
        string summaryText = GetSummaryText(row);
        if (!string.IsNullOrEmpty(summaryText))
        {
            AddSliderSummary(elements, $"options-row-{index}-summary", summaryText, containerX, summaryTop, textWidth, row, textAlpha * 0.86f, rowAction, isInteractive);
        }

        elements.Add(Text($"options-row-{index}-value", value.ToString(CultureInfo.InvariantCulture), containerX + containerWidth - valueWidth, valueTop, valueWidth, titleHeight, RowTitleSize, s_secondaryText, 0.9f * textAlpha, false, rowAction, isInteractive, UiTextAlignment.Right));
        elements.Add(Fill($"options-row-{index}-slider-track", new UiRect(trackX, trackY, trackWidth, SeekbarTrackHeight), s_sliderTrack, alpha, rowAction, 12f * DpScale, isInteractive));
        elements.Add(Fill($"options-row-{index}-slider-fill", new UiRect(trackX, trackY, trackWidth * normalized, SeekbarTrackHeight), s_checkboxAccent, alpha, rowAction, 12f * DpScale, isInteractive));
        elements.Add(Fill($"options-row-{index}-slider-thumb", new UiRect(thumbX, thumbY, SeekbarThumbSize, SeekbarThumbSize), s_white, alpha, rowAction, 12f * DpScale, isInteractive));
    }

    private static void AddSliderSummary(List<UiElementSnapshot> elements, string id, string summary, float x, float y, float width, SettingsRow row, float alpha, UiAction action, bool enabled)
    {
        if (!IsLongSummarySlider(row))
        {
            elements.Add(Text(id, summary, x, y, width, SliderSummaryLineHeight, RowSummarySize, s_secondaryText, alpha, false, action, enabled));
            return;
        }

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
            elements.Add(Text(lineId, line, x, lineY, width, SliderSummaryLineHeight, RowSummarySize, s_secondaryText, alpha, false, action, enabled));
            lineY += SliderSummaryLineHeight;
            elementIndex++;
        }
    }

    private static IEnumerable<string> WrapText(string text, float size, float width)
    {
        int maxCharacters = Math.Max(8, (int)MathF.Floor(width / (size * 0.48f)));
        foreach (string paragraph in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (paragraph.Length == 0)
            {
                yield return string.Empty;
                continue;
            }

            var line = new System.Text.StringBuilder();
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                int nextLength = line.Length == 0 ? word.Length : line.Length + 1 + word.Length;
                if (line.Length > 0 && nextLength > maxCharacters)
                {
                    yield return line.ToString();
                    line.Clear();
                }

                if (line.Length > 0)
                {
                    line.Append(' ');
                }

                line.Append(word);
            }

            if (line.Length > 0)
            {
                yield return line.ToString();
            }
        }
    }

    private static void AddLockOverlay(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        if (!row.IsLocked)
        {
            return;
        }

        float lockSize = 18f * DpScale;
        var lockBounds = new UiRect(bounds.Right - RowPadding - lockSize, bounds.Y + 10f * DpScale, lockSize, lockSize);
        elements.Add(MaterialIcon($"options-row-{index}-lock", UiMaterialIcon.Lock, lockBounds, s_secondaryText, 0.82f, UiAction.None, false));
    }

    private void AddStatusMessage(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (_statusMessageKey is null)
        {
            return;
        }

        string message = _localizer[_statusMessageKey];
        float width = Math.Min(520f * DpScale, viewport.VirtualWidth - ContentPaddingX * 2f);
        var bounds = new UiRect((viewport.VirtualWidth - width) / 2f, viewport.VirtualHeight - 72f * DpScale, width, 44f * DpScale);
        elements.Add(Fill("options-status-message-bg", bounds, s_selectedSection, 0.96f, UiAction.None, AndroidRoundedRectRadius));
        elements.Add(Text("options-status-message", message, bounds.X + 18f * DpScale, bounds.Y + 10f * DpScale, bounds.Width - 36f * DpScale, RowTitleSize + 4f, RowTitleSize, s_white, 1f, true, UiAction.None, true, UiTextAlignment.Center));
    }

    private static float CalculateContentHeight(IReadOnlyList<SettingsCategory> categories) => categories.Sum(category => CategoryTopMargin + CalculateCategoryHeight(category));

    private static float CalculateSectionHeight() => 32f * DpScale + s_sections.Length * SectionStep + 32f * DpScale;

    private static float CalculateCategoryHeight(SettingsCategory category) => CategoryHeaderHeight + category.Rows.Sum(GetRowHeight);

    private static bool IsLongSummarySlider(SettingsRow row) => row.Key == "gameAudioSynchronizationThreshold";

    private static float GetRowHeight(SettingsRow row) => row.Kind == SettingsRowKind.Input
        ? InputRowHeight
        : row.Kind == SettingsRowKind.Slider ? (IsLongSummarySlider(row) ? LongSliderRowHeight : SliderRowHeight) : RowHeight;

    private static UiAction RowAction(SettingsRow row, int index) => row.Action == UiAction.None
        ? (UiAction)((int)UiAction.OptionsRow0 + index)
        : row.Action;

    private static float VisibleContentHeight(VirtualViewport viewport) => Math.Max(0f, viewport.VirtualHeight - ContentTop);

    private static bool IsVisible(UiRect bounds, VirtualViewport viewport) => bounds.Bottom >= AppBarHeight && bounds.Y <= viewport.VirtualHeight;

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float cornerRadius = 0f, bool enabled = true, UiCornerMode cornerMode = UiCornerMode.All) =>
        UiElementFactory.Fill(id, bounds, color, alpha, action, cornerRadius, enabled, cornerMode);

    private static UiElementSnapshot Sprite(string id, string assetName, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) =>
        UiElementFactory.Sprite(id, assetName, bounds, color, alpha, action, enabled);

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) =>
        UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action, enabled);

    private static UiElementSnapshot Icon(string id, UiIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) =>
        UiElementFactory.Icon(id, icon, bounds, color, alpha, action, enabled);

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
        UiTextAlignment alignment = UiTextAlignment.Left) =>
        UiElementFactory.Text(id, value, new UiRect(x, y, width, height), size, color, action, enabled, bold, alignment, alpha: alpha);

}
