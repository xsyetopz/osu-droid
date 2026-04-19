using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public enum MainMenuAction
{
    Activate,
    Back,
    MoveUp,
    MoveDown,
}

public enum MainMenuButtonSlot
{
    First,
    Second,
    Third,
}

public enum MainMenuRoute
{
    None,
    Solo,
    Multiplayer,
    Settings,
    Exit,
}

public sealed class MainMenuScene
{
    private enum MenuVisibility
    {
        Collapsed,
        Expanding,
        Expanded,
        Collapsing,
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
    private bool isAboutDialogOpen;

    public MainMenuScene(string displayVersion = "1.0", MenuNowPlayingState? nowPlaying = null)
    {
        this.displayVersion = string.IsNullOrWhiteSpace(displayVersion) ? "1.0" : displayVersion;
        this.nowPlaying = nowPlaying ?? new MenuNowPlayingState();
    }

    public bool IsSecondMenu { get; private set; }

    public bool IsMenuShown => menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded or MenuVisibility.Collapsing;

    public bool IsAboutDialogOpen => isAboutDialogOpen;

    public MainMenuRoute Handle(MainMenuAction action)
    {
        if (isAboutDialogOpen)
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
        if (!IsMenuShown || isAboutDialogOpen)
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
        if (isAboutDialogOpen)
            return;

        if (menuVisibility is MenuVisibility.Collapsed or MenuVisibility.Collapsing)
            BeginExpand();
        else
            BeginCollapse();
    }

    public void OpenAboutDialog() => isAboutDialogOpen = true;

    public void CloseAboutDialog() => isAboutDialogOpen = false;

    public void Update(TimeSpan elapsed)
    {
        if (isAboutDialogOpen)
            return;

        var elapsedMilliseconds = Math.Max(0d, elapsed.TotalMilliseconds);

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
        }

        if ((menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded) && shownMilliseconds > MenuIdleCollapseMilliseconds)
            BeginCollapse();
    }

