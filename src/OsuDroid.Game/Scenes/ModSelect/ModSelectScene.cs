using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed class ModSelectScene
{
    public const string SelectedModsSettingKey = "selectedMods";
    private const float TopBarHeight = 84f;
    private const float BottomBarHeight = 84f;
    private const float SidePadding = 60f;
    private const float SectionWidth = 340f;
    private const float SectionGap = 16f;
    private const float SectionHeaderHeight = 54f;
    private const float ToggleHeight = 82f;
    private const float ToggleGap = 16f;
    private const float TogglePaddingX = 12f;
    private const float ToggleIconSize = 38f;
    private const float SelectedModIconSize = 42f;
    private const float SelectedModIconSpacing = -5f;
    private const float SearchDebounceSeconds = 0.2f;

    private static readonly UiColor s_background = new(24, 24, 38, 230);
    private static readonly UiColor s_panel = new(32, 32, 52, 255);
    private static readonly UiColor s_button = new(58, 58, 88, 255);
    private static readonly UiColor s_selected = new(243, 115, 115, 255);
    private static readonly UiColor s_selectedText = new(24, 24, 34, 255);
    private static readonly UiColor s_clearButton = new(52, 33, 33, 255);
    private static readonly UiColor s_accent = new(194, 202, 255, 255);
    private static readonly UiColor s_ranked = new(131, 223, 107, 255);
    private static readonly UiColor s_text = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor s_dimText = UiColor.Opaque(184, 184, 208);

    private readonly IGameSettingsStore _settingsStore;
    private readonly GameLocalizer _localizer;
    private readonly HashSet<string> _selectedAcronyms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, float> _sectionScrolls = new(StringComparer.Ordinal);
    private ITextInputService _textInputService;
    private string _searchInputText = string.Empty;
    private string _appliedSearchTerm = string.Empty;
    private float _searchDebounceElapsed = SearchDebounceSeconds;
    private double _elapsedSeconds;
    private float _sectionRailScrollX;
    private float _selectedModsScrollX;

    public ModSelectScene(IGameSettingsStore settingsStore, ITextInputService textInputService, GameLocalizer? localizer = null)
    {
        _settingsStore = settingsStore;
        _textInputService = textInputService;
        _localizer = localizer ?? new GameLocalizer();
        LoadSelectedMods();
    }

    public IReadOnlyCollection<string> SelectedAcronyms => _selectedAcronyms;

    public float ScoreMultiplier => ModCatalog.Entries
        .Where(entry => _selectedAcronyms.Contains(entry.Acronym))
        .Aggregate(1f, (multiplier, entry) => multiplier * entry.ScoreMultiplier);

    public void SetTextInputService(ITextInputService service) => _textInputService = service;

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new(
        "ModSelect",
        "Mods",
        "legacy mod menu",
        [],
        0,
        false,
        CreateUiFrame(viewport));

    public void Update(TimeSpan elapsed)
    {
        _elapsedSeconds += elapsed.TotalSeconds;
        if (_searchInputText != _appliedSearchTerm)
        {
            _searchDebounceElapsed += (float)elapsed.TotalSeconds;
            if (_searchDebounceElapsed >= SearchDebounceSeconds)
            {
                _appliedSearchTerm = _searchInputText;
                ClampAllScrolls(VirtualViewport.LegacyLandscape);
            }
        }
    }

    public void Scroll(float deltaX, float deltaY, UiPoint startPoint, VirtualViewport viewport)
    {
        if (SelectedModsBounds().Contains(startPoint))
        {
            _selectedModsScrollX = Math.Clamp(_selectedModsScrollX + deltaX, 0f, MaxSelectedModsScroll());
            return;
        }

        UiRect rail = SectionRailBounds(viewport);
        if (!rail.Contains(startPoint))
        {
            return;
        }

        if (MathF.Abs(deltaX) > MathF.Abs(deltaY))
        {
            _sectionRailScrollX = Math.Clamp(_sectionRailScrollX + deltaX, 0f, MaxSectionRailScroll(viewport));
            return;
        }

        string? sectionKey = SectionKeyAt(startPoint, viewport);
        if (sectionKey is null)
        {
            return;
        }

        _sectionScrolls[sectionKey] = Math.Clamp(SectionScroll(sectionKey) + deltaY, 0f, MaxSectionScroll(sectionKey, viewport));
    }

    public bool ToggleMod(int index)
    {
        if (index < 0 || index >= ModCatalog.Entries.Count)
        {
            return false;
        }

        string acronym = ModCatalog.Entries[index].Acronym;
        bool isSelected = _selectedAcronyms.Add(acronym);
        if (!isSelected)
        {
            _selectedAcronyms.Remove(acronym);
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

    public void FocusSearch(VirtualViewport viewport)
    {
        _textInputService.RequestTextInput(new TextInputRequest(
            _searchInputText,
            SetSearchTerm,
            SetSearchTerm,
            viewport.ToSurface(SearchBounds(viewport)),
            () => { },
            "Search..."));
    }

    public void SetSearchTerm(string term)
    {
        string nextTerm = term.Trim();
        if (string.Equals(_searchInputText, nextTerm, StringComparison.Ordinal))
        {
            return;
        }

        _searchInputText = nextTerm;
        _searchDebounceElapsed = 0f;
    }

    public void ApplySearchNow()
    {
        _appliedSearchTerm = _searchInputText;
        _searchDebounceElapsed = SearchDebounceSeconds;
        ClampAllScrolls(VirtualViewport.LegacyLandscape);
    }

    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport)
    {
        ClampAllScrolls(viewport);
        var elements = new List<UiElementSnapshot>
        {
            Fill("modselect-background", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_background),
        };

        AddTopBar(elements, viewport);
        AddSections(elements, viewport);
        AddBottomBar(elements, viewport);
        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private void AddTopBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddButton(elements, "modselect-back", new UiRect(SidePadding, 12f, 120f, 58f), _localizer["LegacyLanguagePack_menu_mod_back"], UiAction.ModSelectBack, leadingIcon: UiMaterialIcon.ArrowBack);
        AddButton(elements, "modselect-customize", new UiRect(190f, 12f, 170f, 58f), "Customize", UiAction.ModSelectCustomize, _selectedAcronyms.Any(ModHasCustomization), UiMaterialIcon.Tune);
        AddButton(elements, "modselect-clear", new UiRect(370f, 12f, 120f, 58f), "Clear", UiAction.ModSelectClear, true, UiMaterialIcon.Delete, s_clearButton, new UiColor(255, 191, 191, 255));
        AddSelectedModsIndicator(elements);

        UiRect search = SearchBounds(viewport);
        elements.Add(Fill("modselect-search", search, s_panel, 1f, UiAction.ModSelectSearchBox, 12f));
        elements.Add(Text("modselect-search-text", string.IsNullOrWhiteSpace(_searchInputText) ? "Search..." : _searchInputText, new UiRect(search.X + 18f, search.Y + 15f, search.Width - 74f, 28f), 24f, string.IsNullOrWhiteSpace(_searchInputText) ? s_dimText : s_text, UiAction.ModSelectSearchBox, clipToBounds: true));
        elements.Add(MaterialIcon("modselect-search-icon", UiMaterialIcon.Search, new UiRect(search.Right - 62f, search.Y + 15f, 52f, 28f), s_accent, 1f, UiAction.ModSelectSearchBox));
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
                AddModIcon(elements, $"modselect-selected-{acronym}", acronym, icon, UiAction.None, _selectedAcronyms.Contains(acronym));
            }

            x += SelectedModIconSize + SelectedModIconSpacing;
        }
    }

    private void AddSections(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float x = SidePadding - _sectionRailScrollX;
        foreach (IGrouping<string, ModCatalogEntry> section in VisibleEntries().GroupBy(entry => entry.SectionKey))
        {
            UiRect sectionBounds = new(x, TopBarHeight, SectionWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight);
            if (sectionBounds.Right >= 0f && sectionBounds.X <= viewport.VirtualWidth)
            {
                AddSection(elements, section.Key, section.ToList(), sectionBounds);
            }

            x += SectionWidth + SectionGap;
        }
    }

    private void AddSection(List<UiElementSnapshot> elements, string sectionKey, IReadOnlyList<ModCatalogEntry> entries, UiRect bounds)
    {
        elements.Add(Fill($"modselect-section-{sectionKey}", bounds, s_panel, 1f, radius: 16f));
        elements.Add(Text($"modselect-section-title-{sectionKey}", _localizer[sectionKey], new UiRect(bounds.X + 12f, bounds.Y + 12f, bounds.Width - 24f, 30f), 21f, s_accent, bold: true, alignment: UiTextAlignment.Center, alpha: 0.75f));

        float y = bounds.Y + SectionHeaderHeight - SectionScroll(sectionKey);
        float listTop = bounds.Y + SectionHeaderHeight;
        float listBottom = bounds.Bottom - 12f;
        foreach (ModCatalogEntry entry in entries)
        {
            int index = IndexOfEntry(entry);
            if (UiActionGroups.TryGetModSelectToggleAction(index, out UiAction action) && y >= listTop && y + ToggleHeight <= listBottom)
            {
                AddToggle(elements, entry, action, bounds.X + 12f, y, bounds.Width - 24f);
            }

            y += ToggleHeight + ToggleGap;
        }
    }

    private void AddToggle(List<UiElementSnapshot> elements, ModCatalogEntry entry, UiAction action, float x, float y, float width)
    {
        bool selected = _selectedAcronyms.Contains(entry.Acronym);
        UiColor fill = selected ? s_selected : s_button;
        UiColor text = selected ? s_selectedText : s_text;
        UiColor dimText = selected ? s_selectedText : s_dimText;
        elements.Add(Fill($"modselect-toggle-{entry.Acronym}", new UiRect(x, y, width, ToggleHeight), fill, 1f, action, 12f));
        AddModIcon(elements, $"modselect-toggle-icon-{entry.Acronym}", entry.Acronym, new UiRect(x + TogglePaddingX, y + (ToggleHeight - ToggleIconSize) / 2f, ToggleIconSize, ToggleIconSize), action, selected);
        float textX = x + TogglePaddingX + ToggleIconSize + 8f;
        float textWidth = width - TogglePaddingX * 2f - ToggleIconSize - 8f;
        elements.Add(Text($"modselect-toggle-name-{entry.Acronym}", entry.Name, new UiRect(textX, y + 17f, textWidth, 26f), 21f, text, action, clipToBounds: true));
        elements.Add(Text($"modselect-toggle-description-{entry.Acronym}", entry.Description, new UiRect(textX, y + 44f, textWidth, 20f), 16f, dimText, action, alpha: 0.75f, clipToBounds: true, autoScroll: true));
    }

    private void AddModIcon(List<UiElementSnapshot> elements, string id, string acronym, UiRect bounds, UiAction action, bool selected)
    {
        elements.Add(Fill(id, bounds, selected ? s_selectedText : new UiColor(24, 24, 38, 255), 1f, action, bounds.Height * 0.2f));
        elements.Add(Text($"{id}-text", acronym, bounds, bounds.Height * 0.42f, selected ? s_selected : s_accent, action, true, UiTextAlignment.Center));
    }

    private void AddBottomBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float y = viewport.VirtualHeight - BottomBarHeight + 12f;
        AddBadge(elements, "ar", "AR", "0.00", SidePadding, y);
        AddBadge(elements, "od", "OD", "0.00", SidePadding + 88f, y);
        AddBadge(elements, "cs", "CS", "0.00", SidePadding + 176f, y);
        AddBadge(elements, "hp", "HP", "0.00", SidePadding + 264f, y);
        AddBadge(elements, "bpm", "BPM", "0", SidePadding + 352f, y);

        float scoreX = viewport.VirtualWidth - SidePadding - 170f;
        AddBadge(elements, "score", "Score", ScoreMultiplier.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "x", scoreX, y, 170f);
        elements.Add(Fill("modselect-ranked-badge", new UiRect(scoreX - 112f, y, 102f, 44f), s_ranked, 0.75f, radius: 12f));
        elements.Add(Text("modselect-ranked-badge-text", "Ranked", new UiRect(scoreX - 102f, y + 9f, 82f, 26f), 17f, new UiColor(22, 22, 34, 255), bold: true, alignment: UiTextAlignment.Center));
        elements.Add(Fill("modselect-star-badge", new UiRect(scoreX - 222f, y, 100f, 44f), s_button, 1f, radius: 12f));
        elements.Add(MaterialIcon("modselect-star-icon", UiMaterialIcon.Star, new UiRect(scoreX - 212f, y + 8f, 28f, 28f), s_selected));
        elements.Add(Text("modselect-star-value", "0.00", new UiRect(scoreX - 176f, y + 10f, 46f, 24f), 18f, s_text, alignment: UiTextAlignment.Center));
    }

    private void AddBadge(List<UiElementSnapshot> elements, string id, string label, string value, float x, float y, float width = 76f)
    {
        elements.Add(Fill($"modselect-stat-{id}", new UiRect(x, y, width, 44f), s_button, 1f, radius: 12f));
        elements.Add(Text($"modselect-stat-{id}-label", label, new UiRect(x + 8f, y + 5f, width - 16f, 16f), 12f, s_dimText, alignment: UiTextAlignment.Center));
        elements.Add(Text($"modselect-stat-{id}-value", value, new UiRect(x + 8f, y + 20f, width - 16f, 20f), 16f, s_text, alignment: UiTextAlignment.Center));
    }

    private IEnumerable<ModCatalogEntry> VisibleEntries()
    {
        return string.IsNullOrWhiteSpace(_appliedSearchTerm)
            ? ModCatalog.Entries
            : ModCatalog.Entries.Where(entry =>
            string.Equals(entry.Acronym, _appliedSearchTerm, StringComparison.OrdinalIgnoreCase) ||
            SearchContiguously(entry.Name, _appliedSearchTerm));
    }

    private void ClampAllScrolls(VirtualViewport viewport)
    {
        _sectionRailScrollX = Math.Clamp(_sectionRailScrollX, 0f, MaxSectionRailScroll(viewport));
        _selectedModsScrollX = Math.Clamp(_selectedModsScrollX, 0f, MaxSelectedModsScroll());
        foreach (string sectionKey in _sectionScrolls.Keys.ToArray())
        {
            _sectionScrolls[sectionKey] = Math.Clamp(_sectionScrolls[sectionKey], 0f, MaxSectionScroll(sectionKey, viewport));
        }
    }

    private float MaxSectionRailScroll(VirtualViewport viewport)
    {
        int sectionCount = VisibleEntries().GroupBy(entry => entry.SectionKey).Count();
        if (sectionCount == 0)
        {
            return 0f;
        }

        float contentWidth = SidePadding * 2f + sectionCount * SectionWidth + Math.Max(0, sectionCount - 1) * SectionGap;
        return Math.Max(0f, contentWidth - viewport.VirtualWidth);
    }

    private float MaxSectionScroll(string sectionKey, VirtualViewport viewport)
    {
        int entryCount = VisibleEntries().Count(entry => string.Equals(entry.SectionKey, sectionKey, StringComparison.Ordinal));
        float contentHeight = entryCount * ToggleHeight + Math.Max(0, entryCount - 1) * ToggleGap + 12f;
        float listHeight = viewport.VirtualHeight - TopBarHeight - BottomBarHeight - SectionHeaderHeight;
        return Math.Max(0f, contentHeight - listHeight);
    }

    private float MaxSelectedModsScroll()
    {
        int count = _selectedAcronyms.Count;
        float contentWidth = count == 0 ? 0f : count * SelectedModIconSize + Math.Max(0, count - 1) * SelectedModIconSpacing;
        return Math.Max(0f, contentWidth - SelectedModsBounds().Width);
    }

    private float SectionScroll(string sectionKey) => _sectionScrolls.TryGetValue(sectionKey, out float scroll) ? scroll : 0f;

    private string? SectionKeyAt(UiPoint point, VirtualViewport viewport)
    {
        float x = SidePadding - _sectionRailScrollX;
        foreach (IGrouping<string, ModCatalogEntry> section in VisibleEntries().GroupBy(entry => entry.SectionKey))
        {
            if (new UiRect(x, TopBarHeight, SectionWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight).Contains(point))
            {
                return section.Key;
            }

            x += SectionWidth + SectionGap;
        }

        return null;
    }

    private static bool SearchContiguously(string text, string searchTerm)
    {
        int searchIndex = 0;
        foreach (char candidate in text)
        {
            if (char.ToUpperInvariant(candidate) == char.ToUpperInvariant(searchTerm[searchIndex]))
            {
                searchIndex++;
                if (searchIndex == searchTerm.Length)
                {
                    return true;
                }
            }
        }

        return false;
    }

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

    private static UiRect SearchBounds(VirtualViewport viewport) => new(viewport.VirtualWidth - 460f, 12f, 400f, 58f);

    private static UiRect SelectedModsBounds() => new(506f, 12f, 340f, 58f);

    private static UiRect SectionRailBounds(VirtualViewport viewport) => new(0f, TopBarHeight, viewport.VirtualWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight);

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) =>
        UiElementFactory.Fill(id, bounds, color, alpha, action, radius);

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action);

    private UiElementSnapshot Text(string id, string text, UiRect bounds, float size, UiColor color, UiAction action = UiAction.None, bool bold = false, UiTextAlignment alignment = UiTextAlignment.Left, float alpha = 1f, bool clipToBounds = false, bool autoScroll = false) =>
        UiElementFactory.Text(id, text, bounds, size, color, action, bold: bold, alignment: alignment, verticalAlignment: UiTextVerticalAlignment.Middle, alpha: alpha, clipToBounds: clipToBounds, autoScroll: autoScroll ? new UiTextAutoScroll(_elapsedSeconds) : null);

    private static int IndexOfEntry(ModCatalogEntry entry)
    {
        for (int index = 0; index < ModCatalog.Entries.Count; index++)
        {
            if (ReferenceEquals(ModCatalog.Entries[index], entry))
            {
                return index;
            }
        }

        return -1;
    }

    private void AddButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, bool isEnabled = true, UiMaterialIcon? leadingIcon = null, UiColor? fillOverride = null, UiColor? textOverride = null)
    {
        UiColor fill = fillOverride ?? (isEnabled ? s_button : new UiColor(s_button.Red, s_button.Green, s_button.Blue, 120));
        UiColor color = textOverride ?? (isEnabled ? s_text : s_dimText);
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            bounds,
            fill,
            1f,
            Action: isEnabled ? action : UiAction.None,
            IsEnabled: isEnabled,
            CornerRadius: 12f));

        float textX = bounds.X;
        float textWidth = bounds.Width;
        if (leadingIcon is not null)
        {
            elements.Add(MaterialIcon($"{id}-icon", leadingIcon.Value, new UiRect(bounds.X + 16f, bounds.Y + 15f, 28f, 28f), color, 1f, isEnabled ? action : UiAction.None));
            textX += 28f;
            textWidth -= 34f;
        }

        elements.Add(Text($"{id}-text", text, new UiRect(textX, bounds.Y, textWidth, bounds.Height), 22f, color, isEnabled ? action : UiAction.None, true, UiTextAlignment.Center, clipToBounds: true));
    }
}
