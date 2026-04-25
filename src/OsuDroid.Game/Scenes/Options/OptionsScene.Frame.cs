using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;
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

}
