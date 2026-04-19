using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game;

public sealed class OsuDroidGameCore
{
    private enum ActiveScene
    {
        MainMenu,
        Options,
    }

    private readonly MainMenuScene mainMenu;
    private readonly OptionsScene options;
    private readonly IMenuMusicController musicController;
    private ActiveScene activeScene;

    public OsuDroidGameCore(GameServices services)
    {
        Services = services;
        mainMenu = new MainMenuScene(services.DisplayVersion, services.NowPlaying ?? new MenuNowPlayingState());
        options = new OptionsScene(new GameLocalizer());
        musicController = services.MusicController ?? new NoOpMenuMusicController();
    }

    public GameServices Services { get; }

    public MainMenuRoute LastRoute { get; private set; }

    public string? PendingExternalUrl { get; private set; }

    public MenuMusicCommand LastMusicCommand => musicController.LastCommand;

    public static OsuDroidGameCore Create(string corePath, string buildType, string displayVersion = "1.0")
    {
        var databasePath = DroidDatabaseConstants.GetDatabasePath(corePath, buildType);
        var database = new DroidDatabase(databasePath);
        database.EnsureCreated();
        return new OsuDroidGameCore(new GameServices(database, corePath, buildType, displayVersion));
    }

    public GameFrameSnapshot CurrentFrame => CreateFrame(VirtualViewport.LegacyLandscape);

    public GameFrameSnapshot CreateFrame(VirtualViewport viewport) => activeScene switch
    {
        ActiveScene.MainMenu => mainMenu.CreateSnapshot(viewport),
        ActiveScene.Options => options.CreateSnapshot(viewport),
        _ => throw new InvalidOperationException($"Unknown scene: {activeScene}"),
    };

    public void Update(TimeSpan elapsed)
    {
        if (activeScene == ActiveScene.MainMenu)
            mainMenu.Update(elapsed);
    }

    public void TapMainMenuCookie() => mainMenu.ToggleCookie();

    public void BackToMainMenu() => activeScene = ActiveScene.MainMenu;

    public void ScrollActiveScene(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (activeScene == ActiveScene.Options)
            options.Scroll(deltaY, point, viewport);
    }

    public void ScrollActiveScene(float deltaY, VirtualViewport viewport)
    {
        if (activeScene == ActiveScene.Options)
            options.Scroll(deltaY, viewport);
    }

    public MainMenuRoute HandleMainMenu(MainMenuAction action)
    {
        if (activeScene != ActiveScene.MainMenu)
            return MainMenuRoute.None;

        LastRoute = mainMenu.Handle(action);
        ApplyRoute(LastRoute);
        return LastRoute;
    }

    public MainMenuRoute TapMainMenu(MainMenuButtonSlot slot)
    {
        if (activeScene != ActiveScene.MainMenu)
            return MainMenuRoute.None;

        LastRoute = mainMenu.Tap(slot);
        ApplyRoute(LastRoute);
        return LastRoute;
    }

    public void HandleUiAction(UiAction action) => HandleUiAction(action, VirtualViewport.LegacyLandscape);

    public void HandleUiAction(UiAction action, VirtualViewport viewport)
    {
        switch (action)
        {
            case UiAction.MainMenuCookie:
                TapMainMenuCookie();
                break;

            case UiAction.MainMenuFirst:
            case UiAction.MainMenuSecond:
            case UiAction.MainMenuThird:
                TapMainMenu(UiActionRouter.ToMainMenuSlot(action));
                break;

            case UiAction.MainMenuVersionPill:
                mainMenu.OpenAboutDialog();
                break;

            case UiAction.MainMenuAboutClose:
                mainMenu.CloseAboutDialog();
                break;

            case UiAction.MainMenuAboutChangelog:
                PendingExternalUrl = "https://osudroid.moe/changelog/latest";
                mainMenu.CloseAboutDialog();
                break;

            case UiAction.MainMenuAboutOsuWebsite:
                PendingExternalUrl = "https://osu.ppy.sh";
                break;

            case UiAction.MainMenuAboutOsuDroidWebsite:
                PendingExternalUrl = "https://osudroid.moe";
                break;

            case UiAction.MainMenuAboutDiscord:
                PendingExternalUrl = "https://discord.gg/nyD92cE";
                break;

            case UiAction.MainMenuMusicPrevious:
                musicController.Execute(MenuMusicCommand.Previous);
                break;

            case UiAction.MainMenuMusicPlay:
                musicController.Execute(MenuMusicCommand.Play);
                break;

            case UiAction.MainMenuMusicPause:
                musicController.Execute(MenuMusicCommand.Pause);
                break;

            case UiAction.MainMenuMusicStop:
                musicController.Execute(MenuMusicCommand.Stop);
                break;

            case UiAction.MainMenuMusicNext:
                musicController.Execute(MenuMusicCommand.Next);
                break;

            case UiAction.OptionsBack:
                BackToMainMenu();
                break;

            case UiAction.OptionsSectionGeneral:
            case UiAction.OptionsSectionGameplay:
            case UiAction.OptionsSectionGraphics:
            case UiAction.OptionsSectionAudio:
            case UiAction.OptionsSectionLibrary:
            case UiAction.OptionsSectionInput:
            case UiAction.OptionsSectionAdvanced:
            case UiAction.OptionsToggleServerConnection:
            case UiAction.OptionsToggleLoadAvatar:
            case UiAction.OptionsToggleAnnouncements:
            case UiAction.OptionsToggleMusicPreview:
            case UiAction.OptionsToggleShiftPitch:
            case UiAction.OptionsToggleBeatmapSounds:
                if (activeScene == ActiveScene.Options)
                    options.HandleAction(action, viewport);
                break;
        }
    }

    public string? ConsumePendingExternalUrl()
    {
        var pendingUrl = PendingExternalUrl;
        PendingExternalUrl = null;
        return pendingUrl;
    }

    private void ApplyRoute(MainMenuRoute route)
    {
        if (route == MainMenuRoute.Settings)
            activeScene = ActiveScene.Options;
    }
}
