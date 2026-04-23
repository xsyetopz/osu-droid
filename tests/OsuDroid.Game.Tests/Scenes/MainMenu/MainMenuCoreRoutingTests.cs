using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void TouchingFirstMainMenuButtonSwitchesToSecondMenu()
    {
        var scene = new MainMenuScene();

        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));

        Assert.That(scene.Tap(MainMenuButtonSlot.First), Is.EqualTo(MainMenuRoute.None));
        Assert.That(scene.Snapshot.IsSecondMenu, Is.True);
        Assert.That(scene.Snapshot.UiFrame.Elements.Single(element => element.Id == "menu-0").AssetName, Is.EqualTo(DroidAssets.Solo));
    }
    [Test]
    public void GameCorePublishesExitRouteAfterMainMenuExitAnimation()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-exit-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var music = new RecordingMenuMusicController();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.0", MusicController: music));

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuThird);

        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.None));

        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.ExitAnimationMilliseconds));

        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.Exit));
        Assert.That(music.LastCommand, Is.EqualTo(MenuMusicCommand.Stop));
    }
    [Test]
    public void MainMenuReturnTransitionFadesPreviousBackgroundLikeAndroidSongMenuBack()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.StartReturnTransition();
        var start = scene.CreateSnapshot(viewport).UiFrame;
        var startFade = start.Elements.Single(element => element.Id == "return-background-fade");

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.ReturnBackgroundFadeDurationMilliseconds / 2d));
        var midway = scene.CreateSnapshot(viewport).UiFrame;
        var midwayFade = midway.Elements.Single(element => element.Id == "return-background-fade");

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.ReturnBackgroundFadeDurationMilliseconds / 2d));
        var finished = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(startFade.Alpha, Is.EqualTo(1f));
        Assert.That(midwayFade.Alpha, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(finished.Elements.Any(element => element.Id == "return-background-fade"), Is.False);
        Assert.That(scene.IsReturnTransitionActive, Is.False);
    }
    [Test]
    public void MainMenuReturnTransitionDrawsBetweenBackgroundAndSceneShell()
    {
        var scene = new MainMenuScene();
        scene.StartReturnTransition();
        var elements = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.ToList();

        var backgroundIndex = elements.FindIndex(element => element.Id == "menu-background");
        var fadeIndex = elements.FindIndex(element => element.Id == "return-background-fade");
        var logoIndex = elements.FindIndex(element => element.Id == "logo");
        var profileIndex = elements.FindIndex(element => element.Id == "profile-panel");

        Assert.That(fadeIndex, Is.GreaterThan(backgroundIndex));
        Assert.That(fadeIndex, Is.LessThan(logoIndex));
        Assert.That(fadeIndex, Is.LessThan(profileIndex));
    }
    [Test]
    public void GameCoreCanStartSongSelectBackReturnTransition()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-return-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.0"));
        var viewport = VirtualViewport.FromSurface(1280, 720);

        core.BackToMainMenu();
        Assert.That(core.CreateFrame(viewport).UiFrame.Elements.Any(element => element.Id == "return-background-fade"), Is.False);

        core.BackToMainMenu(MainMenuReturnTransition.SongSelectBack);
        Assert.That(core.CreateFrame(viewport).UiFrame.Elements.Any(element => element.Id == "return-background-fade"), Is.True);
    }
    [Test]
    public void VersionPillOpensAboutDialogAndChangelogUrl()
    {
        var scene = new MainMenuScene("9.9");
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var versionPill = frame.Elements.Single(element => element.Id == "version-pill");
        var versionText = frame.Elements.Single(element => element.Id == "version-pill-text");

        Assert.That(versionPill.Action, Is.EqualTo(UiAction.MainMenuVersionPill));
        Assert.That(versionText.Text, Is.EqualTo("osu!droid 9.9"));
        Assert.That(versionPill.Bounds, Is.EqualTo(scene.GetVersionPillBounds(viewport)));

        scene.OpenAboutDialog();
        var about = scene.CreateSnapshot(viewport).UiFrame;

        var panel = about.Elements.Single(element => element.Id == "about-panel");
        var title = about.Elements.Single(element => element.Id == "about-title");
        var osuLink = about.Elements.Single(element => element.Id == "about-osu-link");
        var droidLink = about.Elements.Single(element => element.Id == "about-droid-link");
        var discordLink = about.Elements.Single(element => element.Id == "about-discord-link");

        Assert.That(scene.IsAboutDialogOpen, Is.True);
        Assert.That(panel.Bounds.Width, Is.EqualTo(500f));
        Assert.That(panel.CornerRadius, Is.EqualTo(14f));
        Assert.That(title.TextStyle?.Alignment, Is.EqualTo(UiTextAlignment.Center));
        Assert.That(osuLink.TextStyle?.Underline, Is.True);
        Assert.That(osuLink.Action, Is.EqualTo(UiAction.MainMenuAboutOsuWebsite));
        Assert.That(droidLink.Action, Is.EqualTo(UiAction.MainMenuAboutOsuDroidWebsite));
        Assert.That(discordLink.Action, Is.EqualTo(UiAction.MainMenuAboutDiscord));
        Assert.That(about.Elements.Single(element => element.Id == "about-changelog").Action, Is.EqualTo(UiAction.MainMenuAboutChangelog));
        Assert.That(about.Elements.Single(element => element.Id == "about-close").Action, Is.EqualTo(UiAction.MainMenuAboutClose));
    }
    [Test]
    public void MainMenuCoreRoutesMusicAndAboutActions()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var musicController = new NoOpMenuMusicController();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.2.3", musicController));

        core.HandleUiAction(UiAction.MainMenuMusicNext);
        Assert.That(core.LastMusicCommand, Is.EqualTo(MenuMusicCommand.Next));

        core.HandleUiAction(UiAction.MainMenuVersionPill);
        Assert.That(core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Any(element => element.Id == "about-panel"), Is.True);

        core.HandleUiAction(UiAction.MainMenuAboutOsuWebsite);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osu.ppy.sh"));

        core.HandleUiAction(UiAction.MainMenuAboutOsuDroidWebsite);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osudroid.moe"));

        core.HandleUiAction(UiAction.MainMenuAboutDiscord);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://discord.gg/nyD92cE"));

        core.HandleUiAction(UiAction.MainMenuAboutChangelog);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osudroid.moe/changelog/latest"));
        Assert.That(core.PendingExternalUrl, Is.Null);
    }

    [Test]
    public void StartupWelcomeSoundsUseAttachedMenuSfxPlayer()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-sfx-startup-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var recorder = new RecordingMenuSfxPlayer();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.0", ShowStartupScene: true));
        core.AttachPlatformServices(platformTextInputService: null, platformPreviewPlayer: null, recorder);

        core.Update(TimeSpan.FromMilliseconds(DroidUiTimings.StartupWelcomeDelayMilliseconds + StartupScene.WelcomeMilliseconds));

        Assert.That(recorder.Keys, Is.EquivalentTo(new[] { "welcome", "welcome_piano" }));
    }

    [Test]
    public void StartupDefersBeatmapPreviewUntilWelcomeCompletes()
    {
        var root = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"startup-music-{Guid.NewGuid():N}");
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        Directory.CreateDirectory(Path.Combine(paths.Songs, "1 Artist - Title"));
        File.WriteAllBytes(Path.Combine(paths.Songs, "1 Artist - Title", "audio.mp3"), [1]);
        var database = new DroidDatabase(Path.Combine(root, "test.db"));
        database.EnsureCreated();
        var music = new RecordingMenuMusicController();
        var library = new StartupMusicLibrary();
        var core = new OsuDroidGameCore(new GameServices(database, paths, "test", "1.0", music, BeatmapLibrary: library, ShowStartupScene: true));

        core.AttachPlatformServices(platformTextInputService: null, platformPreviewPlayer: new NoOpBeatmapPreviewPlayer());

        Assert.That(music.SetPlaylistPlayFlags, Is.EqualTo(new[] { false }));
        Assert.That(music.PlayCommands, Is.Zero);

        core.Update(TimeSpan.FromMilliseconds(DroidUiTimings.StartupWelcomeDelayMilliseconds + StartupScene.WelcomeMilliseconds));

        Assert.That(music.PlayCommands, Is.EqualTo(1));
    }

    [Test]
    public void SoloRouteShowsBeatmapProcessingBootstrapOnlyWhenBeatmapsNeedProcessing()
    {
        var root = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"beatmap-processing-route-{Guid.NewGuid():N}");
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        var database = new DroidDatabase(Path.Combine(root, "test.db"));
        database.EnsureCreated();
        var processing = new FakeBeatmapProcessingService();
        var core = new OsuDroidGameCore(new GameServices(database, paths, "test", "1.0", BeatmapProcessingService: processing));

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuFirst);
        core.HandleUiAction(UiAction.MainMenuFirst);

        var processingFrame = core.CreateFrame(VirtualViewport.FromSurface(1280, 720));
        Assert.That(processingFrame.Scene, Is.EqualTo("Bootstrap"));
        Assert.That(processing.StartCalls, Is.EqualTo(1));
        Assert.That(processingFrame.UiFrame.Elements.Any(element => element.Id == "bootstrap-loading-title"), Is.True);

        processing.Complete();
        core.Update(TimeSpan.Zero);

        Assert.That(core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).Scene, Is.EqualTo("SongSelect"));
    }

    [Test]
    public void MainMenuThirdButtonPlaysSeeyaOnFirstMenu()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-sfx-seeya-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var recorder = new RecordingMenuSfxPlayer();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.0"));
        core.AttachPlatformServices(platformTextInputService: null, platformPreviewPlayer: null, recorder);

        core.HandleUiAction(UiAction.MainMenuThird);

        Assert.That(recorder.Keys, Is.EqualTo(new[] { "seeya" }));
    }

    [Test]
    public void SceneStackKeepsRootScene()
    {
        var stack = new UiSceneStack("MainMenu");

        Assert.That(stack.TryPop(), Is.False);
        stack.Push("SongSelect");
        Assert.That(stack.Current, Is.EqualTo("SongSelect"));
        Assert.That(stack.TryPop(), Is.True);
        Assert.That(stack.Current, Is.EqualTo("MainMenu"));
    }

    private sealed class RecordingMenuSfxPlayer : IMenuSfxPlayer
    {
        public List<string> Keys { get; } = [];

        public void Play(string key) => Keys.Add(key);

        public void SetVolume(float normalizedVolume)
        {
        }
    }

    private sealed class FakeBeatmapProcessingService : IBeatmapProcessingService
    {
        public BeatmapProcessingState State { get; private set; } = new(true, 35, "Processing beatmaps...");

        public int StartCalls { get; private set; }

        private BeatmapLibrarySnapshot? completedSnapshot;

        public bool HasPendingWork() => completedSnapshot is null;

        public void Start() => StartCalls++;

        public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
        {
            if (completedSnapshot is null)
            {
                snapshot = BeatmapLibrarySnapshot.Empty;
                return false;
            }

            snapshot = completedSnapshot;
            completedSnapshot = null;
            State = new BeatmapProcessingState();
            return true;
        }

        public void Complete()
        {
            completedSnapshot = BeatmapLibrarySnapshot.Empty;
            State = new BeatmapProcessingState(false, 100, "Processing beatmaps...");
        }
    }

    private sealed class RecordingMenuMusicController : IMenuMusicController
    {
        public List<bool> SetPlaylistPlayFlags { get; } = [];

        public int PlayCommands { get; private set; }

        public MenuMusicCommand LastCommand { get; private set; }

        public MenuNowPlayingState State { get; private set; } = new();

        public void SetPreviewPlayer(IBeatmapPreviewPlayer player)
        {
        }

        public void Queue(MenuTrack track, bool play) => State = new MenuNowPlayingState(track.DisplayTitle, play);

        public void SetPlaylist(IReadOnlyList<MenuTrack> tracks, int startIndex, bool play)
        {
            SetPlaylistPlayFlags.Add(play);
            if (tracks.Count > 0)
                State = new MenuNowPlayingState(tracks[Math.Clamp(startIndex, 0, tracks.Count - 1)].DisplayTitle, play);
        }

        public void Execute(MenuMusicCommand command)
        {
            LastCommand = command;
            if (command == MenuMusicCommand.Play)
                PlayCommands++;
        }

        public void Update(TimeSpan elapsed)
        {
        }

        public bool TryReadSpectrum1024(float[] destination) => false;
    }

    private sealed class StartupMusicLibrary : IBeatmapLibrary
    {
        private readonly BeatmapLibrarySnapshot snapshot = new([
            new BeatmapSetInfo(1, "1 Artist - Title", [
                new BeatmapInfo(
                    "Easy.osu",
                    "1 Artist - Title",
                    "md5",
                    null,
                    "audio.mp3",
                    null,
                    null,
                    1,
                    "Title",
                    string.Empty,
                    "Artist",
                    string.Empty,
                    "Mapper",
                    "Easy",
                    string.Empty,
                    string.Empty,
                    0,
                    5,
                    5,
                    5,
                    5,
                    1,
                    1,
                    120,
                    120,
                    120,
                    1000,
                    0,
                    1,
                    0,
                    0,
                    1,
                    false)
            ])
        ]);

        public BeatmapLibrarySnapshot Snapshot => snapshot;

        public BeatmapLibrarySnapshot Load() => snapshot;

        public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null) => snapshot;

        public bool NeedsScanRefresh() => false;

        public BeatmapOptions GetOptions(string setDirectory) => new(setDirectory);

        public void SaveOptions(BeatmapOptions options)
        {
        }

        public IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null) => [];

        public IReadOnlySet<string> GetCollectionSetDirectories(string name) => new HashSet<string>(StringComparer.Ordinal);

        public bool CreateCollection(string name) => true;

        public void DeleteCollection(string name)
        {
        }

        public void ToggleCollectionMembership(string name, string setDirectory)
        {
        }

        public void DeleteBeatmapSet(string directory)
        {
        }
    }
}
