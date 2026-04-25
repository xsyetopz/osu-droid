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

        UiRect bounds = PresetAddBounds();
        _textInputService.RequestTextInput(new TextInputRequest(
            string.Empty,
            _ => { },
            AddPreset,
            viewport.ToSurface(bounds),
            () => { },
            "Preset name"));
    }



    private void AddPreset(string name)
    {
        string normalized = name.Trim();
        if (normalized.Length == 0 || _selectedAcronyms.Count == 0)
        {
            return;
        }

        _presets.RemoveAll(preset => string.Equals(preset.Name, normalized, StringComparison.OrdinalIgnoreCase));
        _presets.Add(new ModPreset(normalized, _selectedAcronyms.Order(StringComparer.OrdinalIgnoreCase).ToArray()));
        SavePresets();
    }



    private IEnumerable<ModPreset> VisiblePresets() =>
            string.IsNullOrWhiteSpace(_appliedSearchTerm)
                ? _presets
                : _presets.Where(preset => SearchContiguously(preset.Name, _appliedSearchTerm));



    private void LoadPresets()
    {
        foreach (string line in _settingsStore.GetString(ModPresetsSettingKey, string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = line.Split('|', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            string name = Uri.UnescapeDataString(parts[0]);
            string[] acronyms = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
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
        string serialized = string.Join('\n', _presets.Select(preset => $"{Uri.EscapeDataString(preset.Name)}|{string.Join(',', preset.Acronyms)}"));
        _settingsStore.SetString(ModPresetsSettingKey, serialized);
    }



    private static UiRect PresetAddBounds() => new(SidePadding + 12f, TopBarHeight + SectionHeaderHeight, PresetSectionWidth - 24f, ToggleHeight * 0.62f);


}
