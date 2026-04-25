using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.ModSelect;

internal sealed record ModPreset(string Name, IReadOnlyList<string> Acronyms)
{
    public string SafeId => string.Concat(Name.Where(char.IsLetterOrDigit));
}

internal enum ModScrollAxis
{
    Horizontal,
    Vertical,
    Undecided,
}

internal sealed record ScrollDragTarget(string Key, ModScrollAxis Axis, UiPoint LastPoint, double LastTimestampSeconds);

public sealed class ModSelectScene
{
    public const string SelectedModsSettingKey = "selectedMods";
    public const string ModPresetsSettingKey = "modPresets";
    private const float TopBarHeight = 84f;
    private const float BottomBarHeight = 68f;
    private const float SidePadding = 60f;
    private const float PresetSectionWidth = 300f;
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
    private static readonly UiColor s_accent = DroidUiTheme.ModMenu.Accent;
    private static readonly UiColor s_panel = DroidUiTheme.ModMenu.Panel;
    private static readonly UiColor s_badge = DroidUiTheme.ModMenu.Badge;
    private static readonly UiColor s_button = DroidUiTheme.ModMenu.Button;
    private static readonly UiColor s_search = DroidUiTheme.ModMenu.Search;
    private static readonly UiColor s_searchPlaceholder = DroidUiTheme.ModMenu.SearchPlaceholder;
    private static readonly UiColor s_selectedCard = s_accent;
    private static readonly UiColor s_selected = DroidUiTheme.ModMenu.Selected;
    private static readonly UiColor s_selectedText = DroidUiTheme.ModMenu.SelectedText;
    private static readonly UiColor s_clearButton = DroidUiTheme.ModMenu.ClearButton;
    private static readonly UiColor s_ranked = DroidUiTheme.ModMenu.Ranked;
    private static readonly UiColor s_text = DroidUiTheme.TextPrimary;
    private static readonly UiColor s_dimText = DroidUiTheme.TextSecondary;
    private static readonly UiColor s_black = DroidUiTheme.Black;

    private readonly IGameSettingsStore _settingsStore;
    private readonly GameLocalizer _localizer;
    private readonly HashSet<string> _selectedAcronyms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, float> _sectionScrolls = new(StringComparer.Ordinal);
    private readonly List<ModPreset> _presets = [];
    private ITextInputService _textInputService;
    private string _searchInputText = string.Empty;
    private string _appliedSearchTerm = string.Empty;
    private float _searchDebounceElapsed = SearchDebounceSeconds;
    private double _elapsedSeconds;
    private float _sectionRailScrollX;
    private float _selectedModsScrollX;
    private float _railVelocityX;
    private float _selectedModsVelocityX;
    private readonly Dictionary<string, float> _sectionVelocities = new(StringComparer.Ordinal);
    private double _railScrollbarVisibleUntil;
    private double _selectedModsScrollbarVisibleUntil;
    private readonly Dictionary<string, double> _sectionScrollbarVisibleUntil = new(StringComparer.Ordinal);
    private ScrollDragTarget? _dragTarget;
    private BeatmapInfo? _selectedBeatmap;
    private VirtualViewport _lastViewport = VirtualViewport.LegacyLandscape;

    public ModSelectScene(IGameSettingsStore settingsStore, ITextInputService textInputService, GameLocalizer? localizer = null)
    {
        _settingsStore = settingsStore;
        _textInputService = textInputService;
        _localizer = localizer ?? new GameLocalizer();
        LoadSelectedMods();
        LoadPresets();
    }

    public IReadOnlyCollection<string> SelectedAcronyms => _selectedAcronyms;

    public float ScoreMultiplier => ModCatalog.Entries
        .Where(entry => _selectedAcronyms.Contains(entry.Acronym))
        .Aggregate(1f, (multiplier, entry) => multiplier * entry.ScoreMultiplier);

    public bool IsRanked => _selectedAcronyms.Count == 0 || ModCatalog.Entries
        .Where(entry => _selectedAcronyms.Contains(entry.Acronym))
        .All(entry => entry.IsRanked);

    public void SetTextInputService(ITextInputService service) => _textInputService = service;

