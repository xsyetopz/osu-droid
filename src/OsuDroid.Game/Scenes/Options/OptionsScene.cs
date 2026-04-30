using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;
using OsuDroid.Game.UI.Scrolling;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private static readonly UiColor s_rootBackground = DroidUiColors.Surface;
    private static readonly UiColor s_appBarBackground = DroidUiColors.SurfaceAppBar;
    private static readonly UiColor s_selectedSection = DroidUiColors.SurfaceSelected;
    private static readonly UiColor s_rowBackground = DroidUiColors.SurfaceRow;
    private static readonly UiColor s_inputBackground = DroidUiColors.SurfaceInput;
    private static readonly UiColor s_white = DroidUiColors.TextPrimary;
    private static readonly UiColor s_secondaryText = DroidUiColors.TextSecondary;
    private static readonly UiColor s_disabledWhite = DroidUiColors.TextDisabled;
    private static readonly UiColor s_checkboxAccent = DroidUiColors.Accent;
    private static readonly UiColor s_sliderTrack = DroidUiColors.Track;

    private readonly Dictionary<string, bool> _boolValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _intValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _stringValues = new(StringComparer.Ordinal);
    private readonly GameLocalizer _localizer;
    private readonly IGameSettingsStore? _settingsStore;
    private readonly OptionsPathDefaults _pathDefaults;
    private readonly Action<string>? _settingChanged;
    private ITextInputService _textInputService;
    private OptionsSection _activeSection;
    private float _contentScrollOffset;
    private float _sectionScrollOffset;
    private string? _pendingSfxKey;
    private string? _changedSettingKey;
    private string? _statusMessageKey;
    private TimeSpan _statusMessageRemaining;
    private int? _activeSliderRowIndex;
    private double _elapsedSeconds;
    private OptionsScrollTarget? _activeScrollTarget;
    private readonly KineticScrollState _contentScroll = new(KineticScrollAxis.Vertical);
    private readonly KineticScrollState _sectionScroll = new(KineticScrollAxis.Vertical);
    private VirtualViewport _lastViewport = VirtualViewport.AndroidReferenceLandscape;

    public OptionsScene(
        GameLocalizer localizer,
        IGameSettingsStore? settingsStore = null,
        ITextInputService? textInputService = null,
        OptionsPathDefaults? pathDefaults = null,
        Action<string>? settingChanged = null
    )
    {
        _localizer = localizer;
        _settingsStore = settingsStore;
        _pathDefaults = pathDefaults ?? OptionsPathDefaults.Empty;
        _settingChanged = settingChanged;
        _textInputService = textInputService ?? new NoOpTextInputService();
        ReloadValuesFromStore();
    }

    public OptionsSection ActiveSection => _activeSection;

    public float ScrollOffset => _contentScrollOffset;

    public float ContentScrollOffset => _contentScrollOffset;

    public float SectionScrollOffset => _sectionScrollOffset;

    public IReadOnlyList<string> Sections =>
        OptionsCatalog.Sections.Select(section => _localizer.Get(section.Key)).ToArray();

    public static IReadOnlyList<OptionsSection> AllSections =>
        OptionsCatalog.Sections.Select(section => section.Section).ToArray();

    public IReadOnlyList<string> GeneralRows =>
        OptionsCatalog
            .GeneralCategories.SelectMany(category => category.Rows)
            .Select(row => _localizer.Get(row.TitleKey))
            .ToArray();

    public IReadOnlyList<string> GeneralCategories =>
        OptionsCatalog
            .GeneralCategories.Select(category => _localizer.Get(category.TitleKey))
            .ToArray();

    public IReadOnlyList<string> ActiveRows =>
        ActiveSectionData
            .Categories.SelectMany(category => category.Rows)
            .Select(row => _localizer.Get(row.TitleKey))
            .ToArray();

    public IReadOnlyList<string> ActiveCategories =>
        ActiveSectionData
            .Categories.Select(category => _localizer.Get(category.TitleKey))
            .ToArray();

    public void SetTextInputService(ITextInputService service) => _textInputService = service;

    public void ReloadValuesFromStore()
    {
        _boolValues.Clear();
        _intValues.Clear();
        _stringValues.Clear();
        foreach (SettingsRow? row in AllRows())
        {
            if (row.Kind == SettingsRowKind.Checkbox)
            {
                _boolValues[row.Key] =
                    _settingsStore?.GetBool(row.Key, row.DefaultChecked) ?? row.DefaultChecked;
            }
            else if (row.Kind == SettingsRowKind.Slider)
            {
                _intValues[row.Key] = ClampSliderValue(
                    row,
                    _settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue
                );
            }
            else if (row.Kind == SettingsRowKind.Select)
            {
                _intValues[row.Key] = ClampSelectValue(
                    row,
                    _settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue
                );
            }
            else if (row.Kind == SettingsRowKind.Input)
            {
                string storedValue =
                    _settingsStore?.GetString(row.Key, InputDefaultValue(row))
                    ?? InputDefaultValue(row);
                string normalizedValue = NormalizeInputValue(row, storedValue);
                _stringValues[row.Key] = normalizedValue;
                if (
                    _settingsStore is not null
                    && !string.Equals(storedValue, normalizedValue, StringComparison.Ordinal)
                )
                {
                    _settingsStore.SetString(row.Key, normalizedValue);
                }
            }
        }
    }

    public void ShowStatusMessage(string key)
    {
        _statusMessageKey = key;
        _statusMessageRemaining = TimeSpan.FromSeconds(3);
    }

    public void Update(TimeSpan elapsed)
    {
        float elapsedSeconds = (float)elapsed.TotalSeconds;
        _elapsedSeconds += elapsedSeconds;
        _contentScroll.Update(
            elapsedSeconds,
            () => _contentScrollOffset,
            value => _contentScrollOffset = value,
            0f,
            MaxActiveContentScrollOffset(_lastViewport)
        );
        _sectionScroll.Update(
            elapsedSeconds,
            () => _sectionScrollOffset,
            value => _sectionScrollOffset = value,
            0f,
            MaxSectionScrollOffset(_lastViewport)
        );

        if (_statusMessageKey is not null)
        {
            _statusMessageRemaining -= elapsed;
            if (_statusMessageRemaining <= TimeSpan.Zero)
            {
                _statusMessageKey = null;
            }
        }
    }

    private void RememberViewport(VirtualViewport viewport) => _lastViewport = viewport;

    public string? ConsumePendingSfxKey()
    {
        string? key = _pendingSfxKey;
        _pendingSfxKey = null;
        return key;
    }

    public string? ConsumeChangedSettingKey()
    {
        string? key = _changedSettingKey;
        _changedSettingKey = null;
        return key;
    }

#pragma warning disable IDE0072 // UiAction has cross-scene members; Options maps only Options actions.
#pragma warning restore IDE0072
}
