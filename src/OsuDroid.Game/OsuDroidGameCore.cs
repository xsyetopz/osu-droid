using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.Scenes.BeatmapDownloader;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.Scenes.ModSelect;
using OsuDroid.Game.Scenes.Options;
using OsuDroid.Game.Scenes.SongSelect;
using OsuDroid.Game.Scenes.Startup;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Input;

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
        ModSelect,
    }

    private readonly StartupScene _startup;
    private readonly MainMenuScene _mainMenu;
    private readonly OptionsScene _options;
    private readonly BeatmapDownloaderScene _beatmapDownloader;
    private readonly SongSelectScene _songSelect;
    private readonly ModSelectScene _modSelect;
    private readonly IBeatmapLibrary _beatmapLibrary;
    private readonly IBeatmapProcessingService _beatmapProcessingService;
#pragma warning disable CA1859 // Keep injectable interfaces for tests and platform service ownership.
    private readonly IMenuMusicController _musicController;
    private readonly IGameSettingsStore _settingsStore;
#pragma warning restore CA1859
    private readonly GameSettingsBackupService _settingsBackupService;
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
        _settingsStore =
            services.SettingsStore
            ?? new JsonGameSettingsStore(
                Path.Combine(services.Paths.CoreRoot, "config", "settings.json")
            );
        _settingsBackupService = new GameSettingsBackupService(services.Paths, _settingsStore);
        _menuMusicPreviewEnabled = _settingsStore.GetBool("musicpreview", true);
        var localizer = new GameLocalizer();
        OnlineProfilePanelState? onlinePanelState = CreateOnlinePanelState(services.OnlineProfile);
        _startup = new StartupScene();
        _mainMenu = new MainMenuScene(
            services.DisplayVersion,
            services.NowPlaying ?? new MenuNowPlayingState(),
            onlinePanelState,
            string.Equals(services.BuildType, "debug", StringComparison.OrdinalIgnoreCase),
            localizer
        );
        _options = new OptionsScene(
            localizer,
            _settingsStore,
            pathDefaults: OptionsPathDefaults.FromPaths(services.Paths)
        );
        _textInputService = services.TextInputService ?? new NoOpTextInputService();
        _previewPlayer = services.BeatmapPreviewPlayer ?? new NoOpBeatmapPreviewPlayer();
        IBeatmapDifficultyService difficultyService =
            services.BeatmapDifficultyService
            ?? new BeatmapDifficultyService(
                new BeatmapLibraryRepository(services.Database),
                services.Paths.Songs,
                algorithm: ReadDifficultyAlgorithmSetting()
            );
        difficultyService.EnsureCalculatorVersions();
        _beatmapLibrary =
            services.BeatmapLibrary ?? CreateBeatmapLibrary(services.Database, services.Paths);
        BeatmapLibrarySnapshot initialLibrary = _beatmapLibrary.Load();
        if (initialLibrary.Sets.Count == 0 || _beatmapLibrary.NeedsScanRefresh())
        {
            _ = Task.Run(() => _beatmapLibrary.Scan());
        }

        IBeatmapMirrorClient mirrorClient =
            services.BeatmapMirrorClient ?? new OsuDirectMirrorClient(new HttpClient());
        IBeatmapImportService importService =
            services.BeatmapImportService
            ?? new BeatmapImportService(services.Paths, _beatmapLibrary);
        _beatmapProcessingService =
            services.BeatmapProcessingService
            ?? new BeatmapProcessingService(
                services.Paths,
                importService,
                _beatmapLibrary,
                _settingsStore
            );
        IBeatmapDownloadService downloadService =
            services.BeatmapDownloadService
            ?? new BeatmapDownloadService(services.Paths, mirrorClient, _beatmapProcessingService);
        _beatmapDownloader = new BeatmapDownloaderScene(
            mirrorClient,
            downloadService,
            _textInputService,
            _previewPlayer,
            Path.Combine(services.Paths.CacheRoot, "Covers"),
            localizer,
            Path.Combine(services.Paths.Log, "beatmap-downloader.log")
        );
        _musicController =
            services.MusicController ?? new PreviewMenuMusicController(_previewPlayer);
        _activeMenuSfxPlayer = services.MenuSfxPlayer ?? new NoOpMenuSfxPlayer();
        ApplyOptionAudioVolumes();
        _songSelect = new SongSelectScene(
            _beatmapLibrary,
            _musicController,
            difficultyService,
            services.Paths.Songs,
            onlinePanelState,
            _textInputService,
            localizer: localizer
        );
        _modSelect = new ModSelectScene(_settingsStore, _textInputService, localizer);
        _songSelect.SetSelectedModState(_modSelect.CreateSelectionState());
        ApplyOptionsRuntimeSettings();
        _activeScene = services.ShowStartupScene ? ActiveScene.Startup : ActiveScene.MainMenu;
        QueueStartupPlaylist(_beatmapLibrary, _activeScene != ActiveScene.Startup);
        _mainMenu.SetNowPlaying(_musicController.State);
    }

    public GameServices Services { get; }

    public MainMenuRoute LastRoute { get; private set; }

    public string? PendingExternalUrl { get; private set; }

    public MenuMusicCommand LastMusicCommand => _musicController.LastCommand;
}
