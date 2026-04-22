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

public sealed class OsuDroidGameCore
{
    private enum ActiveScene
    {
        MainMenu,
        Options,
        BeatmapDownloader,
        SongSelect,
    }

    private readonly MainMenuScene mainMenu;
    private readonly OptionsScene options;
    private readonly BeatmapDownloaderScene beatmapDownloader;
    private readonly SongSelectScene songSelect;
    private readonly IBeatmapLibrary beatmapLibrary;
    private readonly IMenuMusicController musicController;
    private readonly IMenuSfxPlayer menuSfxPlayer;
    private readonly IGameSettingsStore settingsStore;
    private readonly Random random = new();
    private readonly float[] menuSpectrumBuffer = new float[512];
    private ITextInputService textInputService;
    private IBeatmapPreviewPlayer previewPlayer;
    private IMenuSfxPlayer attachedMenuSfxPlayer;
    private ActiveScene activeScene;
    private bool menuMusicPreviewEnabled;
    private MenuNowPlayingState? preservedDownloaderMusicState;

    public OsuDroidGameCore(GameServices services)
    {
        Services = services;
        settingsStore = services.SettingsStore ?? new JsonGameSettingsStore(Path.Combine(services.Paths.CoreRoot, "config", "settings.json"));
        menuMusicPreviewEnabled = settingsStore.GetBool("musicpreview", true);
        var profile = services.OnlineProfile ?? OnlineProfileSnapshot.Guest;
        mainMenu = new MainMenuScene(services.DisplayVersion, services.NowPlaying ?? new MenuNowPlayingState(), profile);
        options = new OptionsScene(new GameLocalizer(), settingsStore);
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
        var downloadService = services.BeatmapDownloadService ?? new BeatmapDownloadService(services.Paths, mirrorClient, importService);
        beatmapDownloader = new BeatmapDownloaderScene(mirrorClient, downloadService, textInputService, previewPlayer, Path.Combine(services.Paths.CacheRoot, "Covers"));
        musicController = services.MusicController ?? new PreviewMenuMusicController(previewPlayer);
        menuSfxPlayer = services.MenuSfxPlayer ?? new NoOpMenuSfxPlayer();
        attachedMenuSfxPlayer = menuSfxPlayer;
        songSelect = new SongSelectScene(beatmapLibrary, musicController, difficultyService, services.Paths.Songs, profile, textInputService);
        QueueStartupPreview(beatmapLibrary);
        mainMenu.SetNowPlaying(musicController.State);
    }

    public GameServices Services { get; }

    public MainMenuRoute LastRoute { get; private set; }

    public string? PendingExternalUrl { get; private set; }

    public MenuMusicCommand LastMusicCommand => musicController.LastCommand;


    public void AttachPlatformServices(ITextInputService? platformTextInputService, IBeatmapPreviewPlayer? platformPreviewPlayer, IMenuSfxPlayer? platformMenuSfxPlayer = null)
    {
        if (platformTextInputService is not null)
        {
            textInputService = platformTextInputService;
            beatmapDownloader.SetTextInputService(platformTextInputService);
            songSelect.SetTextInputService(platformTextInputService);
        }

        if (platformPreviewPlayer is not null)
        {
            previewPlayer = platformPreviewPlayer;
            musicController.SetPreviewPlayer(platformPreviewPlayer);
            songSelect.SetPreviewPlayer(platformPreviewPlayer);
            beatmapDownloader.SetPreviewPlayer(platformPreviewPlayer);
            if (menuMusicPreviewEnabled && !string.IsNullOrWhiteSpace(musicController.State.ArtistTitle))
            {
                musicController.Execute(MenuMusicCommand.Play);
                mainMenu.SetNowPlaying(musicController.State);
            }
        }

        if (platformMenuSfxPlayer is not null)
            attachedMenuSfxPlayer = platformMenuSfxPlayer;
    }

    public static OsuDroidGameCore Create(string corePath, string buildType, string displayVersion = "1.0") =>
        Create(DroidPathRoots.FromCoreRoot(corePath), buildType, displayVersion);