    public void SetSelectedBeatmap(BeatmapInfo? beatmap) => _selectedBeatmap = beatmap;

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new(
        "ModSelect",
        "Mods",
        "legacy mod menu",
        [],
        0,
        false,
        CreateUiFrame(viewport));

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport, UiFrameSnapshot parentFrame) => new(
        "ModSelect",
        "Mods",
        "legacy mod menu",
        [],
        0,
        false,
        CreateUiFrame(viewport, parentFrame));

    public void Update(TimeSpan elapsed)
    {
        float elapsedSeconds = (float)elapsed.TotalSeconds;
        _elapsedSeconds += elapsedSeconds;
        UpdateScrollInertia(elapsedSeconds, _lastViewport);
        if (_searchInputText != _appliedSearchTerm)
        {
            _searchDebounceElapsed += elapsedSeconds;
            if (_searchDebounceElapsed >= SearchDebounceSeconds)
            {
                _appliedSearchTerm = _searchInputText;
                ClampAllScrolls(_lastViewport);
            }
        }
    }

    public void Scroll(float deltaX, float deltaY, UiPoint startPoint, VirtualViewport viewport)
    {
        _lastViewport = viewport;
        if (SelectedModsBounds().Contains(startPoint))
        {
            _selectedModsVelocityX = 0f;
            _selectedModsScrollX = Math.Clamp(_selectedModsScrollX + deltaX, 0f, MaxSelectedModsScroll());
            ShowSelectedModsScrollbar();
            return;
        }

        UiRect rail = SectionRailBounds(viewport);
        if (!rail.Contains(startPoint))
        {
            return;
        }

        if (MathF.Abs(deltaX) > MathF.Abs(deltaY))
        {
            _railVelocityX = 0f;
            _sectionRailScrollX = Math.Clamp(_sectionRailScrollX + deltaX, 0f, MaxSectionRailScroll(viewport));
            ShowRailScrollbar();
            return;
        }

        string? sectionKey = SectionKeyAt(startPoint, viewport);
        if (sectionKey is null)
        {
            return;
        }

        _sectionVelocities[sectionKey] = 0f;
        _sectionScrolls[sectionKey] = Math.Clamp(SectionScroll(sectionKey) + deltaY, 0f, MaxSectionScroll(sectionKey, viewport));
        ShowSectionScrollbar(sectionKey);
    }

    public bool TryBeginScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        _lastViewport = viewport;
        double timestamp = timestampSeconds ?? _elapsedSeconds;
        if (SelectedModsBounds().Contains(point) && MaxSelectedModsScroll() > 0f)
        {
            _selectedModsVelocityX = 0f;
            _dragTarget = new ScrollDragTarget("selected", ModScrollAxis.Horizontal, point, timestamp);
            ShowSelectedModsScrollbar();
            return true;
        }

        if (!SectionRailBounds(viewport).Contains(point))
        {
            return false;
        }

        string? sectionKey = SectionKeyAt(point, viewport);
        if (sectionKey is not null && (MaxSectionScroll(sectionKey, viewport) > 0f || MaxSectionRailScroll(viewport) > 0f))
        {
            _sectionVelocities[sectionKey] = 0f;
            _railVelocityX = 0f;
            _dragTarget = new ScrollDragTarget(sectionKey, ModScrollAxis.Undecided, point, timestamp);
            return true;
        }

        if (MaxSectionRailScroll(viewport) <= 0f)
        {
            return false;
        }

        _railVelocityX = 0f;
        _dragTarget = new ScrollDragTarget("rail", ModScrollAxis.Horizontal, point, timestamp);
        ShowRailScrollbar();
        return true;
    }

    public bool UpdateScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        _lastViewport = viewport;
        if (_dragTarget is not { } target)
        {
            return false;
        }

        double timestamp = timestampSeconds ?? _elapsedSeconds;
        float deltaX = target.LastPoint.X - point.X;
        float deltaY = target.LastPoint.Y - point.Y;
        float elapsed = Math.Max(1f / 120f, (float)(timestamp - target.LastTimestampSeconds));

        if (target.Key == "selected")
        {
            _selectedModsScrollX = Math.Clamp(_selectedModsScrollX + deltaX, 0f, MaxSelectedModsScroll());
            _selectedModsVelocityX = Math.Clamp(deltaX / elapsed, -DroidUiTheme.Scroll.MaxVelocity, DroidUiTheme.Scroll.MaxVelocity);
            ShowSelectedModsScrollbar();
        }
        else if (target.Key == "rail" || target.Axis == ModScrollAxis.Horizontal || (target.Axis == ModScrollAxis.Undecided && MathF.Abs(deltaX) > MathF.Abs(deltaY)))
        {
            _sectionRailScrollX = Math.Clamp(_sectionRailScrollX + deltaX, 0f, MaxSectionRailScroll(viewport));
            _railVelocityX = Math.Clamp(deltaX / elapsed, -DroidUiTheme.Scroll.MaxVelocity, DroidUiTheme.Scroll.MaxVelocity);
            ShowRailScrollbar();
            target = target with { Key = "rail", Axis = ModScrollAxis.Horizontal };
        }
        else
        {
            _sectionScrolls[target.Key] = Math.Clamp(SectionScroll(target.Key) + deltaY, 0f, MaxSectionScroll(target.Key, viewport));
            _sectionVelocities[target.Key] = Math.Clamp(deltaY / elapsed, -DroidUiTheme.Scroll.MaxVelocity, DroidUiTheme.Scroll.MaxVelocity);
            ShowSectionScrollbar(target.Key);
            target = target with { Axis = ModScrollAxis.Vertical };
        }

        _dragTarget = target with { LastPoint = point, LastTimestampSeconds = timestamp };
        return true;
    }

    public void EndScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        _lastViewport = viewport;
        if (_dragTarget is null)
        {
            return;
        }

        UpdateScrollDrag(point, viewport, timestampSeconds);
        ClampAllScrolls(viewport);
        _dragTarget = null;
    }

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
        _lastViewport = viewport;
        ClampAllScrolls(viewport);
        var elements = new List<UiElementSnapshot>
        {
            Fill("modselect-background", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_panel, 0.9f),
        };

        AddTopBar(elements, viewport);
        AddSections(elements, viewport);
        AddBottomBar(elements, viewport);
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
        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private void AddTopBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddButton(elements, "modselect-back", new UiRect(SidePadding, 12f, 120f, 58f), _localizer["LegacyLanguagePack_menu_mod_back"], UiAction.ModSelectBack, leadingAsset: DroidAssets.CommonBackArrow);
        AddButton(elements, "modselect-customize", new UiRect(190f, 12f, 170f, 58f), "Customize", UiAction.ModSelectCustomize, _selectedAcronyms.Any(ModHasCustomization), leadingAsset: DroidAssets.CommonTune);
        AddButton(elements, "modselect-clear", new UiRect(370f, 12f, 120f, 58f), "Clear", UiAction.ModSelectClear, true, leadingAsset: DroidAssets.CommonBackspace, fillOverride: s_clearButton, textOverride: DroidUiColors.DangerText);
        AddSelectedModsIndicator(elements);

        UiRect search = SearchBounds(viewport);
        elements.Add(Fill("modselect-search", search, s_search, 1f, UiAction.ModSelectSearchBox, 12f));
        elements.Add(Text("modselect-search-text", string.IsNullOrWhiteSpace(_searchInputText) ? "Search..." : _searchInputText, new UiRect(search.X + 18f, search.Y + 15f, search.Width - 74f, 28f), 24f, string.IsNullOrWhiteSpace(_searchInputText) ? s_searchPlaceholder : s_accent, UiAction.ModSelectSearchBox, clipToBounds: true));
        elements.Add(UiElementFactory.Sprite("modselect-search-icon", DroidAssets.CommonSearchSmall, new UiRect(search.Right - 46f, search.Y + 12f, 34f, 34f), s_accent, 1f, UiAction.ModSelectSearchBox, spriteFit: UiSpriteFit.Contain));
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
                    AddModIcon(elements, $"modselect-selected-{acronym}", entry, icon, UiAction.None);
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
        elements.Add(Fill("modselect-section-presets", bounds, s_panel, 0.9f, radius: 16f));
        elements.Add(Text("modselect-section-title-presets", "Presets", new UiRect(bounds.X + 12f, bounds.Y + 12f, bounds.Width - 24f, 30f), 21f, s_accent, bold: true, alignment: UiTextAlignment.Center, alpha: 0.75f));

        UiRect addBounds = new(bounds.X + 12f, bounds.Y + SectionHeaderHeight, bounds.Width - 24f, ToggleHeight * 0.62f);
        UiRect clip = ListClipBounds(bounds);
        float yOffset = SectionScroll("presets");
        addBounds = addBounds with { Y = addBounds.Y - yOffset };
        AddButton(elements, "modselect-preset-add", addBounds, "Add preset", UiAction.ModSelectPresetAdd, _selectedAcronyms.Count > 0, leadingIcon: UiMaterialIcon.Plus, clipBounds: clip);

        float y = addBounds.Bottom + ToggleGap;
        foreach (ModPreset preset in VisiblePresets())
        {
            if (IntersectsVertically(y, ToggleHeight, clip))
            {
                elements.Add(Fill($"modselect-preset-{preset.SafeId}", new UiRect(bounds.X + 12f, y, bounds.Width - 24f, ToggleHeight), s_button, 1f, radius: 12f) with { ClipBounds = clip });
                elements.Add(Text($"modselect-preset-{preset.SafeId}-name", preset.Name, new UiRect(bounds.X + 24f, y + 10f, bounds.Width - 48f, 28f), 20f, s_text, clipToBounds: true) with { ClipBounds = clip });
                float iconX = bounds.X + 24f;
                foreach (string acronym in preset.Acronyms.Take(8))
                {
                    ModCatalogEntry? entry = EntryByAcronym(acronym);
                    if (entry is not null)
                    {
                        AddModIcon(elements, $"modselect-preset-{preset.SafeId}-{acronym}", entry, new UiRect(iconX, y + 46f, 21f, 21f), UiAction.None, clip);
                        iconX += 19f;
                    }
                }
            }

            y += ToggleHeight + ToggleGap;
        }

        AddVerticalScrollbar(elements, "modselect-section-presets", bounds, "presets", viewport);
    }

    private void AddSection(List<UiElementSnapshot> elements, string sectionKey, IReadOnlyList<ModCatalogEntry> entries, UiRect bounds, VirtualViewport viewport)
    {
        elements.Add(Fill($"modselect-section-{sectionKey}", bounds, s_panel, 0.9f, radius: 16f));
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
        UiColor rankedText = IsRanked ? s_black : s_accent;
        string rankedTextValue = IsRanked ? "Ranked" : "Unranked";
        float rankedWidth = BadgeWidth(rankedTextValue);
        float rankedX = scoreX - 10f - rankedWidth;
        elements.Add(Fill("modselect-ranked-badge", new UiRect(rankedX, y, rankedWidth, 44f), rankedFill, IsRanked ? 0.75f : 1f, radius: 12f));
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

    private float AddLabeledBadge(List<UiElementSnapshot> elements, string id, string label, string value, float x, float y)
    {
        float labelWidth = TextWidth(label, 16f) + 24f;
        float valueWidth = TextWidth(value, 16f) + 24f;
        float width = labelWidth + valueWidth;
        elements.Add(Fill($"modselect-stat-{id}", new UiRect(x, y, width, 44f), s_badge, 1f, radius: 12f));
        elements.Add(Fill($"modselect-stat-{id}-label-bg", new UiRect(x, y, labelWidth, 44f), s_black, 0.1f, radius: 12f));
        elements.Add(Text($"modselect-stat-{id}-label", label, new UiRect(x + 12f, y + 9f, labelWidth - 24f, 24f), 16f, s_accent, alignment: UiTextAlignment.Center));
        elements.Add(Text($"modselect-stat-{id}-value", value, new UiRect(x + labelWidth + 12f, y + 9f, valueWidth - 24f, 24f), 16f, s_text, alignment: UiTextAlignment.Center));
        return x + width;
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

        float contentWidth = SidePadding * 2f + PresetSectionWidth + SectionGap + sectionCount * SectionWidth + Math.Max(0, sectionCount - 1) * SectionGap;
        return Math.Max(0f, contentWidth - viewport.VirtualWidth);
    }

    private float MaxSectionScroll(string sectionKey, VirtualViewport viewport)
    {
        int entryCount = string.Equals(sectionKey, "presets", StringComparison.Ordinal)
            ? VisiblePresets().Count() + 1
            : VisibleEntries().Count(entry => string.Equals(entry.SectionKey, sectionKey, StringComparison.Ordinal));
        float contentHeight = entryCount * ToggleHeight + Math.Max(0, entryCount - 1) * ToggleGap + 12f;
        float listHeight = viewport.VirtualHeight - TopBarHeight - BottomBarHeight - SectionHeaderHeight - 12f;
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
        if (new UiRect(x, TopBarHeight, PresetSectionWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight).Contains(point))
        {
            return "presets";
        }

        x += PresetSectionWidth + SectionGap;
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

    private void UpdateScrollInertia(float elapsedSeconds, VirtualViewport viewport)
    {
        if (_dragTarget is not null || elapsedSeconds <= 0f)
        {
            return;
        }

        if (MathF.Abs(_railVelocityX) > DroidUiTheme.Scroll.VelocityStop)
        {
            _sectionRailScrollX = Math.Clamp(_sectionRailScrollX + _railVelocityX * elapsedSeconds, 0f, MaxSectionRailScroll(viewport));
            _railVelocityX = DecayVelocity(_railVelocityX, elapsedSeconds);
            ShowRailScrollbar();
        }
        else
        {
            _railVelocityX = 0f;
        }

        if (MathF.Abs(_selectedModsVelocityX) > DroidUiTheme.Scroll.VelocityStop)
        {
            _selectedModsScrollX = Math.Clamp(_selectedModsScrollX + _selectedModsVelocityX * elapsedSeconds, 0f, MaxSelectedModsScroll());
            _selectedModsVelocityX = DecayVelocity(_selectedModsVelocityX, elapsedSeconds);
            ShowSelectedModsScrollbar();
        }
        else
        {
            _selectedModsVelocityX = 0f;
        }

        foreach (KeyValuePair<string, float> sectionVelocity in _sectionVelocities.ToArray())
        {
            string sectionKey = sectionVelocity.Key;
            float velocity = sectionVelocity.Value;
            if (MathF.Abs(velocity) <= DroidUiTheme.Scroll.VelocityStop)
            {
                _sectionVelocities[sectionKey] = 0f;
                continue;
            }

            _sectionScrolls[sectionKey] = Math.Clamp(SectionScroll(sectionKey) + velocity * elapsedSeconds, 0f, MaxSectionScroll(sectionKey, viewport));
            _sectionVelocities[sectionKey] = DecayVelocity(velocity, elapsedSeconds);
            ShowSectionScrollbar(sectionKey);
        }
    }

    private static float DecayVelocity(float velocity, float elapsedSeconds)
    {
        float decay = MathF.Pow(DroidUiTheme.Scroll.DecelerationPerFrame, elapsedSeconds * 60f);
        return velocity * decay;
    }

    private static void AddHorizontalScrollbar(List<UiElementSnapshot> elements, string id, UiRect viewportBounds, float scroll, float maxScroll, bool visible)
    {
        if (!visible)
        {
            return;
        }

        UiElementSnapshot? indicator = DroidScrollIndicator.Horizontal($"{id}-scrollbar", viewportBounds, scroll, maxScroll, s_text);
        if (indicator is not null)
        {
            elements.Add(indicator);
        }
    }

    private void AddVerticalScrollbar(List<UiElementSnapshot> elements, string id, UiRect sectionBounds, string sectionKey, VirtualViewport viewport)
    {
        float maxScroll = MaxSectionScroll(sectionKey, viewport);
        if (!IsSectionScrollbarVisible(sectionKey) || maxScroll <= 0f)
        {
            return;
        }

        UiRect clip = ListClipBounds(sectionBounds);
        UiElementSnapshot? indicator = DroidScrollIndicator.Vertical($"{id}-scrollbar", clip, SectionScroll(sectionKey), maxScroll, s_text);
        if (indicator is not null)
        {
            elements.Add(indicator);
        }
    }

    private bool IsRailScrollbarVisible() => _elapsedSeconds <= _railScrollbarVisibleUntil;

    private bool IsSelectedModsScrollbarVisible() => _elapsedSeconds <= _selectedModsScrollbarVisibleUntil;

    private bool IsSectionScrollbarVisible(string sectionKey) =>
        _sectionScrollbarVisibleUntil.TryGetValue(sectionKey, out double visibleUntil) && _elapsedSeconds <= visibleUntil;

    private void ShowRailScrollbar() => _railScrollbarVisibleUntil = _elapsedSeconds + DroidUiTheme.Scroll.IndicatorVisibleSeconds;

    private void ShowSelectedModsScrollbar() => _selectedModsScrollbarVisibleUntil = _elapsedSeconds + DroidUiTheme.Scroll.IndicatorVisibleSeconds;

    private void ShowSectionScrollbar(string sectionKey) => _sectionScrollbarVisibleUntil[sectionKey] = _elapsedSeconds + DroidUiTheme.Scroll.IndicatorVisibleSeconds;

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

    private static UiElementSnapshot WithoutAction(UiElementSnapshot element) =>
        element with { Action = UiAction.None };

    private static float LabeledBadgeWidth(string label, string value) =>
        TextWidth(label, 16f) + TextWidth(value, 16f) + 48f;

    private static float BadgeWidth(string text) =>
        TextWidth(text, 18f) + 24f;

    private static float TextWidth(string text, float size) =>
        MathF.Ceiling(text.Length * size * 0.55f);

    private static UiColor StarRatingColor(float rating) => OsuDroidColors.StarRatingBucket(rating);

    private static UiColor StarRatingTextColor(float rating) => OsuDroidColors.StarRatingText(rating);

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

    private static bool ModHasCustomization(string acronym) =>
        ModCatalog.Entries.Any(entry => entry.HasCustomization && string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase));

    private static ModCatalogEntry? EntryByAcronym(string acronym) =>
        ModCatalog.Entries.FirstOrDefault(entry => string.Equals(entry.Acronym, acronym, StringComparison.OrdinalIgnoreCase));

    private static string FormatStat(float? value) =>
        value?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) ?? "0.00";

    private static UiRect SearchBounds(VirtualViewport viewport) => new(viewport.VirtualWidth - 460f, 12f, 400f, 58f);

    private static UiRect SelectedModsBounds() => new(506f, 12f, 340f, 58f);

    private static UiRect PresetAddBounds() => new(SidePadding + 12f, TopBarHeight + SectionHeaderHeight, PresetSectionWidth - 24f, ToggleHeight * 0.62f);

    private static UiRect SectionRailBounds(VirtualViewport viewport) => new(0f, TopBarHeight, viewport.VirtualWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight);

    private static UiRect ListClipBounds(UiRect sectionBounds) =>
        new(sectionBounds.X, sectionBounds.Y + SectionHeaderHeight, sectionBounds.Width, Math.Max(0f, sectionBounds.Height - SectionHeaderHeight - 12f));

    private static bool IntersectsVertically(float y, float height, UiRect clipBounds) =>
        y < clipBounds.Bottom && y + height > clipBounds.Y;

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) =>
        UiElementFactory.Fill(id, bounds, color, alpha, action, radius);

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action);

    private static UiElementSnapshot Sprite(string id, string assetName, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        UiElementFactory.Sprite(id, assetName, bounds, color, alpha, action);

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

    private void AddButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, bool isEnabled = true, UiMaterialIcon? leadingIcon = null, string? leadingAsset = null, UiColor? fillOverride = null, UiColor? textOverride = null, UiRect? clipBounds = null)
    {
        UiColor fill = fillOverride ?? (isEnabled ? s_button : DroidUiColors.WithAlpha(s_button, 120));
        UiColor color = textOverride ?? (isEnabled ? s_text : s_dimText);
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            bounds,
            fill,
            1f,
            Action: isEnabled ? action : UiAction.None,
            IsEnabled: isEnabled,
            CornerRadius: 12f,
            ClipBounds: clipBounds));

        float textX = bounds.X;
        float textWidth = bounds.Width;
        if (leadingAsset is not null)
        {
            elements.Add(Sprite($"{id}-icon", leadingAsset, new UiRect(bounds.X + 16f, bounds.Y + 15f, 28f, 28f), color, 1f, isEnabled ? action : UiAction.None) with { ClipBounds = clipBounds });
            textX += 28f;
            textWidth -= 34f;
        }
        else if (leadingIcon is not null)
        {
            elements.Add(MaterialIcon($"{id}-icon", leadingIcon.Value, new UiRect(bounds.X + 16f, bounds.Y + 15f, 28f, 28f), color, 1f, isEnabled ? action : UiAction.None) with { ClipBounds = clipBounds });
            textX += 28f;
            textWidth -= 34f;
        }

        elements.Add(Text($"{id}-text", text, new UiRect(textX, bounds.Y, textWidth, bounds.Height), 22f, color, isEnabled ? action : UiAction.None, true, UiTextAlignment.Center, clipToBounds: true) with { ClipBounds = clipBounds });
    }
}
