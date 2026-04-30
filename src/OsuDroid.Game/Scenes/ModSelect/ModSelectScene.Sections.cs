using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private void AddSections(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float x = SidePadding - _sectionRailScrollX;
        UiRect presetsBounds = new(
            x,
            TopBarHeight,
            PresetSectionWidth,
            viewport.VirtualHeight - TopBarHeight - BottomBarHeight
        );
        if (presetsBounds.Right >= 0f && presetsBounds.X <= viewport.VirtualWidth)
        {
            AddPresetsSection(elements, presetsBounds, viewport);
        }

        x += PresetSectionWidth + SectionGap;
        foreach (
            IGrouping<string, ModCatalogEntry> section in VisibleEntries()
                .GroupBy(entry => entry.SectionKey)
        )
        {
            UiRect sectionBounds = new(
                x,
                TopBarHeight,
                SectionWidth,
                viewport.VirtualHeight - TopBarHeight - BottomBarHeight
            );
            if (sectionBounds.Right >= 0f && sectionBounds.X <= viewport.VirtualWidth)
            {
                AddSection(elements, section.Key, section.ToList(), sectionBounds, viewport);
            }

            x += SectionWidth + SectionGap;
        }

        AddHorizontalScrollbar(
            elements,
            "modselect-rail",
            SectionRailBounds(viewport),
            _sectionRailScrollX,
            MaxSectionRailScroll(viewport),
            IsRailScrollbarVisible()
        );
    }

    private void AddPresetsSection(
        List<UiElementSnapshot> elements,
        UiRect bounds,
        VirtualViewport viewport
    )
    {
        elements.Add(Fill("modselect-section-presets", bounds, s_panel, 1f, radius: 16f));
        elements.Add(
            Text(
                "modselect-section-title-presets",
                "Presets",
                new UiRect(bounds.X + 12f, bounds.Y + 12f, bounds.Width - 24f, 30f),
                21f,
                s_accent,
                bold: true,
                alignment: UiTextAlignment.Center,
                alpha: 0.75f
            )
        );

        UiRect addBounds = new(
            bounds.X + 12f,
            bounds.Y + SectionHeaderHeight,
            bounds.Width - 24f,
            ToggleHeight * 0.62f
        );
        UiRect clip = ListClipBounds(bounds);
        float yOffset = SectionScroll("presets");
        addBounds = addBounds with { Y = addBounds.Y - yOffset };
        AddButton(
            elements,
            "modselect-preset-add",
            addBounds,
            "Add preset",
            UiAction.ModSelectPresetAdd,
            _selectedAcronyms.Count > 0,
            leadingIcon: UiMaterialIcon.Plus,
            clipBounds: clip
        );

        float y = addBounds.Bottom + ToggleGap;
        int presetIndex = 0;
        foreach (ModPreset preset in VisiblePresets())
        {
            if (IntersectsVertically(y, ToggleHeight, clip))
            {
                UiAction action = UiActionGroups.TryGetModSelectPresetSlotAction(
                    presetIndex,
                    out UiAction presetAction
                )
                    ? presetAction
                    : UiAction.None;
                bool selected =
                    preset.Acronyms.Count == _selectedAcronyms.Count
                    && preset.Acronyms.All(_selectedAcronyms.Contains);
                UiColor fill = selected ? s_selectedCard : s_button;
                UiColor text = selected ? s_selectedText : s_text;
                elements.Add(
                    Fill(
                        $"modselect-preset-{preset.SafeId}",
                        new UiRect(bounds.X + 12f, y, bounds.Width - 24f, ToggleHeight),
                        fill,
                        1f,
                        action,
                        12f
                    ) with
                    {
                        ClipBounds = clip,
                    }
                );
                elements.Add(
                    Text(
                        $"modselect-preset-{preset.SafeId}-name",
                        preset.Name,
                        new UiRect(bounds.X + 24f, y + 10f, bounds.Width - 48f, 28f),
                        20f,
                        text,
                        action,
                        clipToBounds: true
                    ) with
                    {
                        ClipBounds = clip,
                    }
                );
                float iconX = bounds.X + 24f;
                foreach (string acronym in preset.Acronyms.Take(8))
                {
                    ModCatalogEntry? entry = EntryByAcronym(acronym);
                    if (entry is not null)
                    {
                        AddModIcon(
                            elements,
                            $"modselect-preset-{preset.SafeId}-{acronym}",
                            entry,
                            new UiRect(iconX, y + 46f, 21f, 21f),
                            action,
                            clip
                        );
                        iconX += 19f;
                    }
                }
            }

            presetIndex++;
            y += ToggleHeight + ToggleGap;
        }

        AddVerticalScrollbar(elements, "modselect-section-presets", bounds, "presets", viewport);
    }

    private void AddSection(
        List<UiElementSnapshot> elements,
        string sectionKey,
        IReadOnlyList<ModCatalogEntry> entries,
        UiRect bounds,
        VirtualViewport viewport
    )
    {
        elements.Add(Fill($"modselect-section-{sectionKey}", bounds, s_panel, 1f, radius: 16f));
        elements.Add(
            Text(
                $"modselect-section-title-{sectionKey}",
                _localizer[sectionKey],
                new UiRect(bounds.X + 12f, bounds.Y + 12f, bounds.Width - 24f, 30f),
                21f,
                s_accent,
                bold: true,
                alignment: UiTextAlignment.Center,
                alpha: 0.75f
            )
        );

        float y = bounds.Y + SectionHeaderHeight - SectionScroll(sectionKey);
        UiRect clip = ListClipBounds(bounds);
        foreach (ModCatalogEntry entry in entries)
        {
            int index = IndexOfEntry(entry);
            if (
                UiActionGroups.TryGetModSelectCatalogModToggleAction(index, out UiAction action)
                && IntersectsVertically(y, ToggleHeight, clip)
            )
            {
                AddToggle(elements, entry, action, bounds.X + 12f, y, bounds.Width - 24f, clip);
            }

            y += ToggleHeight + ToggleGap;
        }

        AddVerticalScrollbar(
            elements,
            $"modselect-section-{sectionKey}",
            bounds,
            sectionKey,
            viewport
        );
    }

    private void AddToggle(
        List<UiElementSnapshot> elements,
        ModCatalogEntry entry,
        UiAction action,
        float x,
        float y,
        float width,
        UiRect clipBounds
    )
    {
        bool selected = _selectedAcronyms.Contains(entry.Acronym);
        bool incompatible = !selected && IsIncompatibleWithSelection(entry);
        float alpha = incompatible ? 0.5f : 1f;
        UiColor fill = selected ? s_selectedCard : s_button;
        UiColor text = selected ? s_selectedText : s_text;
        UiColor dimText = selected ? s_selectedText : s_dimText;
        elements.Add(
            Fill(
                $"modselect-toggle-{entry.Acronym}",
                new UiRect(x, y, width, ToggleHeight),
                fill,
                alpha,
                action,
                12f
            ) with
            {
                ClipBounds = clipBounds,
            }
        );
        AddModIcon(
            elements,
            $"modselect-toggle-icon-{entry.Acronym}",
            entry,
            new UiRect(
                x + TogglePaddingX,
                y + (ToggleHeight - ToggleIconSize) / 2f,
                ToggleIconSize,
                ToggleIconSize
            ),
            action,
            clipBounds
        );
        float textX = x + TogglePaddingX + ToggleIconSize + 8f;
        float textWidth = width - TogglePaddingX * 2f - ToggleIconSize - 8f;
        elements.Add(
            Text(
                $"modselect-toggle-name-{entry.Acronym}",
                entry.Name,
                new UiRect(textX, y + 17f, textWidth, 26f),
                21f,
                text,
                action,
                alpha: alpha,
                clipToBounds: true
            ) with
            {
                ClipBounds = clipBounds,
            }
        );
        elements.Add(
            Text(
                $"modselect-toggle-description-{entry.Acronym}",
                entry.Description,
                new UiRect(textX, y + 44f, textWidth, 20f),
                16f,
                dimText,
                action,
                alpha: alpha * 0.75f,
                clipToBounds: true,
                autoScroll: true
            ) with
            {
                ClipBounds = clipBounds,
            }
        );
    }

    private static void AddModIcon(
        List<UiElementSnapshot> elements,
        string id,
        ModCatalogEntry entry,
        UiRect bounds,
        UiAction action,
        UiRect? clipBounds = null
    ) =>
        elements.Add(
            UiElementFactory.Sprite(
                id,
                entry.AssetName,
                bounds,
                s_text,
                1f,
                action,
                spriteFit: UiSpriteFit.Contain
            ) with
            {
                ClipBounds = clipBounds,
            }
        );
}
