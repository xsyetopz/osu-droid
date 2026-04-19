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
    private static readonly UiColor onlinePanelBackground = new(51, 51, 51, 128);
    private static readonly UiColor onlinePanelAvatarFooter = new(51, 51, 51, 204);

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
    public const float VersionPillMargin = 10f;
    public const float VersionPillTextXInset = 10f;
    public const float VersionPillTextYInset = 2f;
    public const float VersionPillCornerRadius = 12f;
    public const double MenuExpandDurationMilliseconds = 300d;
    public const double MenuCollapseDurationMilliseconds = 1000d;
    public const double MenuIdleCollapseMilliseconds = 10000d;
    public const double LogoBeatMilliseconds = 1000d;
    public const double ExitAnimationMilliseconds = 3000d;
    public const double ReturnBackgroundFadeDurationMilliseconds = 1500d;

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
    private const float ExpandedLogoReferenceX = 266f;
    private const float ExpandedLogoReferenceY = 52f;
    private const float ExpandedLogoReferenceSize = 1026f;
    private const float ButtonReferenceX = 710f;
    private const float ButtonReferenceWidth = 1339f;
    private const float ButtonReferenceHeight = 210f;
    private const float FirstButtonReferenceY = 203f;
    private const float SecondButtonReferenceY = 462f;
    private const float ThirdButtonReferenceY = 721f;

    private static readonly UiColor white = UiColor.Opaque(255, 255, 255);

    private readonly string displayVersion;
    private readonly MenuNowPlayingState nowPlaying;
    private int selectedIndex;
    private MenuVisibility menuVisibility;
    private double transitionMilliseconds;
    private double shownMilliseconds;
    private double beatMilliseconds;
    private double heartbeatMilliseconds = -1d;
    private double exitMilliseconds;
    private double returnTransitionMilliseconds;
    private bool isReturnTransitionActive;
    private bool hasPendingExitRoute;
    private bool exitRoutePublished;
    private bool isAboutDialogOpen;
    private UiAction pressedAction;

    public MainMenuScene(string displayVersion = "1.0", MenuNowPlayingState? nowPlaying = null)
    {
        this.displayVersion = string.IsNullOrWhiteSpace(displayVersion) ? "1.0" : displayVersion;
        this.nowPlaying = nowPlaying ?? new MenuNowPlayingState();
    }

    public bool IsSecondMenu { get; private set; }

    public bool IsMenuShown => menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded or MenuVisibility.Collapsing;

    public bool IsExitAnimating => menuVisibility == MenuVisibility.Exiting;

    public bool IsReturnTransitionActive => isReturnTransitionActive;

    public bool IsAboutDialogOpen => isAboutDialogOpen;

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

    public void StartReturnTransition()
    {
        isReturnTransitionActive = true;
        returnTransitionMilliseconds = 0d;
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

        if (isReturnTransitionActive)
        {
            returnTransitionMilliseconds += elapsedMilliseconds;
            if (returnTransitionMilliseconds >= ReturnBackgroundFadeDurationMilliseconds)
            {
                returnTransitionMilliseconds = ReturnBackgroundFadeDurationMilliseconds;
                isReturnTransitionActive = false;
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
