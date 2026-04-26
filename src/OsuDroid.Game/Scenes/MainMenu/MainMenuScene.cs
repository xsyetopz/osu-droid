using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene(
    string displayVersion = "1.0",
    MenuNowPlayingState? nowPlaying = null,
    OnlineProfilePanelState? onlinePanelState = null,
    bool isDevelopmentBuild = false,
    GameLocalizer? localizer = null
)
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
    public const float MusicNowPlayingTextRight =
        VirtualViewport.AndroidReferenceWidth - 500f + 470f;
    public const float MusicNowPlayingSpriteLeftPadding = 130f;
    public const int MusicNowPlayingCharactersMaximum = 35;
    public const float MusicProgressX = VirtualViewport.AndroidReferenceWidth - 320f;
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
    public const double ReturnBackgroundFadeDurationMilliseconds =
        DroidUiTimings.MainMenuReturnBackgroundFadeMilliseconds;

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
    private const float MainMenuReferenceToVirtualScale =
        MainMenuReferenceWidth / VirtualViewport.AndroidReferenceWidth;
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
    private readonly string _displayVersion = string.IsNullOrWhiteSpace(displayVersion)
        ? "1.0"
        : displayVersion;
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

    public bool IsMenuShown =>
        _menuVisibility
            is MenuVisibility.Expanding
                or MenuVisibility.Expanded
                or MenuVisibility.Collapsing;

    public bool IsExitAnimating => _menuVisibility == MenuVisibility.Exiting;

    public bool IsReturnTransitionActive => _isReturnTransitionActive;

    public bool IsAboutDialogOpen => _isAboutDialogOpen;

    public bool IsExitDialogOpen => _isExitDialogOpen;
}
