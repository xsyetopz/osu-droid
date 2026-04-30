using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    public bool IsCustomizeModalOpen =>
        _isCustomizeOpen && !_isPresetFormOpen && !_isPresetDeleteDialogOpen;

    public bool ToggleMod(int index)
    {
        if (index < 0 || index >= ModCatalog.Entries.Count)
        {
            return false;
        }

        ModCatalogEntry entry = ModCatalog.Entries[index];
        string acronym = entry.Acronym;
        bool isSelected = _selectedAcronyms.Add(acronym);
        if (!isSelected)
        {
            _selectedAcronyms.Remove(acronym);
            _modSettings.Remove(acronym);
        }
        else
        {
            EnsureModSettings(entry);
            foreach (
                string incompatibleAcronym in ModCatalog
                    .Entries.Where(candidate =>
                        !string.Equals(
                            candidate.Acronym,
                            acronym,
                            StringComparison.OrdinalIgnoreCase
                        ) && !AreCompatible(entry, candidate)
                    )
                    .Select(candidate => candidate.Acronym)
            )
            {
                _selectedAcronyms.Remove(incompatibleAcronym);
                _modSettings.Remove(incompatibleAcronym);
            }
        }

        _selectedModsScrollX = Math.Clamp(_selectedModsScrollX, 0f, MaxSelectedModsScroll());
        SaveSelectedMods();
        SaveModSettings();
        return isSelected;
    }

    public void Clear()
    {
        _selectedAcronyms.Clear();
        _modSettings.Clear();
        _isCustomizeOpen = false;
        _selectedModsScrollX = 0f;
        _activeCustomizeSliderIndex = null;
        SaveSelectedMods();
        SaveModSettings();
    }

    private bool IsIncompatibleWithSelection(ModCatalogEntry entry) =>
        ModCatalog.Entries.Any(selected =>
            _selectedAcronyms.Contains(selected.Acronym)
            && !string.Equals(selected.Acronym, entry.Acronym, StringComparison.OrdinalIgnoreCase)
            && !AreCompatible(entry, selected)
        );

    private static bool AreCompatible(ModCatalogEntry first, ModCatalogEntry second) =>
        !ContainsAcronym(first.IncompatibleAcronyms, second.Acronym)
        && !ContainsAcronym(second.IncompatibleAcronyms, first.Acronym);

    private static bool ContainsAcronym(IReadOnlyList<string>? acronyms, string acronym) =>
        acronyms is not null
        && acronyms.Any(candidate =>
            string.Equals(candidate, acronym, StringComparison.OrdinalIgnoreCase)
        );

    private void LoadSelectedMods()
    {
        foreach (
            string acronym in _settingsStore
                .GetString(SelectedModsSettingKey, string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        )
        {
            if (
                ModCatalog.Entries.Any(entry =>
                    string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                _selectedAcronyms.Add(acronym);
            }
        }
    }

    private void LoadModSettings()
    {
        _modSettings.Clear();
        foreach (
            KeyValuePair<
                string,
                IReadOnlyDictionary<string, string>
            > pair in ModSelectionStore.LoadSettings(_settingsStore)
        )
        {
            ModCatalogEntry? entry = EntryByAcronym(pair.Key);
            if (entry is null)
            {
                continue;
            }

            Dictionary<string, string> settings = new(StringComparer.OrdinalIgnoreCase);
            foreach (ModSettingDescriptor descriptor in entry.Settings ?? [])
            {
                settings[descriptor.Key] = pair.Value.TryGetValue(descriptor.Key, out string? value)
                    ? NormalizeSettingRawValue(descriptor, value)
                    : ModStatCalculator.DefaultRawValue(descriptor);
            }

            if (settings.Count > 0)
            {
                _modSettings[pair.Key] = settings;
            }
        }

        foreach (string acronym in _selectedAcronyms.ToArray())
        {
            if (EntryByAcronym(acronym) is { } entry)
            {
                EnsureModSettings(entry);
            }
        }
    }

    private void SaveSelectedMods() =>
        _settingsStore.SetString(
            SelectedModsSettingKey,
            string.Join(',', _selectedAcronyms.Order(StringComparer.OrdinalIgnoreCase))
        );

    private void SaveModSettings() => ModSelectionStore.SaveSettings(_settingsStore, _modSettings);

    private static bool ModHasCustomization(string acronym) =>
        ModCatalog.Entries.Any(entry =>
            entry.HasCustomization
            && string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase)
        );

    private static ModCatalogEntry? EntryByAcronym(string acronym) =>
        ModCatalog.Entries.FirstOrDefault(entry =>
            string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase)
        );

    private static string FormatStat(float? value) =>
        value?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) ?? "0.00";

    private void EnsureModSettings(ModCatalogEntry entry)
    {
        if (!entry.HasCustomization)
        {
            return;
        }

        if (!_modSettings.TryGetValue(entry.Acronym, out Dictionary<string, string>? settings))
        {
            settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _modSettings[entry.Acronym] = settings;
        }

        foreach (ModSettingDescriptor descriptor in entry.Settings ?? [])
        {
            settings.TryAdd(descriptor.Key, ModStatCalculator.DefaultRawValue(descriptor));
        }
    }

    public bool ToggleCustomizePanel()
    {
        if (!_selectedAcronyms.Any(ModHasCustomization))
        {
            _isCustomizeOpen = false;
            return false;
        }

        foreach (
            ModCatalogEntry entry in ModCatalog.Entries.Where(entry =>
                _selectedAcronyms.Contains(entry.Acronym)
            )
        )
        {
            EnsureModSettings(entry);
        }

        _isCustomizeOpen = !_isCustomizeOpen;
        SaveModSettings();
        return true;
    }

    public void CloseCustomizePanel()
    {
        _isCustomizeOpen = false;
        _activeCustomizeSliderIndex = null;
    }

    public bool AdjustCustomizeSetting(int visibleIndex, int direction)
    {
        (ModCatalogEntry Entry, ModSettingDescriptor Setting)[] items = VisibleCustomizeSettings()
            .ToArray();
        if ((uint)visibleIndex >= (uint)items.Length)
        {
            return false;
        }

        (ModCatalogEntry entry, ModSettingDescriptor setting) = items[visibleIndex];
        EnsureModSettings(entry);
        Dictionary<string, string> values = _modSettings[entry.Acronym];
        string current = values.TryGetValue(setting.Key, out string? raw)
            ? raw
            : ModStatCalculator.DefaultRawValue(setting);
        values[setting.Key] = AdjustSettingRawValue(setting, current, direction);
        SaveModSettings();
        return true;
    }

    public bool TryBeginCustomizeSliderDrag(
        string elementId,
        UiPoint point,
        VirtualViewport viewport
    )
    {
        if (!_isCustomizeOpen || !TryParseCustomizeSliderIndex(elementId, out int index))
        {
            return false;
        }

        (ModCatalogEntry Entry, ModSettingDescriptor Setting)[] items = VisibleCustomizeSettings()
            .ToArray();
        if ((uint)index >= (uint)items.Length || !IsSlider(items[index].Setting))
        {
            return false;
        }

        _activeCustomizeSliderIndex = index;
        return UpdateCustomizeSliderDrag(point, viewport);
    }

    public bool UpdateCustomizeSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        if (_activeCustomizeSliderIndex is not int index)
        {
            return false;
        }

        (ModCatalogEntry Entry, ModSettingDescriptor Setting)[] items = VisibleCustomizeSettings()
            .ToArray();
        if ((uint)index >= (uint)items.Length)
        {
            _activeCustomizeSliderIndex = null;
            return false;
        }

        UiRect bounds = CustomizeSliderBounds(index, viewport);
        if (bounds.Width <= 0f)
        {
            return false;
        }

        (ModCatalogEntry entry, ModSettingDescriptor setting) = items[index];
        EnsureModSettings(entry);
        double normalized = Math.Clamp((point.X - bounds.X) / bounds.Width, 0d, 1d);
        double raw = setting.MinValue + normalized * (setting.MaxValue - setting.MinValue);
        double stepped =
            setting.Step > 0
                ? setting.MinValue
                    + setting.Step * Math.Round((raw - setting.MinValue) / setting.Step)
                : raw;
        stepped = Math.Clamp(stepped, setting.MinValue, setting.MaxValue);
        if (setting.Kind is ModSettingKind.WholeNumber or ModSettingKind.OptionalWholeNumber)
        {
            stepped = Math.Round(stepped);
        }

        _modSettings[entry.Acronym][setting.Key] = stepped.ToString(
            System.Globalization.CultureInfo.InvariantCulture
        );
        SaveModSettings();
        return true;
    }

    public void EndCustomizeSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        UpdateCustomizeSliderDrag(point, viewport);
        _activeCustomizeSliderIndex = null;
    }

    public void FocusCustomizeSettingInput(int visibleIndex, VirtualViewport viewport)
    {
        (ModCatalogEntry Entry, ModSettingDescriptor Setting)[] items = VisibleCustomizeSettings()
            .ToArray();
        if ((uint)visibleIndex >= (uint)items.Length)
        {
            return;
        }

        (ModCatalogEntry entry, ModSettingDescriptor setting) = items[visibleIndex];
        if (!setting.UseManualInput)
        {
            AdjustCustomizeSetting(visibleIndex, 1);
            return;
        }

        EnsureModSettings(entry);
        string current = _modSettings[entry.Acronym].TryGetValue(setting.Key, out string? raw)
            ? raw
            : string.Empty;
        _textInputService.RequestTextInput(
            new(
                string.Equals(current, "null", StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : current,
                text => SetCustomizeInputValue(entry, setting, text),
                text => SetCustomizeInputValue(entry, setting, text),
                viewport.ToSurface(CustomizeInputBounds(visibleIndex, viewport)),
                () => { },
                setting.Name
            )
        );
    }

    private IEnumerable<(
        ModCatalogEntry Entry,
        ModSettingDescriptor Setting
    )> VisibleCustomizeSettings()
    {
        foreach (
            ModCatalogEntry entry in ModCatalog.Entries.Where(entry =>
                _selectedAcronyms.Contains(entry.Acronym)
            )
        )
        {
            foreach (ModSettingDescriptor setting in entry.Settings ?? [])
            {
                yield return (entry, setting);
            }
        }
    }

    private void SetCustomizeInputValue(
        ModCatalogEntry entry,
        ModSettingDescriptor setting,
        string? text
    )
    {
        EnsureModSettings(entry);
        string value =
            string.IsNullOrWhiteSpace(text) && setting.IsNullable
                ? "null"
                : NormalizeSettingRawValue(setting, text ?? string.Empty);
        _modSettings[entry.Acronym][setting.Key] = value;
        SaveModSettings();
    }

    private static bool IsSlider(ModSettingDescriptor setting) =>
        setting.Kind
            is ModSettingKind.Slider
                or ModSettingKind.OptionalSlider
                or ModSettingKind.WholeNumber
                or ModSettingKind.OptionalWholeNumber
        && !setting.UseManualInput;

    private static bool TryParseCustomizeSliderIndex(string elementId, out int index)
    {
        const string prefix = "modselect-customize-slider-";
        if (!elementId.StartsWith(prefix, StringComparison.Ordinal))
        {
            index = -1;
            return false;
        }

        int dash = elementId.IndexOf('-', prefix.Length);
        ReadOnlySpan<char> span =
            dash < 0
                ? elementId.AsSpan(prefix.Length)
                : elementId.AsSpan(prefix.Length, dash - prefix.Length);
        return int.TryParse(span, out index);
    }

    private static string AdjustSettingRawValue(
        ModSettingDescriptor setting,
        string raw,
        int direction
    )
    {
        if (setting.Kind == ModSettingKind.Toggle)
        {
            return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                ? "false"
                : "true";
        }

        if (setting.Kind == ModSettingKind.Choice)
        {
            IReadOnlyList<string> values = setting.EnumValues ?? [];
            if (values.Count == 0)
            {
                return raw;
            }

            int index = Math.Max(
                0,
                values
                    .ToList()
                    .FindIndex(value =>
                        string.Equals(value, raw, StringComparison.OrdinalIgnoreCase)
                    )
            );
            return values[(index + direction + values.Count) % values.Count];
        }

        if (setting.IsNullable && string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
        {
            return direction < 0
                ? "null"
                : setting.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        double value = double.TryParse(
            raw,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out double parsed
        )
            ? parsed
            : setting.DefaultValue;
        value += setting.Step * Math.Sign(direction);
        if (setting.IsNullable && value < setting.MinValue)
        {
            return "null";
        }

        value = Math.Clamp(value, setting.MinValue, setting.MaxValue);
        if (setting.Kind is ModSettingKind.WholeNumber or ModSettingKind.OptionalWholeNumber)
        {
            value = Math.Round(value);
        }

        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string NormalizeSettingRawValue(ModSettingDescriptor setting, string raw)
    {
        return setting.Kind == ModSettingKind.Toggle
            ? string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                ? "true"
                : "false"
            : setting.IsNullable && string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase)
                ? "null"
                : raw;
    }
}