    public static OsuDroidGameCore Create(DroidPathRoots pathRoots, string buildType, string displayVersion = "1.0")
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
        return new OsuDroidGameCore(new GameServices(database, pathLayout, buildType, displayVersion, BeatmapLibrary: library, BeatmapImportService: importService, BeatmapDownloadService: downloadService, BeatmapMirrorClient: mirrorClient, SettingsStore: settingsStore));
    }

    public GameFrameSnapshot CurrentFrame => CreateFrame(VirtualViewport.LegacyLandscape);

    public GameFrameSnapshot CreateFrame(VirtualViewport viewport) => activeScene switch
    {
        ActiveScene.MainMenu => mainMenu.CreateSnapshot(viewport),
        ActiveScene.Options => options.CreateSnapshot(viewport),
        ActiveScene.BeatmapDownloader => beatmapDownloader.CreateSnapshot(viewport),
        ActiveScene.SongSelect => songSelect.CreateSnapshot(viewport),
        _ => throw new InvalidOperationException($"Unknown scene: {activeScene}"),
    };

    public IReadOnlyList<UiFrameSnapshot> CreateWarmupFrames(VirtualViewport viewport)
    {
        var frames = new List<UiFrameSnapshot>(OptionsScene.AllSections.Count + 2)
        {
            mainMenu.CreateSnapshot(viewport).UiFrame,
            mainMenu.CreateAboutDialogSnapshot(viewport).UiFrame,
        };

        foreach (var section in OptionsScene.AllSections)
            frames.Add(options.CreateSnapshotForSection(section, viewport).UiFrame);

        return frames;
    }

    public void Update(TimeSpan elapsed)
    {
        musicController.Update(elapsed);
        mainMenu.SetNowPlaying(musicController.State);
        mainMenu.SetSpectrum(menuSpectrumBuffer, musicController.TryReadSpectrum1024(menuSpectrumBuffer));

        if (activeScene == ActiveScene.BeatmapDownloader)
        {
            var importedSet = beatmapDownloader.ConsumeImportedSetDirectory();
            if (importedSet is not null)
            {
                activeScene = ActiveScene.SongSelect;
                songSelect.Enter(importedSet);
            }
            return;
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

    public void HandleUiAction(UiAction action, VirtualViewport viewport)
    {
        PlayMenuSfx(action);

        if (HandleIndexedUiAction(action))
            return;

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

            case UiAction.MainMenuBeatmapDownloader:
                PreserveDownloaderMusic();
                activeScene = ActiveScene.BeatmapDownloader;
                beatmapDownloader.Enter();
                break;

            case UiAction.MainMenuMusicPrevious:
                musicController.Execute(MenuMusicCommand.Previous);
                mainMenu.SetNowPlaying(musicController.State);
                break;

            case UiAction.MainMenuMusicPlay:
                musicController.Execute(MenuMusicCommand.Play);
                mainMenu.SetNowPlaying(musicController.State);
                break;

            case UiAction.MainMenuMusicPause:
                musicController.Execute(MenuMusicCommand.Pause);
                mainMenu.SetNowPlaying(musicController.State);
                break;

            case UiAction.MainMenuMusicStop:
                musicController.Execute(MenuMusicCommand.Stop);
                mainMenu.SetNowPlaying(musicController.State);
                break;

            case UiAction.MainMenuMusicNext:
                musicController.Execute(MenuMusicCommand.Next);
                mainMenu.SetNowPlaying(musicController.State);
                break;

            case UiAction.OptionsBack:
                BackToMainMenu();
                break;

            case UiAction.DownloaderBack:
                textInputService.HideTextInput();
                BackToMainMenu();
                break;

            case UiAction.DownloaderSearchBox:
                beatmapDownloader.FocusSearch(viewport);
                break;

            case UiAction.DownloaderSearchSubmit:
                beatmapDownloader.SubmitSearch(beatmapDownloader.Query);
                break;

            case UiAction.DownloaderRefresh:
                beatmapDownloader.Refresh();
                break;

            case UiAction.DownloaderFilters:
                beatmapDownloader.ToggleFilters();
                break;

            case UiAction.DownloaderMirror:
                beatmapDownloader.ToggleMirrorSelector();
                break;

            case UiAction.DownloaderMirrorOsuDirect:
                beatmapDownloader.SelectMirror(BeatmapMirrorKind.OsuDirect);
                break;

            case UiAction.DownloaderMirrorCatboy:
                beatmapDownloader.SelectMirror(BeatmapMirrorKind.Catboy);
                break;

            case UiAction.DownloaderSort:
                beatmapDownloader.ToggleSortDropdown();
                break;

            case UiAction.DownloaderSortTitle:
                beatmapDownloader.SetSort(BeatmapMirrorSort.Title);
                break;

            case UiAction.DownloaderSortArtist:
                beatmapDownloader.SetSort(BeatmapMirrorSort.Artist);
                break;

            case UiAction.DownloaderSortBpm:
                beatmapDownloader.SetSort(BeatmapMirrorSort.Bpm);
                break;

            case UiAction.DownloaderSortDifficultyRating:
                beatmapDownloader.SetSort(BeatmapMirrorSort.DifficultyRating);
                break;

            case UiAction.DownloaderSortHitLength:
                beatmapDownloader.SetSort(BeatmapMirrorSort.HitLength);
                break;

            case UiAction.DownloaderSortPassCount:
                beatmapDownloader.SetSort(BeatmapMirrorSort.PassCount);
                break;

            case UiAction.DownloaderSortPlayCount:
                beatmapDownloader.SetSort(BeatmapMirrorSort.PlayCount);
                break;

            case UiAction.DownloaderSortTotalLength:
                beatmapDownloader.SetSort(BeatmapMirrorSort.TotalLength);
                break;

            case UiAction.DownloaderSortFavouriteCount:
                beatmapDownloader.SetSort(BeatmapMirrorSort.FavouriteCount);
                break;

            case UiAction.DownloaderSortLastUpdated:
                beatmapDownloader.SetSort(BeatmapMirrorSort.LastUpdated);
                break;

            case UiAction.DownloaderSortRankedDate:
                beatmapDownloader.SetSort(BeatmapMirrorSort.RankedDate);
                break;

            case UiAction.DownloaderSortSubmittedDate:
                beatmapDownloader.SetSort(BeatmapMirrorSort.SubmittedDate);
                break;

            case UiAction.DownloaderOrder:
                beatmapDownloader.ToggleOrder();
                break;

            case UiAction.DownloaderStatus:
                beatmapDownloader.ToggleStatusDropdown();
                break;

            case UiAction.DownloaderStatusAll:
                beatmapDownloader.SetStatus(null);
                break;

            case UiAction.DownloaderStatusRanked:
                beatmapDownloader.SetStatus(BeatmapRankedStatus.Ranked);
                break;

            case UiAction.DownloaderStatusApproved:
                beatmapDownloader.SetStatus(BeatmapRankedStatus.Approved);
                break;

            case UiAction.DownloaderStatusQualified:
                beatmapDownloader.SetStatus(BeatmapRankedStatus.Qualified);
                break;

            case UiAction.DownloaderStatusLoved:
                beatmapDownloader.SetStatus(BeatmapRankedStatus.Loved);
                break;

            case UiAction.DownloaderStatusPending:
                beatmapDownloader.SetStatus(BeatmapRankedStatus.Pending);
                break;

            case UiAction.DownloaderStatusWorkInProgress:
                beatmapDownloader.SetStatus(BeatmapRankedStatus.WorkInProgress);
                break;

            case UiAction.DownloaderStatusGraveyard:
                beatmapDownloader.SetStatus(BeatmapRankedStatus.Graveyard);
                break;

            case UiAction.DownloaderDetailsClose:
                beatmapDownloader.CloseDetails();
                break;

            case UiAction.DownloaderDetailsPanel:
                break;

            case UiAction.DownloaderDetailsPreview:
                beatmapDownloader.PreviewDetails();
                break;

            case UiAction.DownloaderDetailsDownload:
                beatmapDownloader.DownloadDetails(true);
                break;

            case UiAction.DownloaderDetailsDownloadNoVideo:
                beatmapDownloader.DownloadDetails(false);
                break;

            case UiAction.DownloaderDownloadCancel:
                beatmapDownloader.CancelDownload();
                break;

            case UiAction.DownloaderDownloadFirst:
            case UiAction.DownloaderDownloadFirstNoVideo:
            case UiAction.DownloaderDownload0:
            case UiAction.DownloaderDownload1:
            case UiAction.DownloaderDownload2:
            case UiAction.DownloaderDownload3:
            case UiAction.DownloaderDownload4:
            case UiAction.DownloaderDownload5:
            case UiAction.DownloaderDownload6:
            case UiAction.DownloaderDownload7:
            case UiAction.DownloaderDownloadNoVideo0:
            case UiAction.DownloaderDownloadNoVideo1:
            case UiAction.DownloaderDownloadNoVideo2:
            case UiAction.DownloaderDownloadNoVideo3:
            case UiAction.DownloaderDownloadNoVideo4:
            case UiAction.DownloaderDownloadNoVideo5:
            case UiAction.DownloaderDownloadNoVideo6:
            case UiAction.DownloaderDownloadNoVideo7:
                beatmapDownloader.DownloadVisible(BeatmapDownloaderScene.DownloadIndex(action), !BeatmapDownloaderScene.IsNoVideoAction(action));
                break;

            case UiAction.SongSelectBack:
                BackToMainMenu(MainMenuReturnTransition.SongSelectBack);
                break;

            case UiAction.SongSelectBeatmapOptions:
                songSelect.OpenBeatmapOptions();
                break;

            case UiAction.SongSelectBeatmapOptionsSearch:
                songSelect.FocusBeatmapOptionsSearch(viewport);
                break;

            case UiAction.SongSelectBeatmapOptionsFavorite:
                songSelect.ToggleBeatmapOptionsFavoriteOnly();
                break;

            case UiAction.SongSelectBeatmapOptionsAlgorithm:
                songSelect.ToggleBeatmapOptionsAlgorithm();
                break;

            case UiAction.SongSelectBeatmapOptionsSort:
                songSelect.CycleBeatmapOptionsSort();
                break;

            case UiAction.SongSelectBeatmapOptionsFolder:
                songSelect.ToggleCollectionFilterPicker();
                break;

            case UiAction.SongSelectPropertiesDismiss:
                songSelect.ClosePopups();
                break;

            case UiAction.SongSelectPropertiesPanel:
                break;

            case UiAction.SongSelectPropertiesOffsetInput:
                songSelect.FocusOffsetInput(viewport);
                break;

            case UiAction.SongSelectPropertiesOffsetMinus:
                songSelect.AdjustOffset(-1);
                break;

            case UiAction.SongSelectPropertiesOffsetPlus:
                songSelect.AdjustOffset(1);
                break;

            case UiAction.SongSelectPropertiesFavorite:
                songSelect.ToggleFavorite();
                break;

            case UiAction.SongSelectPropertiesManageCollections:
                songSelect.OpenCollections();
                break;

            case UiAction.SongSelectPropertiesDelete:
                songSelect.RequestDeleteBeatmap();
                break;

            case UiAction.SongSelectPropertiesDeleteConfirm:
                songSelect.ConfirmDeleteBeatmap();
                break;

            case UiAction.SongSelectPropertiesDeleteCancel:
                songSelect.CancelDeleteBeatmap();
                break;

            case UiAction.SongSelectCollectionsNewFolder:
                songSelect.FocusNewCollectionInput(viewport);
                break;

            case UiAction.SongSelectCollectionsClose:
                songSelect.CloseCollections();
                break;

            case UiAction.SongSelectCollectionDeleteConfirm:
                songSelect.ConfirmDeleteCollection();
                break;

            case UiAction.SongSelectCollectionDeleteCancel:
                songSelect.CancelDeleteCollection();
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
                {
                    options.HandleAction(action, viewport);
                    if (action == UiAction.OptionsToggleMusicPreview)
                        ApplyMusicPreviewSetting();
                }
                break;
        }
    }

    private bool HandleIndexedUiAction(UiAction action)
    {
        if (UiActionGroups.TryGetDownloaderCardIndex(action, out var downloaderCardIndex))
        {
            beatmapDownloader.SelectCard(downloaderCardIndex);
            return true;
        }

        if (UiActionGroups.TryGetDownloaderPreviewIndex(action, out var downloaderPreviewIndex))
        {
            beatmapDownloader.PreviewCard(downloaderPreviewIndex);
            return true;
        }

        if (UiActionGroups.TryGetDownloaderDetailsDifficultyIndex(action, out var downloaderDetailsDifficultyIndex))
        {
            beatmapDownloader.SelectDetailsDifficulty(downloaderDetailsDifficultyIndex);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectSetIndex(action, out var songSelectSetIndex))
        {
            songSelect.SelectSet(songSelectSetIndex);
            mainMenu.SetNowPlaying(musicController.State);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectDifficultyIndex(action, out var songSelectDifficultyIndex))
        {
            songSelect.SelectDifficulty(songSelectDifficultyIndex);
            mainMenu.SetNowPlaying(musicController.State);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out var songSelectCollectionToggleIndex))
        {
            songSelect.HandleCollectionPrimaryAction(songSelectCollectionToggleIndex);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out var songSelectCollectionDeleteIndex))
        {
            songSelect.RequestDeleteCollection(songSelectCollectionDeleteIndex);
            return true;
        }

        return false;
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
        else if (route == MainMenuRoute.Solo)
        {
            activeScene = ActiveScene.SongSelect;
            songSelect.Enter(musicController.State.BeatmapSetDirectory, musicController.State.BeatmapFilename);
        }
    }

    private static BeatmapLibrary CreateBeatmapLibrary(DroidDatabase database, DroidGamePathLayout pathLayout)
    {
        var repository = new BeatmapLibraryRepository(database);
        return new BeatmapLibrary(pathLayout, repository);
    }

    private void ApplyMusicPreviewSetting()
    {
        menuMusicPreviewEnabled = options.GetBoolValue("musicpreview");
        if (!menuMusicPreviewEnabled)
        {
            musicController.Execute(MenuMusicCommand.Stop);
            mainMenu.SetNowPlaying(musicController.State);
            return;
        }

        musicController.Execute(MenuMusicCommand.Play);
        if (!musicController.State.IsPlaying)
            QueueStartupPreview(beatmapLibrary);

        mainMenu.SetNowPlaying(musicController.State);
    }

    private void QueueStartupPreview(IBeatmapLibrary library)
    {
        if (!menuMusicPreviewEnabled)
            return;

        var snapshot = library.Snapshot;
        if (snapshot.Sets.Count == 0)
            snapshot = library.Load();
        if (snapshot.Sets.Count == 0)
            return;

        var startIndex = random.Next(snapshot.Sets.Count);
        for (var offset = 0; offset < snapshot.Sets.Count; offset++)
        {
            var set = snapshot.Sets[(startIndex + offset) % snapshot.Sets.Count];
            var beatmap = set.Beatmaps.FirstOrDefault(static map => !string.IsNullOrWhiteSpace(map.AudioFilename));
            if (beatmap is null)
                continue;

            var audioPath = beatmap.GetAudioPath(Services.Paths.Songs);
            if (!File.Exists(audioPath))
                continue;

            musicController.Queue(
                new MenuTrack(
                    $"beatmap:{set.Directory}/{beatmap.Filename}",
                    $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}",
                    audioPath,
                    beatmap.EffectivePreviewTime,
                    (int)Math.Clamp(beatmap.Length, 0L, int.MaxValue),
                    beatmap.MostCommonBpm,
                    set.Directory,
                    beatmap.Filename),
                true);
            return;
        }
    }

    private void PreserveDownloaderMusic()
    {
        preservedDownloaderMusicState = musicController.State;
    }

    private void RestoreDownloaderMusic()
    {
        if (!menuMusicPreviewEnabled || preservedDownloaderMusicState is not { IsPlaying: true } state)
            return;

        preservedDownloaderMusicState = null;
        if (TryQueueBeatmapPreview(state.BeatmapSetDirectory, state.BeatmapFilename, true))
            mainMenu.SetNowPlaying(musicController.State);
    }

    private bool TryQueueBeatmapPreview(string? setDirectory, string? beatmapFilename, bool play)
    {
        if (string.IsNullOrWhiteSpace(setDirectory) || string.IsNullOrWhiteSpace(beatmapFilename))
            return false;

        var snapshot = beatmapLibrary.Snapshot;
        if (snapshot.Sets.Count == 0)
            snapshot = beatmapLibrary.Load();

        var set = snapshot.Sets.FirstOrDefault(candidate => string.Equals(candidate.Directory, setDirectory, StringComparison.Ordinal));
        var beatmap = set?.Beatmaps.FirstOrDefault(candidate => string.Equals(candidate.Filename, beatmapFilename, StringComparison.Ordinal));
        if (set is null || beatmap is null)
            return false;

        var audioPath = beatmap.GetAudioPath(Services.Paths.Songs);
        if (!File.Exists(audioPath))
            return false;

        musicController.Queue(
            new MenuTrack(
                $"beatmap:{set.Directory}/{beatmap.Filename}",
                $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}",
                audioPath,
                beatmap.EffectivePreviewTime,
                (int)Math.Clamp(beatmap.Length, 0L, int.MaxValue),
                beatmap.MostCommonBpm,
                set.Directory,
                beatmap.Filename),
            play);
        return true;
    }

    private static string DisplayTitle(BeatmapInfo beatmap) => string.IsNullOrWhiteSpace(beatmap.TitleUnicode) ? beatmap.Title : beatmap.TitleUnicode;

    private static string DisplayArtist(BeatmapInfo beatmap) => string.IsNullOrWhiteSpace(beatmap.ArtistUnicode) ? beatmap.Artist : beatmap.ArtistUnicode;

    private void PlayMenuSfx(UiAction action)
    {
        var key = MenuSfxKeyFor(action);

        if (key is not null)
            attachedMenuSfxPlayer.Play(key);
    }

    private static string? MenuSfxKeyFor(UiAction action)
    {
        if (UiActionGroups.IsOptionsSection(action) ||
            UiActionGroups.IsDownloaderSortChoice(action) ||
            UiActionGroups.IsDownloaderStatusChoice(action) ||
            UiActionGroups.TryGetDownloaderDetailsDifficultyIndex(action, out _) ||
            UiActionGroups.TryGetSongSelectDifficultyIndex(action, out _) ||
            UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out _))
            return "menuclick";

        if (UiActionGroups.IsOptionsToggle(action) ||
            UiActionGroups.TryGetDownloaderCardIndex(action, out _) ||
            UiActionGroups.TryGetDownloaderPreviewIndex(action, out _) ||
            UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out _) ||
            IsBetween(action, UiAction.DownloaderDownload0, UiAction.DownloaderDownloadNoVideo7) ||
            UiActionGroups.TryGetSongSelectSetIndex(action, out _))
            return "menuhit";

        return action switch
        {
            UiAction.MainMenuCookie or UiAction.MainMenuFirst or UiAction.MainMenuSecond or UiAction.MainMenuThird => "menuhit",
            UiAction.MainMenuBeatmapDownloader or UiAction.DownloaderDetailsPanel => "menuhit",
            UiAction.DownloaderDetailsPreview or UiAction.DownloaderDetailsDownload or UiAction.DownloaderDetailsDownloadNoVideo => "menuhit",
            UiAction.DownloaderDownloadFirst or UiAction.DownloaderDownloadFirstNoVideo => "menuhit",
            UiAction.SongSelectPropertiesOffsetMinus or UiAction.SongSelectPropertiesOffsetPlus => "menuhit",
            UiAction.SongSelectPropertiesFavorite or UiAction.SongSelectPropertiesManageCollections or UiAction.SongSelectPropertiesDelete => "menuclick",
            UiAction.SongSelectPropertiesOffsetInput or UiAction.SongSelectCollectionsNewFolder => "menuclick",
            UiAction.SongSelectPropertiesDeleteConfirm or UiAction.SongSelectCollectionDeleteConfirm => "menuhit",
            UiAction.SongSelectPropertiesDeleteCancel or UiAction.SongSelectCollectionDeleteCancel => "menuback",
            UiAction.SongSelectPropertiesDismiss or UiAction.SongSelectCollectionsClose => "menuback",
            UiAction.MainMenuVersionPill or UiAction.MainMenuAboutClose or UiAction.MainMenuAboutChangelog => "menuclick",
            UiAction.MainMenuAboutOsuWebsite or UiAction.MainMenuAboutOsuDroidWebsite or UiAction.MainMenuAboutDiscord => "menuclick",
            UiAction.MainMenuMusicPrevious or UiAction.MainMenuMusicPlay or UiAction.MainMenuMusicPause => "menuclick",
            UiAction.MainMenuMusicStop or UiAction.MainMenuMusicNext or UiAction.DownloaderSearchBox => "menuclick",
            UiAction.SongSelectMods or UiAction.SongSelectBeatmapOptions or UiAction.SongSelectBeatmapOptionsSearch or UiAction.SongSelectBeatmapOptionsFavorite or UiAction.SongSelectBeatmapOptionsAlgorithm or UiAction.SongSelectBeatmapOptionsSort or UiAction.SongSelectBeatmapOptionsFolder or UiAction.SongSelectRandom => "menuclick",
            UiAction.DownloaderSearchSubmit or UiAction.DownloaderRefresh or UiAction.DownloaderMirrorOsuDirect or UiAction.DownloaderMirrorCatboy => "menuclick",
            UiAction.DownloaderFilters or UiAction.DownloaderMirror or UiAction.DownloaderSort or UiAction.DownloaderOrder or UiAction.DownloaderStatus => "menuhit",
            UiAction.OptionsBack or UiAction.DownloaderBack or UiAction.SongSelectBack => "menuback",
            UiAction.DownloaderDetailsClose or UiAction.DownloaderDownloadCancel => "menuback",
            _ => null,
        };
    }

    private static bool IsBetween(UiAction action, UiAction first, UiAction last) => action >= first && action <= last;
}
