using System.Globalization;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;

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

    private readonly StartupScene _startup;
    private readonly MainMenuScene _mainMenu;
    private readonly OptionsScene _options;
    private readonly BeatmapDownloaderScene _beatmapDownloader;
    private readonly SongSelectScene _songSelect;
    private readonly IBeatmapLibrary _beatmapLibrary;
    private readonly IBeatmapProcessingService _beatmapProcessingService;
    private readonly IMenuMusicController _musicController;
    private readonly IGameSettingsStore _settingsStore;
    private readonly Random _random = new();
    private readonly float[] _menuSpectrumBuffer = new float[512];
    private ITextInputService _textInputService;
    private IBeatmapPreviewPlayer _previewPlayer;
    private IMenuSfxPlayer _activeMenuSfxPlayer;
    private ActiveScene _activeScene;
    private bool _menuMusicPreviewEnabled;
    private bool _startMenuMusicAfterStartup;
    private MenuNowPlayingState? _preservedDownloaderMusicState;
    private string? _pendingSongSelectBeatmapSetDirectory;
    private string? _pendingSongSelectBeatmapFilename;

    public OsuDroidGameCore(GameServices services)
    {
        Services = services;
        _settingsStore = services.SettingsStore ?? new JsonGameSettingsStore(Path.Combine(services.Paths.CoreRoot, "config", "settings.json"));
        _menuMusicPreviewEnabled = _settingsStore.GetBool("musicpreview", true);
        var localizer = new GameLocalizer();
        OnlineProfileSnapshot profile = services.OnlineProfile ?? OnlineProfileSnapshot.Guest;
        _startup = new StartupScene();
        _mainMenu = new MainMenuScene(services.DisplayVersion, services.NowPlaying ?? new MenuNowPlayingState(), profile, string.Equals(services.BuildType, "debug", StringComparison.OrdinalIgnoreCase), localizer);
        _options = new OptionsScene(localizer, _settingsStore, pathDefaults: OptionsPathDefaults.FromPaths(services.Paths));
        _textInputService = services.TextInputService ?? new NoOpTextInputService();
        _previewPlayer = services.BeatmapPreviewPlayer ?? new NoOpBeatmapPreviewPlayer();
        IBeatmapDifficultyService difficultyService = services.BeatmapDifficultyService ?? new BeatmapDifficultyService(new BeatmapLibraryRepository(services.Database), services.Paths.Songs, algorithm: ReadDifficultyAlgorithmSetting());
        difficultyService.EnsureCalculatorVersions();
        _beatmapLibrary = services.BeatmapLibrary ?? CreateBeatmapLibrary(services.Database, services.Paths);
        BeatmapLibrarySnapshot initialLibrary = _beatmapLibrary.Load();
        if (initialLibrary.Sets.Count == 0 || _beatmapLibrary.NeedsScanRefresh())
        {
            _ = Task.Run(() => _beatmapLibrary.Scan());
        }

        IBeatmapMirrorClient mirrorClient = services.BeatmapMirrorClient ?? new OsuDirectMirrorClient(new HttpClient());
        IBeatmapImportService importService = services.BeatmapImportService ?? new BeatmapImportService(services.Paths, _beatmapLibrary);
        _beatmapProcessingService = services.BeatmapProcessingService ?? new BeatmapProcessingService(services.Paths, importService, _beatmapLibrary, _settingsStore);
        IBeatmapDownloadService downloadService = services.BeatmapDownloadService ?? new BeatmapDownloadService(services.Paths, mirrorClient, _beatmapProcessingService);
        _beatmapDownloader = new BeatmapDownloaderScene(mirrorClient, downloadService, _textInputService, _previewPlayer, Path.Combine(services.Paths.CacheRoot, "Covers"), localizer);
        _musicController = services.MusicController ?? new PreviewMenuMusicController(_previewPlayer);
        _activeMenuSfxPlayer = services.MenuSfxPlayer ?? new NoOpMenuSfxPlayer();
        ApplyOptionAudioVolumes();
        _songSelect = new SongSelectScene(_beatmapLibrary, _musicController, difficultyService, services.Paths.Songs, profile, _textInputService, localizer: localizer);
        ApplyOptionsRuntimeSettings();
        _activeScene = services.ShowStartupScene ? ActiveScene.Startup : ActiveScene.MainMenu;
        QueueStartupPlaylist(_beatmapLibrary, _activeScene != ActiveScene.Startup);
        _mainMenu.SetNowPlaying(_musicController.State);
    }

    public GameServices Services { get; }

    public MainMenuRoute LastRoute { get; private set; }

    public string? PendingExternalUrl { get; private set; }

    public MenuMusicCommand LastMusicCommand => _musicController.LastCommand;


    public void AttachPlatformServices(ITextInputService? platformTextInputService, IBeatmapPreviewPlayer? platformPreviewPlayer, IMenuSfxPlayer? platformMenuSfxPlayer = null)
    {
        if (platformTextInputService is not null)
        {
            AttachTextInputService(platformTextInputService);
        }

        if (platformPreviewPlayer is not null)
        {
            AttachPreviewPlayer(platformPreviewPlayer);
        }

        if (platformMenuSfxPlayer is not null)
        {
            AttachMenuSfxPlayer(platformMenuSfxPlayer);
        }
    }

    public static OsuDroidGameCore Create(string corePath, string buildType, string displayVersion = "1.0", bool showStartupScene = false) =>
        Create(DroidPathRoots.FromCoreRoot(corePath), buildType, displayVersion, showStartupScene);

    public static OsuDroidGameCore Create(DroidPathRoots pathRoots, string buildType, string displayVersion = "1.0", bool showStartupScene = false)
    {
        var pathLayout = new DroidGamePathLayout(pathRoots);
        pathLayout.EnsureDirectories();
        var database = new DroidDatabase(pathLayout.GetDatabasePath(buildType));
        database.EnsureCreated();
        BeatmapLibrary library = CreateBeatmapLibrary(database, pathLayout);
        var mirrorClient = new OsuDirectMirrorClient(new HttpClient());
        var _settingsStore = new JsonGameSettingsStore(Path.Combine(pathLayout.CoreRoot, "config", "settings.json"));
        var importService = new BeatmapImportService(pathLayout, library);
        var processingService = new BeatmapProcessingService(pathLayout, importService, library, _settingsStore);
        var downloadService = new BeatmapDownloadService(pathLayout, mirrorClient, processingService);
        return new OsuDroidGameCore(new GameServices(database, pathLayout, buildType, displayVersion, BeatmapLibrary: library, BeatmapImportService: importService, BeatmapProcessingService: processingService, BeatmapDownloadService: downloadService, BeatmapMirrorClient: mirrorClient, SettingsStore: _settingsStore, ShowStartupScene: showStartupScene));
    }

    public GameFrameSnapshot CurrentFrame => CreateFrame(VirtualViewport.LegacyLandscape);

    public GameFrameSnapshot CreateFrame(VirtualViewport viewport) => _activeScene switch
    {
        ActiveScene.Startup => _startup.CreateSnapshot(viewport),
        ActiveScene.MainMenu => _mainMenu.CreateSnapshot(viewport),
        ActiveScene.Options => _options.CreateSnapshot(viewport),
        ActiveScene.BeatmapDownloader => _beatmapDownloader.CreateSnapshot(viewport),
        ActiveScene.BeatmapProcessing => BootstrapLoadingScene.CreateSnapshot(viewport, CreateBeatmapProcessingProgress(), TimeSpan.Zero),
        ActiveScene.SongSelect => _songSelect.CreateSnapshot(viewport),
        _ => throw new InvalidOperationException($"Unknown scene: {_activeScene}"),
    };

    public IReadOnlyList<UiFrameSnapshot> CreateWarmupFrames(VirtualViewport viewport)
    {
        _songSelect.PrepareForWarmup();
        var frames = new List<UiFrameSnapshot>(OptionsScene.AllSections.Count + 3)
        {
            _mainMenu.CreateSnapshot(viewport).UiFrame,
            _mainMenu.CreateAboutDialogSnapshot(viewport).UiFrame,
            _songSelect.CreateSnapshot(viewport).UiFrame,
        };

        foreach (OptionsSection section in OptionsScene.AllSections)
        {
            frames.Add(_options.CreateSnapshotForSection(section, viewport).UiFrame);
        }

        return frames;
    }

    public void Update(TimeSpan elapsed)
    {
        if (_activeScene == ActiveScene.Startup)
        {
            _startup.Update(elapsed);
            if (_startup.ConsumeWelcomeSoundsRequest())
            {
                _activeMenuSfxPlayer.Play("welcome");
                _activeMenuSfxPlayer.Play("welcome_piano");
            }

            if (!_startup.IsComplete)
            {
                return;
            }

            _activeScene = ActiveScene.MainMenu;
            StartDeferredMenuMusic();
        }

        _musicController.Update(elapsed);
        _mainMenu.SetNowPlaying(_musicController.State);
        _mainMenu.SetSpectrum(_menuSpectrumBuffer, _musicController.TryReadSpectrum1024(_menuSpectrumBuffer));

        if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            return;
        }

        if (_activeScene == ActiveScene.BeatmapProcessing)
        {
            if (!_beatmapProcessingService.TryConsumeCompletedSnapshot(out _))
            {
                return;
            }

            _songSelect.Enter(_pendingSongSelectBeatmapSetDirectory, _pendingSongSelectBeatmapFilename);
            _pendingSongSelectBeatmapSetDirectory = null;
            _pendingSongSelectBeatmapFilename = null;
            _activeScene = ActiveScene.SongSelect;
        }

        if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Update(elapsed);
            return;
        }

        if (_activeScene != ActiveScene.MainMenu)
        {
            return;
        }

        _mainMenu.Update(elapsed);
        MainMenuRoute pendingRoute = _mainMenu.ConsumePendingRoute();
        if (pendingRoute == MainMenuRoute.None)
        {
            return;
        }

        LastRoute = pendingRoute;
        ApplyRoute(pendingRoute);
    }

    public void TapMainMenuCookie() => _mainMenu.ToggleCookie();

    public void BackToMainMenu() => BackToMainMenu(MainMenuReturnTransition.None);

    public void BackToMainMenu(MainMenuReturnTransition transition)
    {
        string? returnBackgroundPath = _activeScene == ActiveScene.SongSelect && transition == MainMenuReturnTransition.SongSelectBack
            ? _songSelect.SelectedBackgroundPath
            : null;

        if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Leave();
        }
        else if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            _beatmapDownloader.Leave();
            RestoreDownloaderMusic();
        }

        _activeScene = ActiveScene.MainMenu;

        if (transition == MainMenuReturnTransition.SongSelectBack)
        {
            _mainMenu.StartReturnTransition(returnBackgroundPath);
        }
    }

    public void PressUiAction(UiAction action)
    {
        if (_activeScene == ActiveScene.MainMenu)
        {
            _mainMenu.Press(action);
        }
    }

    public void ReleaseUiAction()
    {
        if (_activeScene == ActiveScene.MainMenu)
        {
            _mainMenu.ReleasePress();
        }
    }

    public bool TryBeginUiDrag(string elementId, UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene != ActiveScene.Options)
        {
            return false;
        }

        bool captured = _options.TryBeginSliderDrag(elementId, point, viewport);
        if (captured)
        {
            ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
        }

        return captured;
    }

    public void UpdateUiDrag(UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene != ActiveScene.Options)
        {
            return;
        }

        if (!_options.UpdateSliderDrag(point, viewport))
        {
            return;
        }

        ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
    }

    public void EndUiDrag(UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene != ActiveScene.Options)
        {
            return;
        }

        _options.EndSliderDrag(point, viewport);
        ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
    }

    public void ScrollActiveScene(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene == ActiveScene.Options)
        {
            _options.Scroll(deltaY, point, viewport);
        }
        else if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            _beatmapDownloader.Scroll(deltaY, viewport);
        }
        else if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Scroll(deltaY, point, viewport);
        }
    }

    public void ScrollActiveScene(float deltaY, VirtualViewport viewport)
    {
        if (_activeScene == ActiveScene.Options)
        {
            _options.Scroll(deltaY, viewport);
        }
        else if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            _beatmapDownloader.Scroll(deltaY, viewport);
        }
        else if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Scroll(deltaY, viewport);
        }
    }

    public MainMenuRoute HandleMainMenu(MainMenuAction action)
    {
        if (_activeScene != ActiveScene.MainMenu)
        {
            return MainMenuRoute.None;
        }

        LastRoute = _mainMenu.Handle(action);
        ApplyRoute(LastRoute);
        return LastRoute;
    }

    public MainMenuRoute TapMainMenu(MainMenuButtonSlot slot)
    {
        if (_activeScene != ActiveScene.MainMenu)
        {
            return MainMenuRoute.None;
        }

        LastRoute = _mainMenu.Tap(slot);
        ApplyRoute(LastRoute);
        return LastRoute;
    }

    public void HandleUiAction(UiAction action) => HandleUiAction(action, VirtualViewport.LegacyLandscape);

    public bool HandleUiLongPress(UiAction action, VirtualViewport _)
    {
        if (_activeScene != ActiveScene.SongSelect || !UiActionGroups.TryGetSongSelectDifficultyIndex(action, out int index))
        {
            return false;
        }

        PlayMenuSfx(UiAction.SongSelectBeatmapOptions);
        _songSelect.OpenPropertiesForDifficulty(index);
        return true;
    }

    public string? ConsumePendingExternalUrl()
    {
        string? pendingUrl = PendingExternalUrl;
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
        BeatmapProcessingState state = _beatmapProcessingService.State;
        return new BootstrapLoadingProgress(state.Percent, state.StatusText, BootstrapLoadingKind.BeatmapProcessing);
    }

    private void ApplyOptionsRuntimeSettings()
    {
        ApplyDifficultyAlgorithmSetting();
        ApplyRomanizedPreferenceSetting();
        ApplyDownloadPreferenceSetting();
    }

    private DifficultyAlgorithm ReadDifficultyAlgorithmSetting() => _settingsStore.GetInt("difficultyAlgorithm", 0) == 1 ? DifficultyAlgorithm.Standard : DifficultyAlgorithm.Droid;

    private static string CurrentUpdateLanguageCode()
    {
        string language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return string.IsNullOrWhiteSpace(language) ? "en" : language;
    }

    private void AttachTextInputService(ITextInputService service)
    {
        if (ReferenceEquals(_textInputService, service))
        {
            return;
        }

        _textInputService = service;
        _options.SetTextInputService(service);
        _beatmapDownloader.SetTextInputService(service);
        _songSelect.SetTextInputService(service);
    }

    private void AttachPreviewPlayer(IBeatmapPreviewPlayer player)
    {
        if (ReferenceEquals(_previewPlayer, player))
        {
            return;
        }

        _previewPlayer = player;
        ApplyMusicVolumeSetting();
        _musicController.SetPreviewPlayer(player);
        _songSelect.SetPreviewPlayer(player);
        _beatmapDownloader.SetPreviewPlayer(player);
        if (!_menuMusicPreviewEnabled || string.IsNullOrWhiteSpace(_musicController.State.ArtistTitle))
        {
            return;
        }

        if (_activeScene == ActiveScene.Startup)
        {
            _startMenuMusicAfterStartup = true;
            return;
        }

        _musicController.Execute(MenuMusicCommand.Play);
        _mainMenu.SetNowPlaying(_musicController.State);
    }

    private void StartDeferredMenuMusic()
    {
        if (!_startMenuMusicAfterStartup)
        {
            return;
        }

        _startMenuMusicAfterStartup = false;
        if (!_menuMusicPreviewEnabled || string.IsNullOrWhiteSpace(_musicController.State.ArtistTitle))
        {
            return;
        }

        _musicController.Execute(MenuMusicCommand.Play);
        _mainMenu.SetNowPlaying(_musicController.State);
    }

    private void AttachMenuSfxPlayer(IMenuSfxPlayer player)
    {
        if (ReferenceEquals(_activeMenuSfxPlayer, player))
        {
            return;
        }

        _activeMenuSfxPlayer = player;
        ApplyEffectVolumeSetting();
    }


}
