using System.Globalization;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.Scenes.Startup;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private OnlineProfilePanelState? CreateOnlinePanelState(OnlineProfileSnapshot? profile) =>
        _settingsStore.GetBool("stayOnline", false)
            ? OnlineProfilePanelState.FromOptionalProfile(profile)
            : null;

    public void AttachPlatformServices(
        ITextInputService? platformTextInputService,
        IBeatmapPreviewPlayer? platformPreviewPlayer,
        IMenuSfxPlayer? platformMenuSfxPlayer = null
    )
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

    public static OsuDroidGameCore Create(
        string corePath,
        string buildType,
        string displayVersion = "1.0",
        bool showStartupScene = false
    ) => Create(DroidPathRoots.FromCoreRoot(corePath), buildType, displayVersion, showStartupScene);

    public static OsuDroidGameCore Create(
        DroidPathRoots pathRoots,
        string buildType,
        string displayVersion = "1.0",
        bool showStartupScene = false
    )
    {
        var pathLayout = new DroidGamePathLayout(pathRoots);
        pathLayout.EnsureDirectories();
        var database = new DroidDatabase(pathLayout.GetDatabasePath(buildType));
        database.EnsureCreated();
        BeatmapLibrary library = CreateBeatmapLibrary(database, pathLayout);
        var mirrorClient = new OsuDirectMirrorClient(new HttpClient());
        var _settingsStore = new JsonGameSettingsStore(
            Path.Combine(pathLayout.CoreRoot, "config", "settings.json")
        );
        var importService = new BeatmapImportService(pathLayout, library);
        var processingService = new BeatmapProcessingService(
            pathLayout,
            importService,
            library,
            _settingsStore
        );
        var downloadService = new BeatmapDownloadService(
            pathLayout,
            mirrorClient,
            processingService
        );
        return new OsuDroidGameCore(
            new GameServices(
                database,
                pathLayout,
                buildType,
                displayVersion,
                BeatmapLibrary: library,
                BeatmapImportService: importService,
                BeatmapProcessingService: processingService,
                BeatmapDownloadService: downloadService,
                BeatmapMirrorClient: mirrorClient,
                SettingsStore: _settingsStore,
                ShowStartupScene: showStartupScene
            )
        );
    }

    private static BeatmapLibrary CreateBeatmapLibrary(
        DroidDatabase database,
        DroidGamePathLayout pathLayout
    )
    {
        var repository = new BeatmapLibraryRepository(database);
        return new BeatmapLibrary(pathLayout, repository);
    }

    private BootstrapLoadingProgress CreateBeatmapProcessingProgress()
    {
        BeatmapProcessingState state = _beatmapProcessingService.State;
        return new BootstrapLoadingProgress(
            state.Percent,
            state.StatusText,
            BootstrapLoadingKind.BeatmapProcessing
        );
    }

    private void ApplyOptionsRuntimeSettings()
    {
        ApplyDifficultyAlgorithmSetting();
        ApplyRomanizedPreferenceSetting();
        ApplyDownloadPreferenceSetting();
        ApplyOnlinePanelSetting();
    }

    private DifficultyAlgorithm ReadDifficultyAlgorithmSetting() =>
        _settingsStore.GetInt("difficultyAlgorithm", 0) == 1
            ? DifficultyAlgorithm.Standard
            : DifficultyAlgorithm.Droid;

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
        _modSelect.SetTextInputService(service);
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
        if (
            !_menuMusicPreviewEnabled
            || string.IsNullOrWhiteSpace(_musicController.State.ArtistTitle)
        )
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
