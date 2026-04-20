using OsuDroid.Game.Beatmaps;
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
    private readonly IMenuMusicController musicController;
    private ITextInputService textInputService;
    private IBeatmapPreviewPlayer previewPlayer;
    private ActiveScene activeScene;

    public OsuDroidGameCore(GameServices services)
    {
        Services = services;
        mainMenu = new MainMenuScene(services.DisplayVersion, services.NowPlaying ?? new MenuNowPlayingState());
        options = new OptionsScene(new GameLocalizer());
        textInputService = services.TextInputService ?? new NoOpTextInputService();
        previewPlayer = services.BeatmapPreviewPlayer ?? new NoOpBeatmapPreviewPlayer();
        var library = services.BeatmapLibrary ?? CreateBeatmapLibrary(services.Database, services.Paths);
        var mirrorClient = services.BeatmapMirrorClient ?? new OsuDirectMirrorClient(new HttpClient());
        var importService = services.BeatmapImportService ?? new BeatmapImportService(services.Paths, library);
        var downloadService = services.BeatmapDownloadService ?? new BeatmapDownloadService(services.Paths, mirrorClient, importService);
        beatmapDownloader = new BeatmapDownloaderScene(mirrorClient, downloadService, textInputService, previewPlayer, Path.Combine(services.Paths.CacheRoot, "Covers"));
        songSelect = new SongSelectScene(library, previewPlayer, services.Paths.Songs);
        musicController = services.MusicController ?? new NoOpMenuMusicController();
    }

    public GameServices Services { get; }

    public MainMenuRoute LastRoute { get; private set; }

    public string? PendingExternalUrl { get; private set; }

    public MenuMusicCommand LastMusicCommand => musicController.LastCommand;


    public void AttachPlatformServices(ITextInputService? platformTextInputService, IBeatmapPreviewPlayer? platformPreviewPlayer)
    {
        if (platformTextInputService is not null)
        {
            textInputService = platformTextInputService;
            beatmapDownloader.SetTextInputService(platformTextInputService);
        }

        if (platformPreviewPlayer is not null)
        {
            previewPlayer = platformPreviewPlayer;
            songSelect.SetPreviewPlayer(platformPreviewPlayer);
            beatmapDownloader.SetPreviewPlayer(platformPreviewPlayer);
        }
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
        return new OsuDroidGameCore(new GameServices(database, pathLayout, buildType, displayVersion, BeatmapLibrary: library, BeatmapImportService: importService, BeatmapDownloadService: downloadService, BeatmapMirrorClient: mirrorClient));
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
        if (activeScene == ActiveScene.SongSelect)
            songSelect.Leave();
        else if (activeScene == ActiveScene.BeatmapDownloader)
            beatmapDownloader.Leave();

        activeScene = ActiveScene.MainMenu;

        if (transition == MainMenuReturnTransition.SongSelectBack)
            mainMenu.StartReturnTransition();
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
    }

    public void ScrollActiveScene(float deltaY, VirtualViewport viewport)
    {
        if (activeScene == ActiveScene.Options)
            options.Scroll(deltaY, viewport);
        else if (activeScene == ActiveScene.BeatmapDownloader)
            beatmapDownloader.Scroll(deltaY, viewport);
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

            case UiAction.MainMenuBeatmapDownloader:
                activeScene = ActiveScene.BeatmapDownloader;
                beatmapDownloader.Enter();
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

            case UiAction.DownloaderCard0:
            case UiAction.DownloaderCard1:
            case UiAction.DownloaderCard2:
            case UiAction.DownloaderCard3:
            case UiAction.DownloaderCard4:
            case UiAction.DownloaderCard5:
            case UiAction.DownloaderCard6:
            case UiAction.DownloaderCard7:
                beatmapDownloader.SelectCard(BeatmapDownloaderScene.CardIndex(action));
                break;

            case UiAction.DownloaderPreview0:
            case UiAction.DownloaderPreview1:
            case UiAction.DownloaderPreview2:
            case UiAction.DownloaderPreview3:
            case UiAction.DownloaderPreview4:
            case UiAction.DownloaderPreview5:
            case UiAction.DownloaderPreview6:
            case UiAction.DownloaderPreview7:
                beatmapDownloader.PreviewCard(BeatmapDownloaderScene.PreviewIndex(action));
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

            case UiAction.DownloaderDetailsDifficulty0:
            case UiAction.DownloaderDetailsDifficulty1:
            case UiAction.DownloaderDetailsDifficulty2:
            case UiAction.DownloaderDetailsDifficulty3:
            case UiAction.DownloaderDetailsDifficulty4:
            case UiAction.DownloaderDetailsDifficulty5:
            case UiAction.DownloaderDetailsDifficulty6:
            case UiAction.DownloaderDetailsDifficulty7:
            case UiAction.DownloaderDetailsDifficulty8:
            case UiAction.DownloaderDetailsDifficulty9:
            case UiAction.DownloaderDetailsDifficulty10:
            case UiAction.DownloaderDetailsDifficulty11:
            case UiAction.DownloaderDetailsDifficulty12:
            case UiAction.DownloaderDetailsDifficulty13:
            case UiAction.DownloaderDetailsDifficulty14:
            case UiAction.DownloaderDetailsDifficulty15:
                beatmapDownloader.SelectDetailsDifficulty(BeatmapDownloaderScene.DifficultyIndex(action));
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

            case UiAction.SongSelectFirstSet:
            case UiAction.SongSelectSet0:
            case UiAction.SongSelectSet1:
            case UiAction.SongSelectSet2:
            case UiAction.SongSelectSet3:
            case UiAction.SongSelectSet4:
            case UiAction.SongSelectSet5:
            case UiAction.SongSelectSet6:
            case UiAction.SongSelectSet7:
                songSelect.SelectSet(SongSelectScene.SetIndex(action));
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
        else if (route == MainMenuRoute.Solo)
        {
            activeScene = ActiveScene.SongSelect;
            songSelect.Enter();
        }
    }

    private static BeatmapLibrary CreateBeatmapLibrary(DroidDatabase database, DroidGamePathLayout pathLayout)
    {
        var repository = new BeatmapLibraryRepository(database);
        return new BeatmapLibrary(pathLayout, repository);
    }
}
