using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;
namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport)
    {
        _lastViewport = viewport;
        ClampAllScrolls(viewport);
        var elements = new List<UiElementSnapshot>
        {
            Fill("modselect-background", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_panel, 0.9f),
        };

        AddTopBar(elements, viewport);
        AddSections(elements, viewport);
        AddBottomBar(elements, viewport);
        AddPresetDialog(elements, viewport);
        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }



    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport, UiFrameSnapshot parentFrame)
    {
        _lastViewport = viewport;
        ClampAllScrolls(viewport);
        var elements = new List<UiElementSnapshot>(parentFrame.Elements.Count + 96);
        elements.AddRange(parentFrame.Elements.Select(WithoutAction));
        elements.Add(Fill("modselect-background", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_panel, 0.9f));
        AddTopBar(elements, viewport);
        AddSections(elements, viewport);
        AddBottomBar(elements, viewport);
        AddPresetDialog(elements, viewport);
        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }



    private void AddTopBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddButton(elements, "modselect-back", new UiRect(SidePadding, 12f, 120f, 58f), _localizer["OsuDroidLanguagePack_menu_mod_back"], UiAction.ModSelectBack, leadingAsset: DroidAssets.CommonBackArrow);
        AddButton(elements, "modselect-customize", new UiRect(190f, 12f, 170f, 58f), "Customize", UiAction.ModSelectCustomize, _selectedAcronyms.Any(ModHasCustomization), leadingAsset: DroidAssets.CommonTune);
        AddButton(elements, "modselect-clear", new UiRect(370f, 12f, 120f, 58f), "Clear", UiAction.ModSelectClear, true, leadingAsset: DroidAssets.CommonBackspace, fillOverride: s_clearButton, textOverride: DroidUiColors.DangerText);
        AddSelectedModsIndicator(elements);

        UiRect search = SearchBounds(viewport);
        elements.Add(Fill("modselect-search", search, s_search, 1f, UiAction.ModSelectSearchBox, 12f));
        elements.Add(Text("modselect-search-text", string.IsNullOrWhiteSpace(_searchInputText) ? "Search..." : _searchInputText, new UiRect(search.X + 18f, search.Y + 15f, search.Width - 74f, 28f), 24f, string.IsNullOrWhiteSpace(_searchInputText) ? s_searchPlaceholder : s_accent, UiAction.ModSelectSearchBox, clipToBounds: true));
        elements.Add(UiElementFactory.Sprite("modselect-search-icon", DroidAssets.CommonSearchSmall, new UiRect(search.Right - 64f, search.Y + 15f, 52f, 28f), s_accent, 1f, UiAction.ModSelectSearchBox, spriteFit: UiSpriteFit.Contain));
    }



    private void AddSelectedModsIndicator(List<UiElementSnapshot> elements)
    {
        UiRect strip = SelectedModsBounds();
        float x = strip.X - _selectedModsScrollX;
        foreach (string acronym in ModCatalog.Entries.Select(entry => entry.Acronym).Where(_selectedAcronyms.Contains))
        {
            UiRect icon = new(x, strip.Y + 8f, SelectedModIconSize, SelectedModIconSize);
            if (icon.Right >= strip.X && icon.X <= strip.Right)
            {
                ModCatalogEntry? entry = EntryByAcronym(acronym);
                if (entry is not null)
                {
                    AddModIcon(elements, $"modselect-selected-{acronym}", entry, icon, UiAction.None, strip);
                }
            }

            x += SelectedModIconSize + SelectedModIconSpacing;
        }

        AddHorizontalScrollbar(elements, "modselect-selected-mods", strip, _selectedModsScrollX, MaxSelectedModsScroll(), IsSelectedModsScrollbarVisible());
    }



    private void AddSections(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float x = SidePadding - _sectionRailScrollX;
        UiRect presetsBounds = new(x, TopBarHeight, PresetSectionWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight);
        if (presetsBounds.Right >= 0f && presetsBounds.X <= viewport.VirtualWidth)
        {
            AddPresetsSection(elements, presetsBounds, viewport);
        }

        x += PresetSectionWidth + SectionGap;
        foreach (IGrouping<string, ModCatalogEntry> section in VisibleEntries().GroupBy(entry => entry.SectionKey))
        {
            UiRect sectionBounds = new(x, TopBarHeight, SectionWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight);
            if (sectionBounds.Right >= 0f && sectionBounds.X <= viewport.VirtualWidth)
            {
                AddSection(elements, section.Key, section.ToList(), sectionBounds, viewport);
            }

            x += SectionWidth + SectionGap;
        }

        AddHorizontalScrollbar(elements, "modselect-rail", SectionRailBounds(viewport), _sectionRailScrollX, MaxSectionRailScroll(viewport), IsRailScrollbarVisible());
    }



    private void AddPresetsSection(List<UiElementSnapshot> elements, UiRect bounds, VirtualViewport viewport)
    {
        elements.Add(Fill("modselect-section-presets", bounds, s_panel, 1f, radius: 16f));
        elements.Add(Text("modselect-section-title-presets", "Presets", new UiRect(bounds.X + 12f, bounds.Y + 12f, bounds.Width - 24f, 30f), 21f, s_accent, bold: true, alignment: UiTextAlignment.Center, alpha: 0.75f));

        UiRect addBounds = new(bounds.X + 12f, bounds.Y + SectionHeaderHeight, bounds.Width - 24f, ToggleHeight * 0.62f);
        UiRect clip = ListClipBounds(bounds);
        float yOffset = SectionScroll("presets");
        addBounds = addBounds with { Y = addBounds.Y - yOffset };
        AddButton(elements, "modselect-preset-add", addBounds, "Add preset", UiAction.ModSelectPresetAdd, _selectedAcronyms.Count > 0, leadingIcon: UiMaterialIcon.Plus, clipBounds: clip);

        float y = addBounds.Bottom + ToggleGap;
        int presetIndex = 0;
        foreach (ModPreset preset in VisiblePresets())
        {
            if (IntersectsVertically(y, ToggleHeight, clip))
            {
                UiAction action = UiActionGroups.TryGetModSelectPresetAction(presetIndex, out UiAction presetAction)
                    ? presetAction
                    : UiAction.None;
                bool selected = preset.Acronyms.Count == _selectedAcronyms.Count && preset.Acronyms.All(_selectedAcronyms.Contains);
                UiColor fill = selected ? s_selectedCard : s_button;
                UiColor text = selected ? s_selectedText : s_text;
                elements.Add(Fill($"modselect-preset-{preset.SafeId}", new UiRect(bounds.X + 12f, y, bounds.Width - 24f, ToggleHeight), fill, 1f, action, 12f) with { ClipBounds = clip });
                elements.Add(Text($"modselect-preset-{preset.SafeId}-name", preset.Name, new UiRect(bounds.X + 24f, y + 10f, bounds.Width - 48f, 28f), 20f, text, action, clipToBounds: true) with { ClipBounds = clip });
                float iconX = bounds.X + 24f;
                foreach (string acronym in preset.Acronyms.Take(8))
                {
                    ModCatalogEntry? entry = EntryByAcronym(acronym);
                    if (entry is not null)
                    {
                        AddModIcon(elements, $"modselect-preset-{preset.SafeId}-{acronym}", entry, new UiRect(iconX, y + 46f, 21f, 21f), action, clip);
                        iconX += 19f;
                    }
                }
            }

            presetIndex++;
            y += ToggleHeight + ToggleGap;
        }

        AddVerticalScrollbar(elements, "modselect-section-presets", bounds, "presets", viewport);
    }



    private void AddSection(List<UiElementSnapshot> elements, string sectionKey, IReadOnlyList<ModCatalogEntry> entries, UiRect bounds, VirtualViewport viewport)
    {
        elements.Add(Fill($"modselect-section-{sectionKey}", bounds, s_panel, 1f, radius: 16f));
        elements.Add(Text($"modselect-section-title-{sectionKey}", _localizer[sectionKey], new UiRect(bounds.X + 12f, bounds.Y + 12f, bounds.Width - 24f, 30f), 21f, s_accent, bold: true, alignment: UiTextAlignment.Center, alpha: 0.75f));

        float y = bounds.Y + SectionHeaderHeight - SectionScroll(sectionKey);
        UiRect clip = ListClipBounds(bounds);
        foreach (ModCatalogEntry entry in entries)
        {
            int index = IndexOfEntry(entry);
            if (UiActionGroups.TryGetModSelectToggleAction(index, out UiAction action) && IntersectsVertically(y, ToggleHeight, clip))
            {
                AddToggle(elements, entry, action, bounds.X + 12f, y, bounds.Width - 24f, clip);
            }

            y += ToggleHeight + ToggleGap;
        }

        AddVerticalScrollbar(elements, $"modselect-section-{sectionKey}", bounds, sectionKey, viewport);
    }



    private void AddToggle(List<UiElementSnapshot> elements, ModCatalogEntry entry, UiAction action, float x, float y, float width, UiRect clipBounds)
    {
        bool selected = _selectedAcronyms.Contains(entry.Acronym);
        bool incompatible = !selected && IsIncompatibleWithSelection(entry);
        float alpha = incompatible ? 0.5f : 1f;
        UiColor fill = selected ? s_selectedCard : s_button;
        UiColor text = selected ? s_selectedText : s_text;
        UiColor dimText = selected ? s_selectedText : s_dimText;
        elements.Add(Fill($"modselect-toggle-{entry.Acronym}", new UiRect(x, y, width, ToggleHeight), fill, alpha, action, 12f) with { ClipBounds = clipBounds });
        AddModIcon(elements, $"modselect-toggle-icon-{entry.Acronym}", entry, new UiRect(x + TogglePaddingX, y + (ToggleHeight - ToggleIconSize) / 2f, ToggleIconSize, ToggleIconSize), action, clipBounds);
        float textX = x + TogglePaddingX + ToggleIconSize + 8f;
        float textWidth = width - TogglePaddingX * 2f - ToggleIconSize - 8f;
        elements.Add(Text($"modselect-toggle-name-{entry.Acronym}", entry.Name, new UiRect(textX, y + 17f, textWidth, 26f), 21f, text, action, alpha: alpha, clipToBounds: true) with { ClipBounds = clipBounds });
        elements.Add(Text($"modselect-toggle-description-{entry.Acronym}", entry.Description, new UiRect(textX, y + 44f, textWidth, 20f), 16f, dimText, action, alpha: alpha * 0.75f, clipToBounds: true, autoScroll: true) with { ClipBounds = clipBounds });
    }



    private static void AddModIcon(List<UiElementSnapshot> elements, string id, ModCatalogEntry entry, UiRect bounds, UiAction action, UiRect? clipBounds = null) =>
            elements.Add(UiElementFactory.Sprite(id, entry.AssetName, bounds, s_text, 1f, action, spriteFit: UiSpriteFit.Contain) with { ClipBounds = clipBounds });



    private void AddPresetDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (_isPresetFormOpen)
        {
            AddPresetFormDialog(elements, viewport);
            return;
        }

        if (_isPresetDeleteDialogOpen)
        {
            AddPresetDeleteDialog(elements, viewport);
        }
    }



    private void AddPresetFormDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddModalBackdrop(elements, viewport);
        UiRect panel = PresetDialogPanelBounds(viewport);
        elements.Add(Fill("modselect-preset-dialog", panel, s_badge, 1f, radius: 16f));
        elements.Add(Text("modselect-preset-dialog-title", "New mod preset", new UiRect(panel.X, panel.Y + 16f, panel.Width, 30f), 21f, s_accent, bold: true, alignment: UiTextAlignment.Center, alpha: 0.7f));
        elements.Add(Fill("modselect-preset-dialog-divider-top", new UiRect(panel.X, panel.Y + 62f, panel.Width, 1f), s_accent, 0.1f));

        elements.Add(Text("modselect-preset-dialog-name-label", "Name", new UiRect(panel.X + 24f, panel.Y + 82f, panel.Width - 48f, 28f), 19f, s_accent));
        UiRect input = PresetNameInputBounds(viewport);
        elements.Add(Fill("modselect-preset-dialog-name-input", input, s_search, 1f, UiAction.ModSelectPresetNameInput, 12f));
        elements.Add(Fill("modselect-preset-dialog-name-input-outline", input, s_accent, 0.4f, UiAction.ModSelectPresetNameInput, 12f));
        elements.Add(Text("modselect-preset-dialog-name-text", _presetNameInput, new UiRect(input.X + 12f, input.Y + 9f, input.Width - 24f, 28f), 19f, s_accent, UiAction.ModSelectPresetNameInput, clipToBounds: true));

        AddPresetDialogModsIndicator(elements, panel);
        elements.Add(Fill("modselect-preset-dialog-divider-bottom", new UiRect(panel.X, panel.Bottom - 82f, panel.Width, 1f), s_accent, 0.1f));
        AddDialogButton(elements, "modselect-preset-dialog-save", new UiRect(panel.X + 24f, panel.Bottom - 66f, (panel.Width - 60f) / 2f, 42f), "Save", UiAction.ModSelectPresetSave, true);
        AddDialogButton(elements, "modselect-preset-dialog-cancel", new UiRect(panel.X + 36f + (panel.Width - 60f) / 2f, panel.Bottom - 66f, (panel.Width - 60f) / 2f, 42f), "Cancel", UiAction.ModSelectPresetCancel, false);
    }



    private void AddPresetDeleteDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddModalBackdrop(elements, viewport);
        ModPreset? preset = VisiblePresetAt(_pendingPresetDeleteIndex);
        string text = preset is null ? "Delete preset?" : $"Delete preset \"{preset.Name}\"?";
        UiRect panel = new((viewport.VirtualWidth - viewport.VirtualWidth * 0.5f) / 2f, (viewport.VirtualHeight - 210f) / 2f, viewport.VirtualWidth * 0.5f, 210f);
        elements.Add(Fill("modselect-preset-delete-dialog", panel, s_badge, 1f, radius: 16f));
        elements.Add(Text("modselect-preset-delete-title", "Delete preset", new UiRect(panel.X, panel.Y + 16f, panel.Width, 30f), 21f, s_accent, bold: true, alignment: UiTextAlignment.Center, alpha: 0.7f));
        elements.Add(Fill("modselect-preset-delete-divider-top", new UiRect(panel.X, panel.Y + 62f, panel.Width, 1f), s_accent, 0.1f));
        elements.Add(Text("modselect-preset-delete-message", text, new UiRect(panel.X + 24f, panel.Y + 82f, panel.Width - 48f, 34f), 19f, s_accent, alignment: UiTextAlignment.Center, clipToBounds: true));
        elements.Add(Fill("modselect-preset-delete-divider-bottom", new UiRect(panel.X, panel.Bottom - 82f, panel.Width, 1f), s_accent, 0.1f));
        AddDialogButton(elements, "modselect-preset-delete-confirm", new UiRect(panel.X + 24f, panel.Bottom - 66f, (panel.Width - 60f) / 2f, 42f), "Delete", UiAction.ModSelectPresetDeleteConfirm, true);
        AddDialogButton(elements, "modselect-preset-delete-cancel", new UiRect(panel.X + 36f + (panel.Width - 60f) / 2f, panel.Bottom - 66f, (panel.Width - 60f) / 2f, 42f), "Cancel", UiAction.ModSelectPresetDeleteCancel, false);
    }



    private static void AddModalBackdrop(List<UiElementSnapshot> elements, VirtualViewport viewport) =>
            elements.Add(Fill("modselect-preset-dialog-backdrop", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_black, 0.3f, UiAction.ModSelectPresetBackdrop));



    private void AddPresetDialogModsIndicator(List<UiElementSnapshot> elements, UiRect panel)
    {
        float x = panel.X + 24f;
        float y = panel.Y + 190f;
        foreach (string acronym in ModCatalog.Entries.Select(entry => entry.Acronym).Where(_selectedAcronyms.Contains))
        {
            ModCatalogEntry? entry = EntryByAcronym(acronym);
            if (entry is not null)
            {
                AddModIcon(elements, $"modselect-preset-dialog-mod-{acronym}", entry, new UiRect(x, y, 42f, 42f), UiAction.None, new UiRect(panel.X + 24f, panel.Y + 174f, panel.Width - 48f, 54f));
                x += 37f;
            }
        }
    }



    private void AddDialogButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, bool selected)
    {
        UiColor fill = selected ? s_accent : s_button;
        UiColor color = selected ? s_selectedText : s_accent;
        elements.Add(Fill(id, bounds, fill, 1f, action, 12f));
        elements.Add(Text($"{id}-text", text, bounds, 20f, color, action, bold: true, alignment: UiTextAlignment.Center, clipToBounds: true));
    }



    private void AddBottomBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float y = viewport.VirtualHeight - 56f;
        float leftX = SidePadding;
        leftX = AddLabeledBadge(elements, "ar", "AR", FormatStat(_selectedBeatmap?.ApproachRate), leftX, y);
        leftX = AddLabeledBadge(elements, "od", "OD", FormatStat(_selectedBeatmap?.OverallDifficulty), leftX + 10f, y);
        leftX = AddLabeledBadge(elements, "cs", "CS", FormatStat(_selectedBeatmap?.CircleSize), leftX + 10f, y);
        leftX = AddLabeledBadge(elements, "hp", "HP", FormatStat(_selectedBeatmap?.HpDrainRate), leftX + 10f, y);
        AddLabeledBadge(elements, "bpm", "BPM", _selectedBeatmap?.MostCommonBpm.ToString("0", System.Globalization.CultureInfo.InvariantCulture) ?? "0", leftX + 10f, y);

        string scoreValue = ScoreMultiplier.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "x";
        float scoreWidth = LabeledBadgeWidth("Score", scoreValue);
        float scoreX = viewport.VirtualWidth - SidePadding - scoreWidth;
        AddLabeledBadge(elements, "score", "Score", scoreValue, scoreX, y);
        UiColor rankedFill = IsRanked ? s_ranked : s_button;
        UiColor rankedText = IsRanked ? s_rankedText : s_accent;
        string rankedTextValue = IsRanked ? "Ranked" : "Unranked";
        float rankedWidth = BadgeWidth(rankedTextValue);
        float rankedX = scoreX - 10f - rankedWidth;
        elements.Add(Fill("modselect-ranked-badge", new UiRect(rankedX, y, rankedWidth, 44f), rankedFill, 1f, radius: 12f));
        elements.Add(Text("modselect-ranked-badge-text", rankedTextValue, new UiRect(rankedX + 12f, y + 8f, rankedWidth - 24f, 28f), 18f, rankedText, bold: true, alignment: UiTextAlignment.Center));

        float starValue = _selectedBeatmap?.DroidStarRating ?? _selectedBeatmap?.StandardStarRating ?? 0f;
        UiColor starFill = StarRatingColor(starValue);
        UiColor starText = starValue >= 6.5f ? StarRatingTextColor(starValue) : s_black;
        float starAlpha = starValue >= 6.5f ? 1f : 0.75f;
        string starValueText = FormatStat(starValue);
        float starWidth = 12f + 20f + 8f + TextWidth(starValueText, 18f) + 12f;
        float starX = rankedX - 10f - starWidth;
        elements.Add(Fill("modselect-star-badge", new UiRect(starX, y, starWidth, 44f), starFill, starAlpha, radius: 12f));
        elements.Add(UiElementFactory.Sprite("modselect-star-icon", DroidAssets.SongSelectStar, new UiRect(starX + 12f, y + 12f, 20f, 20f), starText, 1f, spriteFit: UiSpriteFit.Contain));
        elements.Add(Text("modselect-star-value", starValueText, new UiRect(starX + 40f, y + 9f, starWidth - 52f, 24f), 18f, starText, alignment: UiTextAlignment.Center));
    }



}
