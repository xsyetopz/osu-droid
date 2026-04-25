namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
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
        }
        else
        {
            foreach (string incompatibleAcronym in ModCatalog.Entries
                .Where(candidate => !string.Equals(candidate.Acronym, acronym, StringComparison.OrdinalIgnoreCase) && !AreCompatible(entry, candidate))
                .Select(candidate => candidate.Acronym))
            {
                _selectedAcronyms.Remove(incompatibleAcronym);
            }
        }

        _selectedModsScrollX = Math.Clamp(_selectedModsScrollX, 0f, MaxSelectedModsScroll());
        SaveSelectedMods();
        return isSelected;
    }



    public void Clear()
    {
        _selectedAcronyms.Clear();
        _selectedModsScrollX = 0f;
        SaveSelectedMods();
    }



    private bool IsIncompatibleWithSelection(ModCatalogEntry entry) =>
            ModCatalog.Entries.Any(selected =>
                _selectedAcronyms.Contains(selected.Acronym) &&
                !string.Equals(selected.Acronym, entry.Acronym, StringComparison.OrdinalIgnoreCase) &&
                !AreCompatible(entry, selected));



    private static bool AreCompatible(ModCatalogEntry first, ModCatalogEntry second) =>
            !ContainsAcronym(first.IncompatibleAcronyms, second.Acronym) &&
            !ContainsAcronym(second.IncompatibleAcronyms, first.Acronym);



    private static bool ContainsAcronym(IReadOnlyList<string>? acronyms, string acronym) =>
            acronyms is not null && acronyms.Any(candidate => string.Equals(candidate, acronym, StringComparison.OrdinalIgnoreCase));



    private void LoadSelectedMods()
    {
        foreach (string acronym in _settingsStore.GetString(SelectedModsSettingKey, string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (ModCatalog.Entries.Any(entry => string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase)))
            {
                _selectedAcronyms.Add(acronym);
            }
        }
    }



    private void SaveSelectedMods() => _settingsStore.SetString(SelectedModsSettingKey, string.Join(',', _selectedAcronyms.Order(StringComparer.OrdinalIgnoreCase)));



    private static bool ModHasCustomization(string acronym) =>
            ModCatalog.Entries.Any(entry => entry.HasCustomization && string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase));



    private static ModCatalogEntry? EntryByAcronym(string acronym) =>
            ModCatalog.Entries.FirstOrDefault(entry => string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase));



    private static string FormatStat(float? value) =>
            value?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) ?? "0.00";


}
