using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class MainMenuScene
{
    public GameFrameSnapshot Snapshot => CreateSnapshot(VirtualViewport.LegacyLandscape);

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new(
        "MainMenu",
        "osu!droid",
        IsSecondMenu ? "legacy second menu" : "legacy first menu",
        CurrentEntries,
        selectedIndex,
        IsSecondMenu,
        CreateUiFrame(viewport));

    public GameFrameSnapshot CreateAboutDialogSnapshot(VirtualViewport viewport) => new(
        "MainMenu",
        "osu!droid",
        IsSecondMenu ? "legacy second menu" : "legacy first menu",
        CurrentEntries,
        selectedIndex,
        IsSecondMenu,
        CreateUiFrame(viewport, true));

    private string[] CurrentEntries => IsSecondMenu ? secondMenu : firstMenu;

    public static UiRect GetAndroidCollapsedLogoBounds(VirtualViewport viewport) => GetCenteredLogoBounds(viewport);

    public static UiRect GetAndroidExpandedLogoBounds(VirtualViewport viewport) => GetExpandedLogoBounds(viewport);

    public static UiRect GetAndroidMainMenuButtonBounds(int index) => index switch
    {
        0 => ReferenceRect(ButtonReferenceX, FirstButtonReferenceY, ButtonReferenceWidth, ButtonReferenceHeight),
        1 => ReferenceRect(ButtonReferenceX, SecondButtonReferenceY, ButtonReferenceWidth, ButtonReferenceHeight),
        2 => ReferenceRect(ButtonReferenceX, ThirdButtonReferenceY, ButtonReferenceWidth, ButtonReferenceHeight),
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
    };

    public static UiRect GetAndroidMusicControlBounds(float legacyIndex) => new(
        VirtualViewport.LegacyWidth - MusicControlStep * legacyIndex + MusicControlRightOffset,
        MusicControlY,
        MusicControlSize,
        MusicControlSize);

    public static UiRect GetAndroidMusicNowPlayingBounds()
    {
        var nowPlayingAsset = DroidAssets.MainMenuManifest.Get(DroidAssets.MusicNowPlaying);
        var width = MusicNowPlayingHeight * nowPlayingAsset.NativeSize.Width / nowPlayingAsset.NativeSize.Height;
        return new UiRect(VirtualViewport.LegacyWidth - MusicNowPlayingXOffset, 0f, width, MusicNowPlayingHeight);
    }

    public UiRect GetVersionPillBounds(VirtualViewport viewport) => CreateVersionPillBounds(viewport, GetVersionText());

    private MainMenuRoute ActivateSelected()
    {
        if (!IsSecondMenu)
        {
            return selectedIndex switch
            {
                0 => ShowSecondMenu(),
                1 => MainMenuRoute.Settings,
                2 => BeginExitAnimation(),
                _ => MainMenuRoute.None,
            };
        }

        return selectedIndex switch
        {
            0 => MainMenuRoute.Solo,
            1 => MainMenuRoute.Multiplayer,
            2 => BackToFirstMenu(),
            _ => MainMenuRoute.None,
        };
    }

    private MainMenuRoute BeginExitAnimation()
    {
        ShowFirstMenu();
        menuVisibility = MenuVisibility.Exiting;
        transitionMilliseconds = 0d;
        shownMilliseconds = 0d;
        exitMilliseconds = 0d;
        hasPendingExitRoute = false;
        exitRoutePublished = false;
        pressedAction = UiAction.None;
        return MainMenuRoute.None;
    }

    private MainMenuRoute ShowSecondMenu()
    {
        IsSecondMenu = true;
        selectedIndex = 0;
        return MainMenuRoute.None;
    }

    private MainMenuRoute BackToFirstMenu()
    {
        ShowFirstMenu();
        return MainMenuRoute.None;
    }

    private void ShowFirstMenu()
    {
        IsSecondMenu = false;
        selectedIndex = 0;
    }

    private void BeginExpand()
    {
        ShowFirstMenu();
        menuVisibility = MenuVisibility.Expanding;
        pressedAction = UiAction.None;
        transitionMilliseconds = 0d;
        shownMilliseconds = 0d;
    }

    private void BeginCollapse()
    {
        ShowFirstMenu();
        menuVisibility = MenuVisibility.Collapsing;
        pressedAction = UiAction.None;
        transitionMilliseconds = 0d;
    }

    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport, bool forceAboutDialog = false)
    {
        var elements = new List<UiElementSnapshot>
        {
            new(
                "background-color",
                UiElementKind.Fill,
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                backgroundColor,
                1f),
        };

        AddBackground(elements, viewport);
        AddReturnTransitionBackground(elements, viewport);
        AddMenuButtons(elements);
        AddLogo(elements, viewport);
        AddProfileShell(elements);
        AddVersionPill(elements, viewport);
        AddDownloaderTab(elements, viewport);
        AddMusicControls(elements, viewport);
        AddExitOverlay(elements, viewport);

        if (isAboutDialogOpen || forceAboutDialog)
            AddAboutDialog(elements, viewport, displayVersion);

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private static void AddBackground(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var background = DroidAssets.MainMenuManifest.Get(DroidAssets.MenuBackground);
        var scale = viewport.VirtualWidth / background.NativeSize.Width;
        var width = background.NativeSize.Width * scale;
        var height = background.NativeSize.Height * scale;
        elements.Add(new UiElementSnapshot(
            "menu-background",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height),
            white,
            1f,
            DroidAssets.MenuBackground));
    }

    private static void AddProfileShell(List<UiElementSnapshot> elements)
    {
        elements.Add(new UiElementSnapshot(
            "profile-panel",
            UiElementKind.Fill,
            new UiRect(OnlinePanelX, OnlinePanelY, OnlinePanelWidth, OnlinePanelHeight),
            onlinePanelBackground,
            1f));

        elements.Add(new UiElementSnapshot(
            "profile-avatar-footer",
            UiElementKind.Fill,
            new UiRect(OnlinePanelX, OnlinePanelY, OnlinePanelAvatarFooterSize, OnlinePanelAvatarFooterSize),
            onlinePanelAvatarFooter,
            1f));

        elements.Add(new UiElementSnapshot(
            "profile-avatar",
            UiElementKind.Sprite,
            new UiRect(OnlinePanelX, OnlinePanelY, OnlinePanelAvatarFooterSize, OnlinePanelAvatarFooterSize),
            white,
            1f,
            DroidAssets.EmptyAvatar));
    }

    private void AddVersionPill(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var text = GetVersionText();
        var bounds = CreateVersionPillBounds(viewport, text);
        elements.Add(new UiElementSnapshot(
            "version-pill",
            UiElementKind.Fill,
            bounds,
            translucentBlack,
            1f,
            Action: UiAction.MainMenuVersionPill,
            CornerRadius: VersionPillCornerRadius));

        elements.Add(new UiElementSnapshot(
            "version-pill-text",
            UiElementKind.Text,
            new UiRect(bounds.X + VersionPillTextXInset, bounds.Y + VersionPillTextYInset, bounds.Width - VersionPillTextXInset * 2f, bounds.Height - VersionPillTextYInset * 2f),
            white,
            1f,
            Text: text,
            TextStyle: new UiTextStyle(VersionPillTextSize)));
    }

    private void AddLogo(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var baseLogoBounds = GetLogoBounds(viewport);
        var logoBounds = ScaleFromCenter(baseLogoBounds, GetLogoHeartbeatScale());
        var overlayBounds = ScaleFromCenter(baseLogoBounds, LogoBeatScale);
        var exitProgress = GetExitProgress();
        if (exitProgress > 0f)
        {
            logoBounds = ScaleFromCenter(logoBounds, Lerp(1f, ExitLogoScale, exitProgress));
            overlayBounds = ScaleFromCenter(overlayBounds, Lerp(1f, ExitLogoScale, exitProgress));
        }

        var rotation = Lerp(0f, ExitLogoRotationDegrees, exitProgress);

        elements.Add(new UiElementSnapshot(
            "logo",
            UiElementKind.Sprite,
            logoBounds,
            white,
            1f,
            DroidAssets.Logo,
            UiAction.MainMenuCookie,
            RotationDegrees: rotation));

        elements.Add(new UiElementSnapshot(
            "logo-glow",
            UiElementKind.Sprite,
            overlayBounds,
            white,
            0.2f,
            DroidAssets.Logo,
            RotationDegrees: rotation));
    }

}
