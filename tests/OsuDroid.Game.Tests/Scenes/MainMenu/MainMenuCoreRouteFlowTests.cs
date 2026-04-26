using NUnit.Framework;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.Scenes.Startup;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{
    [Test]
    public void MainMenuCoreRoutesMusicAndAboutActions()
    {
        var database = new DroidDatabase(
            Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                $"main-menu-{Guid.NewGuid():N}.db"
            )
        );
        database.EnsureCreated();
        var musicController = new NoOpMenuMusicController();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                new DroidGamePathLayout(
                    DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)
                ),
                "test",
                "1.2.3",
                musicController
            )
        );

        core.HandleUiAction(UiAction.MainMenuMusicNext);
        Assert.That(core.LastMusicCommand, Is.EqualTo(MenuMusicCommand.Next));

        core.HandleUiAction(UiAction.MainMenuVersionPill);
        Assert.That(
            core.CreateFrame(VirtualViewport.FromSurface(1280, 720))
                .UiFrame.Elements.Any(element => element.Id == "about-panel"),
            Is.True
        );

        core.HandleUiAction(UiAction.MainMenuAboutOsuWebsite);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osu.ppy.sh"));

        core.HandleUiAction(UiAction.MainMenuAboutOsuDroidWebsite);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osudroid.moe"));

        core.HandleUiAction(UiAction.MainMenuAboutDiscord);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://discord.gg/nyD92cE"));

        core.HandleUiAction(UiAction.MainMenuAboutChangelog);
        Assert.That(
            core.ConsumePendingExternalUrl(),
            Is.EqualTo("https://osudroid.moe/changelog/latest")
        );
        Assert.That(core.PendingExternalUrl, Is.Null);
    }

    [Test]
    public void StartupWelcomeSoundsUseAttachedMenuSfxPlayer()
    {
        var database = new DroidDatabase(
            Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                $"main-menu-sfx-startup-{Guid.NewGuid():N}.db"
            )
        );
        database.EnsureCreated();
        var recorder = new RecordingMenuSfxPlayer();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                new DroidGamePathLayout(
                    DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)
                ),
                "test",
                "1.0",
                ShowStartupScene: true
            )
        );
        core.AttachPlatformServices(
            platformTextInputService: null,
            platformPreviewPlayer: null,
            recorder
        );

        core.Update(
            TimeSpan.FromMilliseconds(
                DroidUiTimings.StartupWelcomeDelayMilliseconds + StartupScene.WelcomeMilliseconds
            )
        );

        Assert.That(recorder.Keys, Is.EquivalentTo(new[] { "welcome", "welcome_piano" }));
    }

    [Test]
    public void StartupDefersBeatmapPreviewUntilWelcomeCompletes()
    {
        string root = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"startup-music-{Guid.NewGuid():N}"
        );
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        Directory.CreateDirectory(Path.Combine(paths.Songs, "1 Artist - Title"));
        File.WriteAllBytes(Path.Combine(paths.Songs, "1 Artist - Title", "audio.mp3"), [1]);
        var database = new DroidDatabase(Path.Combine(root, "test.db"));
        database.EnsureCreated();
        var music = new RecordingMenuMusicController();
        var library = new StartupMusicLibrary();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                paths,
                "test",
                "1.0",
                music,
                BeatmapLibrary: library,
                ShowStartupScene: true
            )
        );

        core.AttachPlatformServices(
            platformTextInputService: null,
            platformPreviewPlayer: new NoOpBeatmapPreviewPlayer()
        );

        Assert.That(music.SetPlaylistPlayFlags, Is.EqualTo(new[] { false }));
        Assert.That(music.PlayCommands, Is.Zero);

        core.Update(
            TimeSpan.FromMilliseconds(
                DroidUiTimings.StartupWelcomeDelayMilliseconds + StartupScene.WelcomeMilliseconds
            )
        );

        Assert.That(music.PlayCommands, Is.EqualTo(1));
    }

    [Test]
    public void SoloRouteShowsBeatmapProcessingBootstrapOnlyWhenBeatmapsNeedProcessing()
    {
        string root = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"beatmap-processing-route-{Guid.NewGuid():N}"
        );
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        var database = new DroidDatabase(Path.Combine(root, "test.db"));
        database.EnsureCreated();
        var processing = new FakeBeatmapProcessingService();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                paths,
                "test",
                "1.0",
                BeatmapLibrary: new StartupMusicLibrary(),
                BeatmapProcessingService: processing
            )
        );

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuFirst);
        core.HandleUiAction(UiAction.MainMenuFirst);

        GameFrameSnapshot processingFrame = core.CreateFrame(
            VirtualViewport.FromSurface(1280, 720)
        );
        Assert.That(processingFrame.Scene, Is.EqualTo("Bootstrap"));
        Assert.That(processing.StartCalls, Is.EqualTo(1));
        Assert.That(
            processingFrame.UiFrame.Elements.Any(element =>
                element.Id == "bootstrap-loading-title"
            ),
            Is.True
        );

        processing.Complete();
        core.Update(TimeSpan.Zero);

        Assert.That(
            core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).Scene,
            Is.EqualTo("SongSelect")
        );
    }

    [Test]
    public void SoloRouteOpensBeatmapDownloaderWhenLibraryIsEmpty()
    {
        string root = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"empty-library-route-{Guid.NewGuid():N}"
        );
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        var database = new DroidDatabase(Path.Combine(root, "test.db"));
        database.EnsureCreated();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                paths,
                "test",
                "1.0",
                BeatmapProcessingService: new NoPendingBeatmapProcessingService()
            )
        );

        OpenSoloRoute(core);

        Assert.That(
            core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).Scene,
            Is.EqualTo("BeatmapDownloader")
        );
    }

    [Test]
    public void SoloRouteOpensSongSelectWhenLibraryHasBeatmaps()
    {
        string root = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"nonempty-library-route-{Guid.NewGuid():N}"
        );
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        var database = new DroidDatabase(Path.Combine(root, "test.db"));
        database.EnsureCreated();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                paths,
                "test",
                "1.0",
                BeatmapLibrary: new StartupMusicLibrary(),
                BeatmapProcessingService: new NoPendingBeatmapProcessingService()
            )
        );

        OpenSoloRoute(core);

        Assert.That(
            core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).Scene,
            Is.EqualTo("SongSelect")
        );
    }

    private static void OpenSoloRoute(OsuDroidGameCore core)
    {
        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuFirst);
        core.HandleUiAction(UiAction.MainMenuFirst);
    }
}
