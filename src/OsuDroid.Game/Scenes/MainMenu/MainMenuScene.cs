using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene(string displayVersion = "1.0", MenuNowPlayingState? nowPlaying = null, OnlineProfilePanelState? onlinePanelState = null, bool isDevelopmentBuild = false, GameLocalizer? localizer = null)
{
    private enum MenuVisibility
    {
        Collapsed,
        Expanding,
        Expanded,
        Collapsing,
        Exiting,
    }

    private static readonly UiColor s_backgroundColor = UiColor.Opaque(70, 129, 252);
    private static readonly UiColor s_translucentBlack = new(0, 0, 0, 128);
    private static readonly UiColor s_modalScrim = new(0, 0, 0, 172);
    private static readonly UiColor s_modalPanel = new(24, 24, 38, 255);
    private static readonly UiColor s_modalDivider = new(42, 42, 60, 255);
    private static readonly UiColor s_modalLink = new(243, 115, 115, 255);
    public const float OnlinePanelX = 5f;
    public const float OnlinePanelY = 5f;
    public const float OnlinePanelWidth = 410f;
    public const float OnlinePanelHeight = 110f;
    public const float OnlinePanelAvatarFooterSize = 110f;
    public const float MusicControlSize = 40f;
    public const float MusicControlY = 47f;
    public const float MusicControlStep = 50f;
    public const float MusicControlRightOffset = 35f;
    public const float MusicNowPlayingXOffset = 500f;
    public const float MusicNowPlayingHeight = 40f;
    public const float MusicNowPlayingTextRight = VirtualViewport.LegacyWidth - 500f + 470f;
    public const float MusicNowPlayingSpriteLeftPadding = 130f;
    public const int MusicNowPlayingCharactersMaximum = 35;
    public const float MusicProgressX = VirtualViewport.LegacyWidth - 320f;
    public const float MusicProgressY = 100f;
    public const float MusicProgressWidth = 300f;
    public const float MusicProgressHeight = 7f;
    public const float VersionPillMargin = 10f;
    public const float VersionPillTextXInset = 10f;
    public const float VersionPillTextYInset = 2f;
    public const float VersionPillCornerRadius = 12f;
    public const double MenuExpandDurationMilliseconds = 300d;
    public const double MenuCollapseDurationMilliseconds = 1000d;
    public const double MenuIdleCollapseMilliseconds = 10000d;
    public const double LogoBeatMilliseconds = 1000d;
    public const double ExitAnimationMilliseconds = 3000d;
    public const double ReturnBackgroundFadeDurationMilliseconds = DroidUiTimings.MainMenuReturnBackgroundFadeMilliseconds;

    private const float ButtonExpandOffset = 100f;
    private const float ButtonCollapseOffset = 50f;
    private const float VersionPillTextSize = 16f;
    private const float VersionPillTextHeight = 24f;
    private const float AboutPanelWidth = 500f;
    private const float AboutPanelHeight = 526f;
    private const float AboutPanelRadius = 14f;
    private const float AboutTitleBarHeight = 68f;
    private const float AboutButtonRowHeight = 104f;
    private const float AboutContentTop = 110f;
    private const float AboutLinkGap = 58f;
    private const float ExitDialogPanelWidth = 610f;
    private const float ExitDialogPanelHeight = 244f;
    private const float ExitDialogPanelRadius = 10f;
    private const float ExitDialogTitleBarHeight = 62f;
    private const float ExitDialogButtonHeight = 58f;
    private const float ExitDialogButtonGap = 12f;
    private const float ExitDialogContentInset = 28f;
    private const float ExitDialogTextSize = 24f;
    private const float MainMenuReferenceWidth = 2340f;
    private const float MainMenuReferenceToVirtualScale = MainMenuReferenceWidth / VirtualViewport.LegacyWidth;
    private const float PressTint = 0.7f;
    private const float LogoBeatScale = 1.07f;
    private const float ExitLogoScale = 0.8f;
    private const float ExitLogoRotationDegrees = -15f;
    private const float ExitInstructionTextSize = 22f;
    private const float ExpandedLogoReferenceX = 266f;
    private const float ExpandedLogoReferenceY = 52f;
    private const float ExpandedLogoReferenceSize = 1026f;
    private const float ButtonReferenceX = 710f;
    private const float ButtonReferenceWidth = 1339f;
    private const float ButtonReferenceHeight = 210f;
    private const float FirstButtonReferenceY = 203f;
    private const float SecondButtonReferenceY = 462f;
    private const float ThirdButtonReferenceY = 721f;
    private const int SpectrumBarCount = 120;

    private static readonly UiColor s_white = UiColor.Opaque(255, 255, 255);

    private readonly GameLocalizer _localizer = localizer ?? new GameLocalizer();
    private readonly string _displayVersion = string.IsNullOrWhiteSpace(displayVersion) ? "1.0" : displayVersion;
    private readonly bool _isDevelopmentBuild = isDevelopmentBuild;
    private OnlineProfilePanelState? _onlinePanelState = onlinePanelState;
    private MenuNowPlayingState _nowPlaying = nowPlaying ?? new MenuNowPlayingState();
    private int _selectedIndex;
    private MenuVisibility _menuVisibility;
    private double _transitionMilliseconds;
    private double _shownMilliseconds;
    private double _beatMilliseconds;
    private double _heartbeatMilliseconds = -1d;
    private double _currentBeatMilliseconds = LogoBeatMilliseconds;
    private double _exitMilliseconds;
    private double _returnTransitionMilliseconds;
    private string? _returnTransitionBackgroundPath;
    private bool _isReturnTransitionActive;
    private bool _hasPendingExitRoute;
    private bool _exitRoutePublished;
    private bool _isAboutDialogOpen;
    private bool _isExitDialogOpen;
    private UiAction _pressedAction;
    private readonly float[] _spectrumPeakLevel = new float[SpectrumBarCount];
    private readonly float[] _spectrumPeakDownRate = new float[SpectrumBarCount];
    private readonly float[] _spectrumPeakAlpha = new float[SpectrumBarCount];
    private readonly float[] _rawSpectrum = new float[512];
    private bool _hasRawSpectrum;

    public bool IsSecondMenu { get; private set; }

    public bool IsMenuShown => _menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded or MenuVisibility.Collapsing;

    public bool IsExitAnimating => _menuVisibility == MenuVisibility.Exiting;

    public bool IsReturnTransitionActive => _isReturnTransitionActive;

    public bool IsAboutDialogOpen => _isAboutDialogOpen;

    public bool IsExitDialogOpen => _isExitDialogOpen;

    public void SetNowPlaying(MenuNowPlayingState state)
    {
        _nowPlaying = state;
        _currentBeatMilliseconds = state.IsPlaying && state.Bpm > 0.01f ? Math.Clamp(60000d / state.Bpm, 260d, 2000d) : LogoBeatMilliseconds;
    }

    public void SetOnlinePanelState(OnlineProfilePanelState? state) => _onlinePanelState = state;

    public void SetSpectrum(float[] spectrum1024, bool available)
    {
        if (!available || spectrum1024.Length < _rawSpectrum.Length)
        {
            _hasRawSpectrum = false;
            return;
        }

        Array.Copy(spectrum1024, _rawSpectrum, _rawSpectrum.Length);
        _hasRawSpectrum = true;
    }

    public MainMenuRoute Handle(MainMenuAction action)
    {
        if (_isAboutDialogOpen || _isExitDialogOpen || IsExitAnimating)
        {
            return MainMenuRoute.None;
        }

        if (!IsMenuShown && action == MainMenuAction.Activate)
        {
            BeginExpand();
            return MainMenuRoute.None;
        }

        switch (action)
        {
            case MainMenuAction.MoveUp:
                _selectedIndex = (_selectedIndex + CurrentEntries.Length - 1) % CurrentEntries.Length;
                return MainMenuRoute.None;

            case MainMenuAction.MoveDown:
                _selectedIndex = (_selectedIndex + 1) % CurrentEntries.Length;
                return MainMenuRoute.None;

            case MainMenuAction.Back:
                ShowFirstMenu();
                return MainMenuRoute.None;

            case MainMenuAction.Activate:
                return ActivateSelected();

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public MainMenuRoute Tap(MainMenuButtonSlot slot)
    {
        if (!IsMenuShown || _isAboutDialogOpen || _isExitDialogOpen || IsExitAnimating || _menuVisibility == MenuVisibility.Collapsing)
        {
            return MainMenuRoute.None;
        }

        _shownMilliseconds = 0d;
        _selectedIndex = slot switch
        {
            MainMenuButtonSlot.First => 0,
            MainMenuButtonSlot.Second => 1,
            MainMenuButtonSlot.Third => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
        };

        return ActivateSelected();
    }

    public void ToggleCookie()
    {
        if (_isAboutDialogOpen || _isExitDialogOpen || IsExitAnimating)
        {
            return;
        }

        if (_menuVisibility is MenuVisibility.Collapsed or MenuVisibility.Collapsing)
        {
            BeginExpand();
        }
        else
        {
            BeginCollapse();
        }
    }

    public void OpenAboutDialog()
    {
        if (!_isExitDialogOpen && !IsExitAnimating)
        {
            _isAboutDialogOpen = true;
        }
    }

    public void CloseAboutDialog() => _isAboutDialogOpen = false;

    public void ConfirmExitDialog()
    {
        if (!_isExitDialogOpen)
        {
            return;
        }

        _isExitDialogOpen = false;
        _ = BeginExitAnimation();
    }

    public void CancelExitDialog()
    {
        _isExitDialogOpen = false;
        _pressedAction = UiAction.None;
    }

    public void StartReturnTransition(string? backgroundPath = null)
    {
        _isReturnTransitionActive = true;
        _returnTransitionMilliseconds = 0d;
        _returnTransitionBackgroundPath = backgroundPath;
    }

    public void Press(UiAction action)
    {
        if (_isAboutDialogOpen || _isExitDialogOpen || IsExitAnimating)
        {
            return;
        }

        _pressedAction = action;
    }

    public void ReleasePress() => _pressedAction = UiAction.None;

    public MainMenuRoute ConsumePendingRoute()
    {
        if (!_hasPendingExitRoute)
        {
            return MainMenuRoute.None;
        }

        _hasPendingExitRoute = false;
        return MainMenuRoute.Exit;
    }

    public void Update(TimeSpan elapsed)
    {
        double elapsedMilliseconds = Math.Max(0d, elapsed.TotalMilliseconds);
        if (_nowPlaying.IsPlaying)
        {
            _beatMilliseconds += elapsedMilliseconds;
            if (_beatMilliseconds >= _currentBeatMilliseconds)
            {
                _beatMilliseconds %= _currentBeatMilliseconds;
                _heartbeatMilliseconds = 0d;
            }

            if (_heartbeatMilliseconds >= 0d)
            {
                _heartbeatMilliseconds += elapsedMilliseconds;
                if (_heartbeatMilliseconds > _currentBeatMilliseconds * 0.97d)
                {
                    _heartbeatMilliseconds = -1d;
                }
            }
        }
        else
        {
            _beatMilliseconds += elapsedMilliseconds;
            if (_beatMilliseconds >= LogoBeatMilliseconds)
            {
                _beatMilliseconds %= LogoBeatMilliseconds;
                _heartbeatMilliseconds = 0d;
            }

            if (_heartbeatMilliseconds >= 0d)
            {
                _heartbeatMilliseconds += elapsedMilliseconds;
                if (_heartbeatMilliseconds > LogoBeatMilliseconds * 0.97d)
                {
                    _heartbeatMilliseconds = -1d;
                }
            }
        }

        UpdateSpectrumState();

        if (_isReturnTransitionActive)
        {
            _returnTransitionMilliseconds += elapsedMilliseconds;
            if (_returnTransitionMilliseconds >= ReturnBackgroundFadeDurationMilliseconds)
            {
                _returnTransitionMilliseconds = ReturnBackgroundFadeDurationMilliseconds;
                _isReturnTransitionActive = false;
                _returnTransitionBackgroundPath = null;
            }
        }

        if (_isAboutDialogOpen || _isExitDialogOpen)
        {
            return;
        }

        switch (_menuVisibility)
        {
            case MenuVisibility.Expanding:
                _transitionMilliseconds += elapsedMilliseconds;
                _shownMilliseconds += elapsedMilliseconds;
                if (_transitionMilliseconds >= MenuExpandDurationMilliseconds)
                {
                    _transitionMilliseconds = MenuExpandDurationMilliseconds;
                    _menuVisibility = MenuVisibility.Expanded;
                }

                break;

            case MenuVisibility.Expanded:
                _shownMilliseconds += elapsedMilliseconds;
                break;

            case MenuVisibility.Collapsing:
                _transitionMilliseconds += elapsedMilliseconds;
                if (_transitionMilliseconds >= MenuCollapseDurationMilliseconds)
                {
                    _transitionMilliseconds = 0d;
                    _shownMilliseconds = 0d;
                    _menuVisibility = MenuVisibility.Collapsed;
                }

                break;

            case MenuVisibility.Exiting:
                _exitMilliseconds += elapsedMilliseconds;
                if (_exitMilliseconds >= ExitAnimationMilliseconds)
                {
                    _exitMilliseconds = ExitAnimationMilliseconds;
                    if (!_exitRoutePublished)
                    {
                        _hasPendingExitRoute = true;
                        _exitRoutePublished = true;
                    }
                }

                break;
            case MenuVisibility.Collapsed:
                break;
            default:
                break;
        }

        if ((_menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded) && _shownMilliseconds > MenuIdleCollapseMilliseconds)
        {
            BeginCollapse();
        }
    }


}