    public GameFrameSnapshot Snapshot => CreateSnapshot(VirtualViewport.LegacyLandscape);

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new(
        "MainMenu",
        "osu!droid",
        IsSecondMenu ? "legacy second menu" : "legacy first menu",
        CurrentEntries,
        selectedIndex,
        IsSecondMenu,
        CreateUiFrame(viewport));

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
                2 => MainMenuRoute.Exit,
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
        transitionMilliseconds = 0d;
        shownMilliseconds = 0d;
    }

    private void BeginCollapse()
    {
        ShowFirstMenu();
        menuVisibility = MenuVisibility.Collapsing;
        transitionMilliseconds = 0d;
    }

    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport)
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
        AddMenuButtons(elements);
        AddLogo(elements, viewport);
        AddProfileShell(elements);
        AddVersionPill(elements, viewport);
        AddDownloaderTab(elements, viewport);
        AddMusicControls(elements, viewport);

        if (isAboutDialogOpen)
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
        var logoBounds = GetLogoBounds(viewport);

        elements.Add(new UiElementSnapshot(
            "logo-glow",
            UiElementKind.Sprite,
            ScaleFromCenter(logoBounds, 1.07f),
            white,
            0.2f,
            DroidAssets.Logo));

        elements.Add(new UiElementSnapshot(
            "logo",
            UiElementKind.Sprite,
            logoBounds,
            white,
            1f,
            DroidAssets.Logo,
            UiAction.MainMenuCookie));
    }

    private void AddMenuButtons(List<UiElementSnapshot> elements)
    {
        if (!IsMenuShown)
            return;

        AddMenuButton(elements, 0, GetAndroidMainMenuButtonBounds(0), UiAction.MainMenuFirst);
        AddMenuButton(elements, 1, GetAndroidMainMenuButtonBounds(1), UiAction.MainMenuSecond);
        AddMenuButton(elements, 2, GetAndroidMainMenuButtonBounds(2), UiAction.MainMenuThird);
    }

    private void AddMenuButton(List<UiElementSnapshot> elements, int index, UiRect finalBounds, UiAction action)
    {
        var assetName = CurrentButtonAsset(index);
        var animatedBounds = finalBounds with { X = GetAnimatedMenuButtonX(finalBounds.X) };
        elements.Add(new UiElementSnapshot(
            $"menu-{index}",
            UiElementKind.Sprite,
            animatedBounds,
            white,
            GetMenuButtonAlpha(),
            assetName,
            action));
    }

    private string CurrentButtonAsset(int index)
    {
        if (!IsSecondMenu)
        {
            return index switch
            {
                0 => DroidAssets.Play,
                1 => DroidAssets.Options,
                2 => DroidAssets.Exit,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
        }

        return index switch
        {
            0 => DroidAssets.Solo,
            1 => DroidAssets.Multi,
            2 => DroidAssets.Back,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
        };
    }

    private static void AddDownloaderTab(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var tab = DroidAssets.MainMenuManifest.Get(DroidAssets.BeatmapDownloader);
        elements.Add(new UiElementSnapshot(
            "beatmap-downloader",
            UiElementKind.Sprite,
            new UiRect(viewport.VirtualWidth - tab.NativeSize.Width, (viewport.VirtualHeight - tab.NativeSize.Height) / 2f, tab.NativeSize.Width, tab.NativeSize.Height),
            white,
            0.92f,
            DroidAssets.BeatmapDownloader));
    }

    private void AddMusicControls(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(new UiElementSnapshot(
            "music-now-playing",
            UiElementKind.Sprite,
            GetAndroidMusicNowPlayingBounds(),
            white,
            1f,
            DroidAssets.MusicNowPlaying));

        if (!string.IsNullOrWhiteSpace(nowPlaying.ArtistTitle))
        {
            elements.Add(new UiElementSnapshot(
                "music-title",
                UiElementKind.Text,
                new UiRect(GetAndroidMusicNowPlayingBounds().X + 80f, GetAndroidMusicNowPlayingBounds().Y + 3f, 270f, 35f),
                white,
                1f,
                Text: nowPlaying.ArtistTitle,
                TextStyle: new UiTextStyle(22f, false, UiTextAlignment.Right)));
        }

        AddMusicControl(elements, DroidAssets.MusicPrevious, UiAction.MainMenuMusicPrevious, 6f);
        AddMusicControl(elements, DroidAssets.MusicPlay, UiAction.MainMenuMusicPlay, 5f);
        AddMusicControl(elements, DroidAssets.MusicPause, UiAction.MainMenuMusicPause, 4f);
        AddMusicControl(elements, DroidAssets.MusicStop, UiAction.MainMenuMusicStop, 3f);
        AddMusicControl(elements, DroidAssets.MusicNext, UiAction.MainMenuMusicNext, 2f);
    }

    private static void AddMusicControl(List<UiElementSnapshot> elements, string assetName, UiAction action, float legacyIndex)
    {
        elements.Add(new UiElementSnapshot(
            assetName,
            UiElementKind.Sprite,
            GetAndroidMusicControlBounds(legacyIndex),
            white,
            1f,
            assetName,
            action));
    }

    private static void AddAboutDialog(List<UiElementSnapshot> elements, VirtualViewport viewport, string displayVersion)
    {
        var panel = CreateAboutPanelBounds(viewport);
        elements.Add(new UiElementSnapshot(
            "about-scrim",
            UiElementKind.Fill,
            new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
            modalScrim,
            1f,
            Action: UiAction.MainMenuAboutClose));

        elements.Add(new UiElementSnapshot(
            "about-panel",
            UiElementKind.Fill,
            panel,
            modalPanel,
            1f,
            CornerRadius: AboutPanelRadius));

        var contentWidth = panel.Width;
        AddAboutText(elements, panel.X, panel.Y + 19f, "about-dialog-title", "About", contentWidth, 25f, false, white, UiTextAlignment.Center);
        AddAboutDivider(elements, panel.X, panel.Y + AboutTitleBarHeight, panel.Width, 1f, "about-title-divider");

        AddAboutText(elements, panel.X, panel.Y + AboutContentTop, "about-title", "osu!droid", contentWidth, 36f, true, white, UiTextAlignment.Center);
        AddAboutText(elements, panel.X, panel.Y + AboutContentTop + 68f, "about-version", $"Version {displayVersion}", contentWidth, 30f, true, white, UiTextAlignment.Center);
        AddAboutText(elements, panel.X, panel.Y + AboutContentTop + 138f, "about-made-by", "Made by osu!droid team", contentWidth, 26f, false, white, UiTextAlignment.Center);
        AddAboutText(elements, panel.X, panel.Y + AboutContentTop + 174f, "about-copyright", "osu! is © peppy 2007-2026", contentWidth, 26f, false, white, UiTextAlignment.Center);

        var firstLinkY = panel.Y + 335f;
        AddAboutText(elements, panel.X, firstLinkY, "about-osu-link", "Visit official osu! website ↗", contentWidth, 26f, false, modalLink, UiTextAlignment.Center, true, UiAction.MainMenuAboutOsuWebsite);
        AddAboutText(elements, panel.X, firstLinkY + AboutLinkGap, "about-droid-link", "Visit official osu!droid website ↗", contentWidth, 26f, false, modalLink, UiTextAlignment.Center, true, UiAction.MainMenuAboutOsuDroidWebsite);
        AddAboutText(elements, panel.X, firstLinkY + AboutLinkGap * 2f, "about-discord-link", "Join the official Discord server ↗", contentWidth, 26f, false, modalLink, UiTextAlignment.Center, true, UiAction.MainMenuAboutDiscord);

        var buttonY = panel.Bottom - AboutButtonRowHeight;
        AddAboutDivider(elements, panel.X, buttonY, panel.Width, 1f, "about-button-row-divider");
        AddAboutDivider(elements, panel.X + panel.Width / 2f, buttonY, 1f, AboutButtonRowHeight, "about-button-divider");
        AddAboutButton(elements, "about-changelog", new UiRect(panel.X, buttonY, panel.Width / 2f, AboutButtonRowHeight), "Changelog", UiAction.MainMenuAboutChangelog);
        AddAboutButton(elements, "about-close", new UiRect(panel.X + panel.Width / 2f, buttonY, panel.Width / 2f, AboutButtonRowHeight), "Close", UiAction.MainMenuAboutClose);
    }

    private static UiRect CreateAboutPanelBounds(VirtualViewport viewport)
    {
        var height = Math.Min(AboutPanelHeight, viewport.VirtualHeight - 64f);
        return new UiRect(
            (viewport.VirtualWidth - AboutPanelWidth) / 2f,
            (viewport.VirtualHeight - height) / 2f,
            AboutPanelWidth,
            height);
    }

    private static void AddAboutDivider(List<UiElementSnapshot> elements, float x, float y, float width, float height, string id)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            new UiRect(x, y, width, height),
            modalDivider,
            1f));
    }

    private static void AddAboutText(
        List<UiElementSnapshot> elements,
        float x,
        float y,
        string id,
        string text,
        float width,
        float size,
        bool isBold,
        UiColor color,
        UiTextAlignment alignment,
        bool isUnderlined = false,
        UiAction action = UiAction.None)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Text,
            new UiRect(x, y, width, size + 12f),
            color,
            1f,
            Action: action,
            Text: text,
            TextStyle: new UiTextStyle(size, isBold, alignment, isUnderlined)));
    }

    private static void AddAboutButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            bounds,
            modalPanel,
            1f,
            Action: action,
            CornerMode: UiCornerMode.None));
        elements.Add(new UiElementSnapshot(
            $"{id}-text",
            UiElementKind.Text,
            new UiRect(bounds.X, bounds.Y + 34f, bounds.Width, bounds.Height - 34f),
            white,
            1f,
            Text: text,
            TextStyle: new UiTextStyle(28f, true, UiTextAlignment.Center)));
    }

    private UiRect GetLogoBounds(VirtualViewport viewport)
    {
        var collapsed = GetCenteredLogoBounds(viewport);
        var expanded = GetExpandedLogoBounds(viewport);
        var progress = GetMenuTransitionProgress();
        return new UiRect(
            Lerp(collapsed.X, expanded.X, progress),
            Lerp(collapsed.Y, expanded.Y, progress),
            Lerp(collapsed.Width, expanded.Width, progress),
            Lerp(collapsed.Height, expanded.Height, progress));
    }

    private static UiRect GetCenteredLogoBounds(VirtualViewport viewport)
    {
        var logoSize = ExpandedLogoReferenceSize / MainMenuReferenceToVirtualScale;
        return new UiRect((viewport.VirtualWidth - logoSize) / 2f, (viewport.VirtualHeight - logoSize) / 2f, logoSize, logoSize);
    }

    private static UiRect GetExpandedLogoBounds(VirtualViewport viewport) => ReferenceRect(ExpandedLogoReferenceX, ExpandedLogoReferenceY, ExpandedLogoReferenceSize, ExpandedLogoReferenceSize);

    private float GetMenuTransitionProgress() => menuVisibility switch
    {
        MenuVisibility.Collapsed => 0f,
        MenuVisibility.Expanding => (float)Math.Clamp(transitionMilliseconds / MenuExpandDurationMilliseconds, 0d, 1d),
        MenuVisibility.Expanded => 1f,
        MenuVisibility.Collapsing => 1f - (float)Math.Clamp(transitionMilliseconds / MenuCollapseDurationMilliseconds, 0d, 1d),
        _ => 0f,
    };

    private float GetMenuButtonAlpha() => menuVisibility switch
    {
        MenuVisibility.Expanding => 0.9f * (float)Math.Clamp(transitionMilliseconds / MenuExpandDurationMilliseconds, 0d, 1d),
        MenuVisibility.Expanded => 0.9f,
        MenuVisibility.Collapsing => 0.9f * (1f - (float)Math.Clamp(transitionMilliseconds / MenuCollapseDurationMilliseconds, 0d, 1d)),
        _ => 0f,
    };

    private float GetAnimatedMenuButtonX(float finalX)
    {
        return menuVisibility switch
        {
            MenuVisibility.Expanding => Lerp(finalX - ButtonExpandOffset / MainMenuReferenceToVirtualScale, finalX, (float)Math.Clamp(transitionMilliseconds / MenuExpandDurationMilliseconds, 0d, 1d)),
            MenuVisibility.Collapsing => Lerp(finalX, finalX - ButtonCollapseOffset / MainMenuReferenceToVirtualScale, (float)Math.Clamp(transitionMilliseconds / MenuCollapseDurationMilliseconds, 0d, 1d)),
            _ => finalX,
        };
    }


    private string GetVersionText() => $"osu!droid {displayVersion}";

    private static UiRect CreateVersionPillBounds(VirtualViewport viewport, string text)
    {
        var width = EstimateVersionTextWidth(text) + VersionPillTextXInset * 2f;
        var height = VersionPillTextHeight + VersionPillTextYInset * 2f;
        return new UiRect(VersionPillMargin, viewport.VirtualHeight - height - VersionPillMargin, width, height);
    }

    private static float EstimateVersionTextWidth(string text) => Math.Max(120f, text.Length * VersionPillTextSize * 0.58f + 4f);

    private static UiRect ReferenceRect(float x, float y, float width, float height) => new(
        x / MainMenuReferenceToVirtualScale,
        y / MainMenuReferenceToVirtualScale,
        width / MainMenuReferenceToVirtualScale,
        height / MainMenuReferenceToVirtualScale);

    private static float Lerp(float start, float end, float progress) => start + (end - start) * progress;

    private static UiRect ScaleFromCenter(UiRect bounds, float scale)
    {
        var width = bounds.Width * scale;
        var height = bounds.Height * scale;
        return new UiRect(
            bounds.X - (width - bounds.Width) / 2f,
            bounds.Y - (height - bounds.Height) / 2f,
            width,
            height);
    }
}
