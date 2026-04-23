using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class MainMenuScene
{
    private enum MenuVisibility
    {
        Collapsed,
        Expanding,
        Expanded,
        Collapsing,
        Exiting,
    }

    private static readonly string[] firstMenu = ["Play", "Options", "Exit"];
    private static readonly string[] secondMenu = ["Solo", "Multiplayer", "Back"];
    private static readonly UiColor backgroundColor = UiColor.Opaque(70, 129, 252);
    private static readonly UiColor translucentBlack = new(0, 0, 0, 128);
    private static readonly UiColor modalScrim = new(0, 0, 0, 172);
    private static readonly UiColor modalPanel = new(24, 24, 38, 255);
    private static readonly UiColor modalDivider = new(42, 42, 60, 255);
    private static readonly UiColor modalLink = new(243, 115, 115, 255);
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

    private static readonly UiColor white = UiColor.Opaque(255, 255, 255);

    private readonly string displayVersion;
    private readonly bool isDevelopmentBuild;
    private readonly OnlineProfileSnapshot profile;
    private MenuNowPlayingState nowPlaying;
    private int selectedIndex;
    private MenuVisibility menuVisibility;
    private double transitionMilliseconds;
    private double shownMilliseconds;
    private double beatMilliseconds;
    private double heartbeatMilliseconds = -1d;
    private double currentBeatMilliseconds = LogoBeatMilliseconds;
    private double exitMilliseconds;
    private double returnTransitionMilliseconds;
    private string? returnTransitionBackgroundPath;
    private bool isReturnTransitionActive;
    private bool hasPendingExitRoute;
    private bool exitRoutePublished;
    private bool isAboutDialogOpen;
    private UiAction pressedAction;
    private readonly float[] spectrumPeakLevel = new float[SpectrumBarCount];
    private readonly float[] spectrumPeakDownRate = new float[SpectrumBarCount];
    private readonly float[] spectrumPeakAlpha = new float[SpectrumBarCount];
    private readonly float[] rawSpectrum = new float[512];
    private bool hasRawSpectrum;

    public MainMenuScene(string displayVersion = "1.0", MenuNowPlayingState? nowPlaying = null, OnlineProfileSnapshot? profile = null, bool isDevelopmentBuild = false)
    {
        this.displayVersion = string.IsNullOrWhiteSpace(displayVersion) ? "1.0" : displayVersion;
        this.isDevelopmentBuild = isDevelopmentBuild;
        this.nowPlaying = nowPlaying ?? new MenuNowPlayingState();
        this.profile = profile ?? OnlineProfileSnapshot.Guest;
    }

    public bool IsSecondMenu { get; private set; }

    public bool IsMenuShown => menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded or MenuVisibility.Collapsing;

    public bool IsExitAnimating => menuVisibility == MenuVisibility.Exiting;

    public bool IsReturnTransitionActive => isReturnTransitionActive;

    public bool IsAboutDialogOpen => isAboutDialogOpen;

    public void SetNowPlaying(MenuNowPlayingState state)
    {
        nowPlaying = state;
        if (state.IsPlaying && state.Bpm > 0.01f)
            currentBeatMilliseconds = Math.Clamp(60000d / state.Bpm, 260d, 2000d);
        else
            currentBeatMilliseconds = LogoBeatMilliseconds;
    }

    public void SetSpectrum(float[] spectrum1024, bool available)
    {
        if (!available || spectrum1024.Length < rawSpectrum.Length)
        {
            hasRawSpectrum = false;
            return;
        }

        Array.Copy(spectrum1024, rawSpectrum, rawSpectrum.Length);
        hasRawSpectrum = true;
    }

    public MainMenuRoute Handle(MainMenuAction action)
    {
        if (isAboutDialogOpen || IsExitAnimating)
            return MainMenuRoute.None;

        if (!IsMenuShown && action == MainMenuAction.Activate)
        {
            BeginExpand();
            return MainMenuRoute.None;
        }

        switch (action)
        {
            case MainMenuAction.MoveUp:
                selectedIndex = (selectedIndex + CurrentEntries.Length - 1) % CurrentEntries.Length;
                return MainMenuRoute.None;

            case MainMenuAction.MoveDown:
                selectedIndex = (selectedIndex + 1) % CurrentEntries.Length;
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
        if (!IsMenuShown || isAboutDialogOpen || IsExitAnimating || menuVisibility == MenuVisibility.Collapsing)
            return MainMenuRoute.None;

        shownMilliseconds = 0d;
        selectedIndex = slot switch
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
        if (isAboutDialogOpen || IsExitAnimating)
            return;

        if (menuVisibility is MenuVisibility.Collapsed or MenuVisibility.Collapsing)
            BeginExpand();
        else
            BeginCollapse();
    }

    public void OpenAboutDialog()
    {
        if (!IsExitAnimating)
            isAboutDialogOpen = true;
    }

    public void CloseAboutDialog() => isAboutDialogOpen = false;

    public void StartReturnTransition(string? backgroundPath = null)
    {
        isReturnTransitionActive = true;
        returnTransitionMilliseconds = 0d;
        returnTransitionBackgroundPath = backgroundPath;
    }

    public void Press(UiAction action)
    {
        if (isAboutDialogOpen || IsExitAnimating)
            return;

        pressedAction = action;
    }

    public void ReleasePress() => pressedAction = UiAction.None;

    public MainMenuRoute ConsumePendingRoute()
    {
        if (!hasPendingExitRoute)
            return MainMenuRoute.None;

        hasPendingExitRoute = false;
        return MainMenuRoute.Exit;
    }

    public void Update(TimeSpan elapsed)
    {
        var elapsedMilliseconds = Math.Max(0d, elapsed.TotalMilliseconds);
        if (nowPlaying.IsPlaying)
        {
            beatMilliseconds += elapsedMilliseconds;
            if (beatMilliseconds >= currentBeatMilliseconds)
            {
                beatMilliseconds %= currentBeatMilliseconds;
                heartbeatMilliseconds = 0d;
            }

            if (heartbeatMilliseconds >= 0d)
            {
                heartbeatMilliseconds += elapsedMilliseconds;
                if (heartbeatMilliseconds > currentBeatMilliseconds * 0.97d)
                    heartbeatMilliseconds = -1d;
            }
        }
        else
        {
            beatMilliseconds += elapsedMilliseconds;
            if (beatMilliseconds >= LogoBeatMilliseconds)
            {
                beatMilliseconds %= LogoBeatMilliseconds;
                heartbeatMilliseconds = 0d;
            }

            if (heartbeatMilliseconds >= 0d)
            {
                heartbeatMilliseconds += elapsedMilliseconds;
                if (heartbeatMilliseconds > LogoBeatMilliseconds * 0.97d)
                    heartbeatMilliseconds = -1d;
            }
        }

        UpdateSpectrumState(elapsedMilliseconds);

        if (isReturnTransitionActive)
        {
            returnTransitionMilliseconds += elapsedMilliseconds;
            if (returnTransitionMilliseconds >= ReturnBackgroundFadeDurationMilliseconds)
            {
                returnTransitionMilliseconds = ReturnBackgroundFadeDurationMilliseconds;
                isReturnTransitionActive = false;
                returnTransitionBackgroundPath = null;
            }
        }

        if (isAboutDialogOpen)
            return;

        switch (menuVisibility)
        {
            case MenuVisibility.Expanding:
                transitionMilliseconds += elapsedMilliseconds;
                shownMilliseconds += elapsedMilliseconds;
                if (transitionMilliseconds >= MenuExpandDurationMilliseconds)
                {
                    transitionMilliseconds = MenuExpandDurationMilliseconds;
                    menuVisibility = MenuVisibility.Expanded;
                }

                break;

            case MenuVisibility.Expanded:
                shownMilliseconds += elapsedMilliseconds;
                break;

            case MenuVisibility.Collapsing:
                transitionMilliseconds += elapsedMilliseconds;
                if (transitionMilliseconds >= MenuCollapseDurationMilliseconds)
                {
                    transitionMilliseconds = 0d;
                    shownMilliseconds = 0d;
                    menuVisibility = MenuVisibility.Collapsed;
                }

                break;

            case MenuVisibility.Exiting:
                exitMilliseconds += elapsedMilliseconds;
                if (exitMilliseconds >= ExitAnimationMilliseconds)
                {
                    exitMilliseconds = ExitAnimationMilliseconds;
                    if (!exitRoutePublished)
                    {
                        hasPendingExitRoute = true;
                        exitRoutePublished = true;
                    }
                }

                break;
        }

        if ((menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded) && shownMilliseconds > MenuIdleCollapseMilliseconds)
            BeginCollapse();
    }


}
