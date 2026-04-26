using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game.Scenes.ModSelect;

internal sealed record ModPreset(string Name, IReadOnlyList<string> Acronyms)
{
    public string SafeId => string.Concat(Name.Where(char.IsLetterOrDigit));
}

public sealed partial class ModSelectScene
{
    public void FocusPresetName(VirtualViewport viewport)
    {
        if (_selectedAcronyms.Count == 0)
        {
            return;
        }

        _lastViewport = viewport;
        _isPresetFormOpen = true;
        _isPresetDeleteDialogOpen = false;
        _pendingPresetDeleteIndex = -1;
        _presetNameInput = string.Empty;
        FocusPresetDialogName(viewport);
    }

    public void FocusPresetDialogName(VirtualViewport viewport)
    {
        if (!_isPresetFormOpen)
        {
            return;
        }

        UiRect bounds = PresetNameInputBounds(viewport);
        _textInputService.RequestTextInput(
            new TextInputRequest(
                _presetNameInput,
                value => _presetNameInput = value,
                value => _presetNameInput = value,
                viewport.ToSurface(bounds),
                () => { },
                "Name"
            )
        );
    }

    public void SavePresetDialog()
    {
        if (!_isPresetFormOpen)
        {
            return;
        }

        AddPreset(_presetNameInput);
        ClosePresetDialog();
    }

    public void CancelPresetDialog() => ClosePresetDialog();

    public void ActivatePreset(int visibleIndex)
    {
        ModPreset? preset = VisiblePresetAt(visibleIndex);
        if (preset is null)
        {
            return;
        }

        if (
            preset.Acronyms.Count == _selectedAcronyms.Count
            && preset.Acronyms.All(_selectedAcronyms.Contains)
        )
        {
            Clear();
            return;
        }

        _selectedAcronyms.Clear();
        foreach (string acronym in preset.Acronyms)
        {
            if (EntryByAcronym(acronym) is not null)
            {
                _selectedAcronyms.Add(acronym);
            }
        }

        SaveSelectedMods();
        _selectedModsScrollX = Math.Clamp(_selectedModsScrollX, 0f, MaxSelectedModsScroll());
    }

    public bool OpenPresetDeleteDialog(int visibleIndex)
    {
        if (VisiblePresetAt(visibleIndex) is null)
        {
            return false;
        }

        _isPresetFormOpen = false;
        _isPresetDeleteDialogOpen = true;
        _pendingPresetDeleteIndex = visibleIndex;
        _textInputService.HideTextInput();
        return true;
    }

    public void ConfirmPresetDelete()
    {
        ModPreset? preset = VisiblePresetAt(_pendingPresetDeleteIndex);
        if (preset is not null)
        {
            _presets.Remove(preset);
            SavePresets();
        }

        ClosePresetDialog();
    }

    public void ClosePresetDialog()
    {
        _isPresetFormOpen = false;
        _isPresetDeleteDialogOpen = false;
        _pendingPresetDeleteIndex = -1;
        _presetNameInput = string.Empty;
        _textInputService.HideTextInput();
    }

    private void AddPreset(string name)
    {
        string normalized = name.Trim();
        if (normalized.Length == 0 || _selectedAcronyms.Count == 0)
        {
            return;
        }

        _presets.RemoveAll(preset =>
            string.Equals(preset.Name, normalized, StringComparison.OrdinalIgnoreCase)
        );
        _presets.Add(
            new ModPreset(
                normalized,
                _selectedAcronyms.Order(StringComparer.OrdinalIgnoreCase).ToArray()
            )
        );
        SavePresets();
    }

    private IEnumerable<ModPreset> VisiblePresets() =>
        string.IsNullOrWhiteSpace(_appliedSearchTerm)
            ? _presets
            : _presets.Where(preset => SearchContiguously(preset.Name, _appliedSearchTerm));

    private ModPreset? VisiblePresetAt(int visibleIndex) =>
        (uint)visibleIndex < VisiblePresetActionLimit
            ? VisiblePresets().Skip(visibleIndex).FirstOrDefault()
            : null;

    private void LoadPresets()
    {
        foreach (
            string line in _settingsStore
                .GetString(ModPresetsSettingKey, string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        )
        {
            string[] parts = line.Split('|', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            string name = Uri.UnescapeDataString(parts[0]);
            string[] acronyms = parts[1]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(acronym => EntryByAcronym(acronym) is not null)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (name.Length > 0 && acronyms.Length > 0)
            {
                _presets.Add(new ModPreset(name, acronyms));
            }
        }
    }

    private void SavePresets()
    {
        string serialized = string.Join(
            '\n',
            _presets.Select(preset =>
                $"{Uri.EscapeDataString(preset.Name)}|{string.Join(',', preset.Acronyms)}"
            )
        );
        _settingsStore.SetString(ModPresetsSettingKey, serialized);
    }

    private static UiRect PresetNameInputBounds(VirtualViewport viewport)
    {
        UiRect panel = PresetDialogPanelBounds(viewport);
        return new UiRect(panel.X + 24f, panel.Y + 126f, panel.Width - 48f, 48f);
    }

    private static UiRect PresetDialogPanelBounds(VirtualViewport viewport)
    {
        float width = viewport.VirtualWidth * 0.5f;
        const float height = 286f;
        return new UiRect(
            (viewport.VirtualWidth - width) / 2f,
            (viewport.VirtualHeight - height) / 2f,
            width,
            height
        );
    }
}
