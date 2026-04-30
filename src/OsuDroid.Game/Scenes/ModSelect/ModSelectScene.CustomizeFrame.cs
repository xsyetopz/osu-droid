using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private const float CustomizeCardX = 60f;
    private const float CustomizeCardY = 90f;
    private const float CustomizeCardWidthRatio = 0.475f;
    private const float CustomizeCardHeightRatio = 0.75f;
    private const float CustomizeHeaderHeight = 62f;
    private const float CustomizeSettingHeight = 96f;
    private const float CustomizeSectionGap = 16f;

    private void AddCustomizeDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!_isCustomizeOpen || _isPresetFormOpen || _isPresetDeleteDialogOpen)
        {
            return;
        }

        ModCatalogEntry[] selectedEntries = ModCatalog
            .Entries.Where(entry =>
                _selectedAcronyms.Contains(entry.Acronym) && entry.HasCustomization
            )
            .ToArray();
        if (selectedEntries.Length == 0)
        {
            CloseCustomizePanel();
            return;
        }

        elements.Add(
            Fill(
                "modselect-customize-backdrop",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_black,
                0.3f,
                UiAction.ModSelectPresetBackdrop
            )
        );

        UiRect panel = CustomizePanelBounds(viewport);
        elements.Add(Fill("modselect-customize-dialog", panel, s_badge, 1f, radius: 16f));

        UiRect clip = panel;
        float y = panel.Y + 16f;
        int settingIndex = 0;
        foreach (ModCatalogEntry entry in selectedEntries)
        {
            EnsureModSettings(entry);
            UiRect header = new(panel.X + 18f, y, panel.Width - 36f, 50f);
            elements.Add(
                Fill(
                    $"modselect-customize-section-{entry.Acronym}",
                    header,
                    s_black,
                    0.05f,
                    radius: 12f
                ) with
                {
                    ClipBounds = clip,
                }
            );
            elements.Add(
                Sprite(
                    $"modselect-customize-section-icon-{entry.Acronym}",
                    entry.AssetName,
                    new UiRect(header.X + 14f, header.Y + 8f, 34f, 34f),
                    s_text
                ) with
                {
                    ClipBounds = clip,
                }
            );
            elements.Add(
                Text(
                    $"modselect-customize-section-title-{entry.Acronym}",
                    entry.Name.ToUpperInvariant(),
                    new UiRect(header.X + 60f, header.Y + 11f, header.Width - 74f, 28f),
                    20f,
                    s_accent,
                    alpha: 0.9f,
                    bold: true,
                    clipToBounds: true
                ) with
                {
                    ClipBounds = clip,
                }
            );

            y += CustomizeHeaderHeight;
            foreach (ModSettingDescriptor setting in entry.Settings ?? [])
            {
                AddCustomizeSetting(elements, entry, setting, settingIndex, y, panel, clip);
                y += CustomizeSettingHeight;
                settingIndex++;
            }

            y += CustomizeSectionGap;
        }
    }

    private void AddCustomizeSetting(
        List<UiElementSnapshot> elements,
        ModCatalogEntry entry,
        ModSettingDescriptor setting,
        int index,
        float y,
        UiRect panel,
        UiRect clip
    )
    {
        string raw =
            _modSettings.TryGetValue(entry.Acronym, out Dictionary<string, string>? values)
            && values.TryGetValue(setting.Key, out string? stored)
                ? stored
                : ModStatCalculator.DefaultRawValue(setting);
        string value = ModStatCalculator.FormatSettingValue(setting, raw);
        UiRect row = new(panel.X + 24f, y, panel.Width - 48f, CustomizeSettingHeight - 8f);
        UiAction rowAction = UiActionGroups.TryGetModSelectCustomizeSettingIncreaseAction(
            index,
            out UiAction action
        )
            ? action
            : UiAction.None;

        elements.Add(
            Fill($"modselect-customize-row-{index}", row, s_button, 0.35f, rowAction, 10f) with
            {
                ClipBounds = clip,
            }
        );
        elements.Add(
            Text(
                $"modselect-customize-row-{index}-name",
                setting.Name,
                new UiRect(row.X + 16f, row.Y + 10f, row.Width * 0.64f, 28f),
                19f,
                s_text,
                rowAction,
                clipToBounds: true
            ) with
            {
                ClipBounds = clip,
            }
        );
        elements.Add(
            Text(
                $"modselect-customize-row-{index}-value",
                value,
                new UiRect(row.Right - 110f, row.Y + 10f, 94f, 28f),
                18f,
                s_accent,
                rowAction,
                alignment: UiTextAlignment.Right,
                clipToBounds: true
            ) with
            {
                ClipBounds = clip,
            }
        );

        if (IsSliderSetting(setting))
        {
            AddCustomizeSlider(elements, setting, raw, index, clip, rowAction);
        }
        else if (setting.Kind == ModSettingKind.Toggle)
        {
            AddCustomizeCheckbox(elements, raw, index, row, clip, rowAction);
        }
        else if (setting.Kind == ModSettingKind.Choice)
        {
            elements.Add(
                MaterialIcon(
                    $"modselect-customize-row-{index}-dropdown",
                    UiMaterialIcon.ArrowDropDown,
                    new UiRect(row.Right - 42f, row.Y + 8f, 30f, 30f),
                    s_accent,
                    0.9f,
                    rowAction
                ) with
                {
                    ClipBounds = clip,
                }
            );
        }
        else if (setting.UseManualInput)
        {
            elements.Add(
                Fill(
                    $"modselect-customize-row-{index}-input",
                    new UiRect(row.X + 16f, row.Y + 48f, row.Width - 32f, 34f),
                    s_search,
                    0.7f,
                    rowAction,
                    8f
                ) with
                {
                    ClipBounds = clip,
                }
            );
        }
    }

    private void AddCustomizeSlider(
        List<UiElementSnapshot> elements,
        ModSettingDescriptor setting,
        string raw,
        int index,
        UiRect clip,
        UiAction action
    )
    {
        UiRect slider = CustomizeSliderBounds(index, _lastViewport);
        double value = SettingNumber(setting, raw);
        double normalized =
            setting.MaxValue <= setting.MinValue
                ? 0
                : Math.Clamp(
                    (value - setting.MinValue) / (setting.MaxValue - setting.MinValue),
                    0d,
                    1d
                );
        float progressWidth = Math.Max(24f, (float)(slider.Width * normalized));
        elements.Add(
            Fill($"modselect-customize-slider-{index}", slider, s_accent, 0.25f, action, 12f) with
            {
                ClipBounds = clip,
            }
        );
        elements.Add(
            Fill(
                $"modselect-customize-slider-{index}-progress",
                new UiRect(slider.X, slider.Y, progressWidth, slider.Height),
                s_accent,
                0.5f,
                action,
                12f
            ) with
            {
                ClipBounds = clip,
            }
        );
        elements.Add(
            Fill(
                $"modselect-customize-slider-{index}-thumb",
                new UiRect(slider.X + progressWidth - 12f, slider.Y, 24f, slider.Height),
                s_accent,
                1f,
                action,
                12f
            ) with
            {
                ClipBounds = clip,
            }
        );
    }

    private static void AddCustomizeCheckbox(
        List<UiElementSnapshot> elements,
        string raw,
        int index,
        UiRect row,
        UiRect clip,
        UiAction action
    )
    {
        bool enabled = string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
        UiRect checkbox = new(row.Right - 44f, row.Y + 46f, 28f, 28f);
        elements.Add(
            MaterialIcon(
                $"modselect-customize-row-{index}-checkbox",
                enabled ? UiMaterialIcon.Check : UiMaterialIcon.CheckboxBlankOutline,
                checkbox,
                enabled ? s_selectedText : s_accent,
                0.9f,
                action
            ) with
            {
                ClipBounds = clip,
            }
        );
    }

    private static UiRect CustomizePanelBounds(VirtualViewport viewport) =>
        new(
            CustomizeCardX,
            CustomizeCardY,
            viewport.VirtualWidth * CustomizeCardWidthRatio,
            viewport.VirtualHeight * CustomizeCardHeightRatio
        );

    private UiRect CustomizeSliderBounds(int index, VirtualViewport viewport)
    {
        UiRect panel = CustomizePanelBounds(viewport);
        float y = panel.Y + 16f;
        int current = 0;
        foreach (
            ModCatalogEntry entry in ModCatalog.Entries.Where(entry =>
                _selectedAcronyms.Contains(entry.Acronym) && entry.HasCustomization
            )
        )
        {
            y += CustomizeHeaderHeight;
            foreach (ModSettingDescriptor _ in entry.Settings ?? [])
            {
                if (current == index)
                {
                    return new UiRect(panel.X + 48f, y + 48f, panel.Width - 96f, 48f);
                }

                y += CustomizeSettingHeight;
                current++;
            }

            y += CustomizeSectionGap;
        }

        float rowY = panel.Y + 16f + CustomizeHeaderHeight + index * CustomizeSettingHeight + 48f;
        return new UiRect(panel.X + 48f, rowY, panel.Width - 96f, 48f);
    }

    private UiRect CustomizeInputBounds(int index, VirtualViewport viewport)
    {
        UiRect slider = CustomizeSliderBounds(index, viewport);
        return new UiRect(slider.X, slider.Y, slider.Width, 36f);
    }

    private static bool IsSliderSetting(ModSettingDescriptor setting) =>
        setting.Kind
            is ModSettingKind.Slider
                or ModSettingKind.OptionalSlider
                or ModSettingKind.WholeNumber
                or ModSettingKind.OptionalWholeNumber
        && !setting.UseManualInput;

    private static double SettingNumber(ModSettingDescriptor setting, string raw)
    {
        return string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase)
            ? setting.DefaultValue == 0
                ? setting.MinValue
                : setting.DefaultValue
            : double.TryParse(
                raw,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double value
            )
                ? value
                : setting.DefaultValue;
    }
}
