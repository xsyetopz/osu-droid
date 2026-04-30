using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
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
    private const int VisiblePresetActionLimit = 16;
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
    private static readonly UiColor s_rankedText = DroidUiTheme.ModMenu.RankedText;
    private static readonly UiColor s_text = DroidUiTheme.TextPrimary;
    private static readonly UiColor s_dimText = DroidUiTheme.TextSecondary;
    private static readonly UiColor s_black = DroidUiTheme.Black;

    private readonly IGameSettingsStore _settingsStore;
    private readonly GameLocalizer _localizer;
    private readonly HashSet<string> _selectedAcronyms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string>> _modSettings = new(
        StringComparer.OrdinalIgnoreCase
    );
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
    private readonly Dictionary<string, double> _sectionScrollbarVisibleUntil = new(
        StringComparer.Ordinal
    );
    private ScrollDragTarget? _dragTarget;
    private BeatmapInfo? _selectedBeatmap;
    private VirtualViewport _lastViewport = VirtualViewport.AndroidReferenceLandscape;
    private bool _isPresetFormOpen;
    private bool _isPresetDeleteDialogOpen;
    private bool _isCustomizeOpen;
    private int? _activeCustomizeSliderIndex;
    private string _presetNameInput = string.Empty;
    private int _pendingPresetDeleteIndex = -1;

    public ModSelectScene(
        IGameSettingsStore settingsStore,
        ITextInputService textInputService,
        GameLocalizer? localizer = null
    )
    {
        _settingsStore = settingsStore;
        _textInputService = textInputService;
        _localizer = localizer ?? new GameLocalizer();
        LoadSelectedMods();
        LoadModSettings();
        LoadPresets();
    }

    public IReadOnlyCollection<string> SelectedAcronyms => _selectedAcronyms;

    public float ScoreMultiplier => CurrentStats().ScoreMultiplier;

    public bool IsRanked => CurrentStats().IsRanked;

    public bool IsPresetDialogOpen => _isPresetFormOpen || _isPresetDeleteDialogOpen;

    public void SetTextInputService(ITextInputService service) => _textInputService = service;

    public void SetSelectedBeatmap(BeatmapInfo? beatmap) => _selectedBeatmap = beatmap;

    public ModSelectionState CreateSelectionState() =>
        new(
            ModCatalog
                .Entries.Select(entry => entry.Acronym)
                .Where(_selectedAcronyms.Contains)
                .ToArray(),
            _modSettings.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyDictionary<string, string>)pair.Value,
                StringComparer.OrdinalIgnoreCase
            )
        );

    private ModStatSnapshot CurrentStats() =>
        ModStatCalculator.FromBeatmap(_selectedBeatmap, CreateSelectionState());

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) =>
        new("ModSelect", "Mods", "osu!droid mod menu", [], 0, false, CreateUiFrame(viewport));

    public GameFrameSnapshot CreateSnapshot(
        VirtualViewport viewport,
        UiFrameSnapshot parentFrame
    ) =>
        new(
            "ModSelect",
            "Mods",
            "osu!droid mod menu",
            [],
            0,
            false,
            CreateUiFrame(viewport, parentFrame)
        );

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
}
