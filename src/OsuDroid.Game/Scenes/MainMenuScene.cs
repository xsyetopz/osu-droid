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
    private static readonly UiColor translucentBlack = new(0, 0, 0, 150);
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
    public const double MenuExpandDurationMilliseconds = 300d;
    public const double MenuCollapseDurationMilliseconds = 1000d;
    public const double MenuIdleCollapseMilliseconds = 10000d;

    private const float ReferenceHeight = 780f;
    private const float MinimumLogoScale = 0.74f;
    private const float ButtonGap = 32f;

    private static readonly UiColor white = UiColor.Opaque(255, 255, 255);

    private int selectedIndex;
    private MenuVisibility menuVisibility;
    private double transitionMilliseconds;
    private double shownMilliseconds;

    public bool IsSecondMenu { get; private set; }

    public bool IsMenuShown => menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded or MenuVisibility.Collapsing;

    public MainMenuRoute Handle(MainMenuAction action)
    {
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
        if (!IsMenuShown)
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
        if (menuVisibility is MenuVisibility.Collapsed or MenuVisibility.Collapsing)
            BeginExpand();
        else
            BeginCollapse();
    }

    public void Update(TimeSpan elapsed)
    {
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
        AddMenuButtons(elements, viewport);
        AddLogo(elements, viewport);
        AddProfileShell(elements);
        AddVersionPill(elements, viewport);
        AddDownloaderTab(elements, viewport);
        AddMusicControls(elements, viewport);

        return new UiFrameSnapshot(viewport, elements, LegacyUiAssets.MainMenuManifest);
    }

    private static void AddBackground(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var background = LegacyUiAssets.MainMenuManifest.Get(LegacyUiAssets.MenuBackground);
        var scale = Math.Max(viewport.VirtualWidth / background.NativeSize.Width, viewport.VirtualHeight / background.NativeSize.Height);
        var width = background.NativeSize.Width * scale;
        var height = background.NativeSize.Height * scale;
        elements.Add(new UiElementSnapshot(
            "menu-background",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height),
            white,
            1f,
            LegacyUiAssets.MenuBackground));
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
    }

    private static void AddVersionPill(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(new UiElementSnapshot(
            "version-pill",
            UiElementKind.Fill,
            new UiRect(12f, viewport.VirtualHeight - 38f, 220f, 28f),
            translucentBlack,
            1f));
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
            LegacyUiAssets.Logo));

        elements.Add(new UiElementSnapshot(
            "logo",
            UiElementKind.Sprite,
            logoBounds,
            white,
            1f,
            LegacyUiAssets.Logo,
            UiAction.MainMenuCookie));
    }

    private void AddMenuButtons(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!IsMenuShown)
            return;

        var scale = GetMainMenuScale(viewport);
        var middleAsset = LegacyUiAssets.MainMenuManifest.Get(CurrentButtonAsset(1));
        var buttonHeight = middleAsset.NativeSize.Height * scale;
        var gap = ButtonGap * scale;
        var middleY = (viewport.VirtualHeight - buttonHeight) / 2f;
        var logoBounds = GetExpandedLogoBounds(viewport);
        var buttonX = logoBounds.X + logoBounds.Width * 0.52f;

        AddMenuButton(elements, 0, buttonX, middleY - buttonHeight - gap, scale, UiAction.MainMenuFirst);
        AddMenuButton(elements, 1, buttonX, middleY, scale, UiAction.MainMenuSecond);
        AddMenuButton(elements, 2, buttonX, middleY + buttonHeight + gap, scale, UiAction.MainMenuThird);
    }

    private void AddMenuButton(List<UiElementSnapshot> elements, int index, float x, float y, float scale, UiAction action)
    {
        var assetName = CurrentButtonAsset(index);
        var asset = LegacyUiAssets.MainMenuManifest.Get(assetName);
        var alpha = index == selectedIndex ? 1f : 0.92f;
        elements.Add(new UiElementSnapshot(
            $"menu-{index}",
            UiElementKind.Sprite,
            new UiRect(x, y, asset.NativeSize.Width * scale, asset.NativeSize.Height * scale),
            white,
            alpha,
            assetName,
            action));
    }

    private string CurrentButtonAsset(int index)
    {
        if (!IsSecondMenu)
        {
            return index switch
            {
                0 => LegacyUiAssets.Play,
                1 => LegacyUiAssets.Options,
                2 => LegacyUiAssets.Exit,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
        }

        return index switch
        {
            0 => LegacyUiAssets.Solo,
            1 => LegacyUiAssets.Multi,
            2 => LegacyUiAssets.Back,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
        };
    }

    private static void AddDownloaderTab(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var tab = LegacyUiAssets.MainMenuManifest.Get(LegacyUiAssets.BeatmapDownloader);
        elements.Add(new UiElementSnapshot(
            "beatmap-downloader",
            UiElementKind.Sprite,
            new UiRect(viewport.VirtualWidth - tab.NativeSize.Width, (viewport.VirtualHeight - tab.NativeSize.Height) / 2f, tab.NativeSize.Width, tab.NativeSize.Height),
            white,
            0.92f,
            LegacyUiAssets.BeatmapDownloader));
    }

    private static void AddMusicControls(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var nowPlaying = LegacyUiAssets.MainMenuManifest.Get(LegacyUiAssets.MusicNowPlaying);
        var nowPlayingWidth = MusicNowPlayingHeight * nowPlaying.NativeSize.Width / nowPlaying.NativeSize.Height;
        elements.Add(new UiElementSnapshot(
            "music-now-playing",
            UiElementKind.Sprite,
            new UiRect(viewport.VirtualWidth - MusicNowPlayingXOffset, 0f, nowPlayingWidth, MusicNowPlayingHeight),
            white,
            0.9f,
            LegacyUiAssets.MusicNowPlaying));

        AddMusicControl(elements, LegacyUiAssets.MusicPrevious, viewport.VirtualWidth, 6f);
        AddMusicControl(elements, LegacyUiAssets.MusicPlay, viewport.VirtualWidth, 5f);
        AddMusicControl(elements, LegacyUiAssets.MusicPause, viewport.VirtualWidth, 4f);
        AddMusicControl(elements, LegacyUiAssets.MusicStop, viewport.VirtualWidth, 3f);
        AddMusicControl(elements, LegacyUiAssets.MusicNext, viewport.VirtualWidth, 2f);
    }

    private static void AddMusicControl(List<UiElementSnapshot> elements, string assetName, float viewportWidth, float legacyIndex)
    {
        elements.Add(new UiElementSnapshot(
            assetName,
            UiElementKind.Sprite,
            new UiRect(viewportWidth - MusicControlStep * legacyIndex + MusicControlRightOffset, MusicControlY, MusicControlSize, MusicControlSize),
            white,
            0.9f,
            assetName));
    }

    private UiRect GetLogoBounds(VirtualViewport viewport)
    {
        var collapsed = GetCenteredLogoBounds(viewport);
        var expanded = GetExpandedLogoBounds(viewport);
        var progress = GetMenuTransitionProgress();
        return new UiRect(
            Lerp(collapsed.X, expanded.X, progress),
            collapsed.Y,
            collapsed.Width,
            collapsed.Height);
    }

    private static UiRect GetCenteredLogoBounds(VirtualViewport viewport)
    {
        var logo = LegacyUiAssets.MainMenuManifest.Get(LegacyUiAssets.Logo);
        var scale = GetMainMenuScale(viewport);
        var width = logo.NativeSize.Width * scale;
        var height = logo.NativeSize.Height * scale;
        return new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height);
    }

    private static UiRect GetExpandedLogoBounds(VirtualViewport viewport)
    {
        var logo = LegacyUiAssets.MainMenuManifest.Get(LegacyUiAssets.Logo);
        var scale = GetMainMenuScale(viewport);
        var width = logo.NativeSize.Width * scale;
        var height = logo.NativeSize.Height * scale;
        return new UiRect(viewport.VirtualWidth / 3f - width / 2f, (viewport.VirtualHeight - height) / 2f, width, height);
    }

    private float GetMenuTransitionProgress() => menuVisibility switch
    {
        MenuVisibility.Collapsed => 0f,
        MenuVisibility.Expanding => (float)Math.Clamp(transitionMilliseconds / MenuExpandDurationMilliseconds, 0d, 1d),
        MenuVisibility.Expanded => 1f,
        MenuVisibility.Collapsing => 1f - (float)Math.Clamp(transitionMilliseconds / MenuCollapseDurationMilliseconds, 0d, 1d),
        _ => 0f,
    };

    private static float Lerp(float start, float end, float progress) => start + (end - start) * progress;

    private static float GetMainMenuScale(VirtualViewport viewport) =>
        Math.Clamp(viewport.VirtualHeight / ReferenceHeight, MinimumLogoScale, 1f);

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
