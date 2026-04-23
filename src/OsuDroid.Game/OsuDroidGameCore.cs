using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private enum ActiveScene
    {
        Startup,
        MainMenu,
        Options,
        BeatmapDownloader,
        BeatmapProcessing,
        SongSelect,
    }

    private readonly StartupScene startup;
    private readonly MainMenuScene mainMenu;
    private readonly OptionsScene options;
    private readonly BeatmapDownloaderScene beatmapDownloader;
    private readonly SongSelectScene songSelect;
    private readonly IBeatmapLibrary beatmapLibrary;
    private readonly IBeatmapProcessingService beatmapProcessingService;
    private readonly IMenuMusicController musicController;
    private readonly IGameSettingsStore settingsStore;
    private readonly Random random = new();
    private readonly float[] menuSpectrumBuffer = new float[512];
    private ITextInputService textInputService;
    private IBeatmapPreviewPlayer previewPlayer;
    private IMenuSfxPlayer activeMenuSfxPlayer;
    private ActiveScene activeScene;
    private bool menuMusicPreviewEnabled;
    private bool startMenuMusicAfterStartup;
    private MenuNowPlayingState? preservedDownloaderMusicState;
    private string? pendingSongSelectBeatmapSetDirectory;
    private string? pendingSongSelectBeatmapFilename;

    public OsuDroidGameCore(GameServices services)
    {
        Services = services;
        settingsStore = services.SettingsStore ?? new JsonGameSettingsStore(Path.Combine(services.Paths.CoreRoot, "config", "settings.json"));
        menuMusicPreviewEnabled = settingsStore.GetBool("musicpreview", true);
        var profile = services.OnlineProfile ?? OnlineProfileSnapshot.Guest;
        startup = new StartupScene();
        mainMenu = new MainMenuScene(services.DisplayVersion, services.NowPlaying ?? new MenuNowPlayingState(), profile, string.Equals(services.BuildType, "debug", StringComparison.OrdinalIgnoreCase));
        options = new OptionsScene(new GameLocalizer(), settingsStore, pathDefaults: OptionsPathDefaults.FromPaths(services.Paths));
        textInputService = services.TextInputService ?? new NoOpTextInputService();
        previewPlayer = services.BeatmapPreviewPlayer ?? new NoOpBeatmapPreviewPlayer();
        var difficultyService = services.BeatmapDifficultyService ?? new BeatmapDifficultyService(new BeatmapLibraryRepository(services.Database), services.Paths.Songs);
        difficultyService.EnsureCalculatorVersions();
        beatmapLibrary = services.BeatmapLibrary ?? CreateBeatmapLibrary(services.Database, services.Paths);
        var initialLibrary = beatmapLibrary.Load();
        if (initialLibrary.Sets.Count == 0 || beatmapLibrary.NeedsScanRefresh())
            _ = Task.Run(() => beatmapLibrary.Scan());
        var mirrorClient = services.BeatmapMirrorClient ?? new OsuDirectMirrorClient(new HttpClient());
        var importService = services.BeatmapImportService ?? new BeatmapImportService(services.Paths, beatmapLibrary);
        beatmapProcessingService = services.BeatmapProcessingService ?? new BeatmapProcessingService(services.Paths, importService, beatmapLibrary);
        var downloadService = services.BeatmapDownloadService ?? new BeatmapDownloadService(services.Paths, mirrorClient, importService);
        beatmapDownloader = new BeatmapDownloaderScene(mirrorClient, downloadService, textInputService, previewPlayer, Path.Combine(services.Paths.CacheRoot, "Covers"));
        musicController = services.MusicController ?? new PreviewMenuMusicController(previewPlayer);
        activeMenuSfxPlayer = services.MenuSfxPlayer ?? new NoOpMenuSfxPlayer();
        ApplyOptionAudioVolumes();
        songSelect = new SongSelectScene(beatmapLibrary, musicController, difficultyService, services.Paths.Songs, profile, textInputService);
        activeScene = services.ShowStartupScene ? ActiveScene.Startup : ActiveScene.MainMenu;
        QueueStartupPlaylist(beatmapLibrary, activeScene != ActiveScene.Startup);
        mainMenu.SetNowPlaying(musicController.State);
    }

    public GameServices Services { get; }

    public MainMenuRoute LastRoute { get; private set; }

    public string? PendingExternalUrl { get; private set; }

    public MenuMusicCommand LastMusicCommand => musicController.LastCommand;


    public void AttachPlatformServices(ITextInputService? platformTextInputService, IBeatmapPreviewPlayer? platformPreviewPlayer, IMenuSfxPlayer? platformMenuSfxPlayer = null)
    {
        if (platformTextInputService is not null)
            AttachTextInputService(platformTextInputService);

        if (platformPreviewPlayer is not null)
            AttachPreviewPlayer(platformPreviewPlayer);

        if (platformMenuSfxPlayer is not null)
            AttachMenuSfxPlayer(platformMenuSfxPlayer);
    }

    public static OsuDroidGameCore Create(string corePath, string buildType, string displayVersion = "1.0", bool showStartupScene = false) =>
        Create(DroidPathRoots.FromCoreRoot(corePath), buildType, displayVersion, showStartupScene);

    public static OsuDroidGameCore Create(DroidPathRoots pathRoots, string buildType, string displayVersion = "1.0", bool showStartupScene = false)
    {
        var pathLayout = new DroidGamePathLayout(pathRoots);
        pathLayout.EnsureDirectories();
        var database = new DroidDatabase(pathLayout.GetDatabasePath(buildType));
        database.EnsureCreated();
        var library = CreateBeatmapLibrary(database, pathLayout);
        var mirrorClient = new OsuDirectMirrorClient(new HttpClient());
        var importService = new BeatmapImportService(pathLayout, library);
        var downloadService = new BeatmapDownloadService(pathLayout, mirrorClient, importService);
        var settingsStore = new JsonGameSettingsStore(Path.Combine(pathLayout.CoreRoot, "config", "settings.json"));
        return new OsuDroidGameCore(new GameServices(database, pathLayout, buildType, displayVersion, BeatmapLibrary: library, BeatmapImportService: importService, BeatmapDownloadService: downloadService, BeatmapMirrorClient: mirrorClient, SettingsStore: settingsStore, ShowStartupScene: showStartupScene));
    }

    public GameFrameSnapshot CurrentFrame => CreateFrame(VirtualViewport.LegacyLandscape);

    public GameFrameSnapshot CreateFrame(VirtualViewport viewport) => activeScene switch
    {
        ActiveScene.Startup => startup.CreateSnapshot(viewport),
        ActiveScene.MainMenu => mainMenu.CreateSnapshot(viewport),
        ActiveScene.Options => options.CreateSnapshot(viewport),
        ActiveScene.BeatmapDownloader => beatmapDownloader.CreateSnapshot(viewport),
        ActiveScene.BeatmapProcessing => BootstrapLoadingScene.CreateSnapshot(viewport, CreateBeatmapProcessingProgress(), TimeSpan.Zero),
        ActiveScene.SongSelect => songSelect.CreateSnapshot(viewport),
        _ => throw new InvalidOperationException($"Unknown scene: {activeScene}"),
    };

    public IReadOnlyList<UiFrameSnapshot> CreateWarmupFrames(VirtualViewport viewport)
    {
        songSelect.PrepareForWarmup();
        var frames = new List<UiFrameSnapshot>(OptionsScene.AllSections.Count + 3)
        {
            mainMenu.CreateSnapshot(viewport).UiFrame,
            mainMenu.CreateAboutDialogSnapshot(viewport).UiFrame,
            songSelect.CreateSnapshot(viewport).UiFrame,
        };

        foreach (var section in OptionsScene.AllSections)
            frames.Add(options.CreateSnapshotForSection(section, viewport).UiFrame);

        return frames;
    }

    public void Update(TimeSpan elapsed)
    {
        if (activeScene == ActiveScene.Startup)
        {
            startup.Update(elapsed);
            if (startup.ConsumeWelcomeSoundsRequest())
            {
                activeMenuSfxPlayer.Play("welcome");
                activeMenuSfxPlayer.Play("welcome_piano");
            }

            if (!startup.IsComplete)
                return;

            activeScene = ActiveScene.MainMenu;
            StartDeferredMenuMusic();
        }

        musicController.Update(elapsed);
        mainMenu.SetNowPlaying(musicController.State);
        mainMenu.SetSpectrum(menuSpectrumBuffer, musicController.TryReadSpectrum1024(menuSpectrumBuffer));

        if (activeScene == ActiveScene.BeatmapDownloader)
            return;

        if (activeScene == ActiveScene.BeatmapProcessing)
        {
            if (!beatmapProcessingService.TryConsumeCompletedSnapshot(out _))
                return;

            songSelect.Enter(pendingSongSelectBeatmapSetDirectory, pendingSongSelectBeatmapFilename);
            pendingSongSelectBeatmapSetDirectory = null;
            pendingSongSelectBeatmapFilename = null;
            activeScene = ActiveScene.SongSelect;
        }

        if (activeScene == ActiveScene.SongSelect)
        {
            songSelect.Update(elapsed);
            return;
        }

        if (activeScene != ActiveScene.MainMenu)
            return;

        mainMenu.Update(elapsed);
        var pendingRoute = mainMenu.ConsumePendingRoute();
        if (pendingRoute == MainMenuRoute.None)
            return;

        LastRoute = pendingRoute;
        ApplyRoute(pendingRoute);
    }

    public void TapMainMenuCookie() => mainMenu.ToggleCookie();

    public void BackToMainMenu() => BackToMainMenu(MainMenuReturnTransition.None);

    public void BackToMainMenu(MainMenuReturnTransition transition)
    {
        var returnBackgroundPath = activeScene == ActiveScene.SongSelect && transition == MainMenuReturnTransition.SongSelectBack
            ? songSelect.SelectedBackgroundPath
            : null;

        if (activeScene == ActiveScene.SongSelect)
            songSelect.Leave();
        else if (activeScene == ActiveScene.BeatmapDownloader)
        {
            beatmapDownloader.Leave();
            RestoreDownloaderMusic();
        }

        activeScene = ActiveScene.MainMenu;

        if (transition == MainMenuReturnTransition.SongSelectBack)
            mainMenu.StartReturnTransition(returnBackgroundPath);
    }

    public void PressUiAction(UiAction action)
    {
        if (activeScene == ActiveScene.MainMenu)
            mainMenu.Press(action);
    }

    public void ReleaseUiAction()
    {
        if (activeScene == ActiveScene.MainMenu)
            mainMenu.ReleasePress();
    }

    public void ScrollActiveScene(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (activeScene == ActiveScene.Options)
            options.Scroll(deltaY, point, viewport);
        else if (activeScene == ActiveScene.BeatmapDownloader)
            beatmapDownloader.Scroll(deltaY, viewport);
        else if (activeScene == ActiveScene.SongSelect)
            songSelect.Scroll(deltaY, point, viewport);
    }

    public void ScrollActiveScene(float deltaY, VirtualViewport viewport)
    {
        if (activeScene == ActiveScene.Options)
            options.Scroll(deltaY, viewport);
        else if (activeScene == ActiveScene.BeatmapDownloader)
            beatmapDownloader.Scroll(deltaY, viewport);
        else if (activeScene == ActiveScene.SongSelect)
            songSelect.Scroll(deltaY, viewport);
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

    public bool HandleUiLongPress(UiAction action, VirtualViewport viewport)
    {
        if (activeScene != ActiveScene.SongSelect || !UiActionGroups.TryGetSongSelectDifficultyIndex(action, out var index))
            return false;

        PlayMenuSfx(UiAction.SongSelectBeatmapOptions);
        songSelect.OpenPropertiesForDifficulty(index);
        return true;
    }

    public string? ConsumePendingExternalUrl()
    {
        var pendingUrl = PendingExternalUrl;
        PendingExternalUrl = null;
        return pendingUrl;
    }


    private static BeatmapLibrary CreateBeatmapLibrary(DroidDatabase database, DroidGamePathLayout pathLayout)
    {
        var repository = new BeatmapLibraryRepository(database);
        return new BeatmapLibrary(pathLayout, repository);
    }

    private BootstrapLoadingProgress CreateBeatmapProcessingProgress()
    {
        var state = beatmapProcessingService.State;
        return new BootstrapLoadingProgress(state.Percent, state.StatusText, BootstrapLoadingKind.BeatmapProcessing);
    }

    private void AttachTextInputService(ITextInputService service)
    {
        if (ReferenceEquals(textInputService, service))
            return;

        textInputService = service;
        options.SetTextInputService(service);
        beatmapDownloader.SetTextInputService(service);
        songSelect.SetTextInputService(service);
    }

    private void AttachPreviewPlayer(IBeatmapPreviewPlayer player)
    {
        if (ReferenceEquals(previewPlayer, player))
            return;

        previewPlayer = player;
        ApplyMusicVolumeSetting();
        musicController.SetPreviewPlayer(player);
        songSelect.SetPreviewPlayer(player);
        beatmapDownloader.SetPreviewPlayer(player);
        if (!menuMusicPreviewEnabled || string.IsNullOrWhiteSpace(musicController.State.ArtistTitle))
            return;

        if (activeScene == ActiveScene.Startup)
        {
            startMenuMusicAfterStartup = true;
            return;
        }

        musicController.Execute(MenuMusicCommand.Play);
        mainMenu.SetNowPlaying(musicController.State);
    }

    private void StartDeferredMenuMusic()
    {
        if (!startMenuMusicAfterStartup)
            return;

        startMenuMusicAfterStartup = false;
        if (!menuMusicPreviewEnabled || string.IsNullOrWhiteSpace(musicController.State.ArtistTitle))
            return;

        musicController.Execute(MenuMusicCommand.Play);
        mainMenu.SetNowPlaying(musicController.State);
    }

    private void AttachMenuSfxPlayer(IMenuSfxPlayer player)
    {
        if (ReferenceEquals(activeMenuSfxPlayer, player))
            return;

        activeMenuSfxPlayer = player;
        ApplyEffectVolumeSetting();
    }


}
