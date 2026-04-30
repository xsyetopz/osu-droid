using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.Scenes.Options;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Tests;

public sealed partial class OptionsSceneTests
{
    [Test]
    public void OptionsSceneWarmupSnapshotDoesNotMutateActiveSectionOrScroll()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        scene.Scroll(160f, viewport);

        GameFrameSnapshot warmup = scene.CreateSnapshotForSection(
            OptionsSection.Advanced,
            viewport
        );
        GameFrameSnapshot active = scene.CreateSnapshot(viewport);

        Assert.That(warmup.SelectedIndex, Is.EqualTo((int)OptionsSection.Advanced));
        Assert.That(scene.ActiveSection, Is.EqualTo(OptionsSection.Audio));
        Assert.That(scene.ContentScrollOffset, Is.GreaterThan(0f));
        Assert.That(active.SelectedIndex, Is.EqualTo((int)OptionsSection.Audio));
    }

    [Test]
    public void CoreRoutesMainMenuOptionsToOptionsSceneBackAndScrolls()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"options-core-{Guid.NewGuid():N}"
        );
        try
        {
            var core = OsuDroidGameCore.Create(path, "debug");
            var viewport = VirtualViewport.FromSurface(1280, 720);
            var contentPoint = new UiPoint(
                DroidUiMetrics.ContentPaddingX
                    + DroidUiMetrics.SectionRailWidth
                    + DroidUiMetrics.ListGap
                    + 10f,
                DroidUiMetrics.ContentTop
            );
            core.TapMainMenuCookie();
            core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));

            Assert.That(
                core.TapMainMenu(MainMenuButtonSlot.Second),
                Is.EqualTo(MainMenuRoute.Settings)
            );
            Assert.That(core.CreateFrame(viewport).Scene, Is.EqualTo("Options"));

            UiElementSnapshot beforeScroll = core.CreateFrame(viewport)
                .UiFrame.Elements.Single(element => element.Id == "options-row-0");
            core.ScrollActiveScene(160f, contentPoint, viewport);
            UiElementSnapshot afterScroll = core.CreateFrame(viewport)
                .UiFrame.Elements.Single(element => element.Id == "options-row-0");
            Assert.That(afterScroll.Bounds.Y, Is.LessThan(beforeScroll.Bounds.Y));

            core.HandleUiAction(UiAction.OptionsBack);

            Assert.That(core.CreateFrame(viewport).Scene, Is.EqualTo("MainMenu"));
        }
        finally
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    [Test]
    public void CoreWarmupFramesIncludeMainMenuAboutAndEveryOptionsSection()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"options-warmup-{Guid.NewGuid():N}"
        );
        try
        {
            var core = OsuDroidGameCore.Create(path, "debug");
            var viewport = VirtualViewport.FromSurface(1280, 720);

            IReadOnlyList<UiFrameSnapshot> frames = core.CreateWarmupFrames(viewport);
            UiFrameSnapshot[] optionFrames = frames
                .Where(frame => frame.Elements.Any(element => element.Id == "options-root"))
                .ToArray();
            UiAction[] selectedActions = optionFrames
                .Select(frame =>
                    frame
                        .Elements.Single(element => element.Id == "options-section-selected")
                        .Action
                )
                .ToArray();

            Assert.That(
                frames.Any(frame => frame.Elements.Any(element => element.Id == "about-panel")),
                Is.True
            );
            Assert.That(optionFrames.Length, Is.EqualTo(OptionsScene.AllSections.Count));
            Assert.That(selectedActions, Does.Contain(UiAction.OptionsSectionGeneral));
            Assert.That(selectedActions, Does.Contain(UiAction.OptionsSectionAdvanced));
        }
        finally
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    [Test]
    public void CoreWarmupFramesDoNotChangeActiveScene()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"options-warmup-scene-{Guid.NewGuid():N}"
        );
        try
        {
            var core = OsuDroidGameCore.Create(path, "debug");
            var viewport = VirtualViewport.FromSurface(1280, 720);

            _ = core.CreateWarmupFrames(viewport);

            Assert.That(core.CreateFrame(viewport).Scene, Is.EqualTo("MainMenu"));

            core.TapMainMenuCookie();
            core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
            _ = core.TapMainMenu(MainMenuButtonSlot.Second);
            core.HandleUiAction(UiAction.OptionsSectionAudio, viewport);

            _ = core.CreateWarmupFrames(viewport);

            GameFrameSnapshot frame = core.CreateFrame(viewport);
            Assert.That(frame.Scene, Is.EqualTo("Options"));
            Assert.That(frame.SelectedIndex, Is.EqualTo((int)OptionsSection.Audio));
        }
        finally
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    [Test]
    public void CoreAppliesOptionsVolumeSlidersToRuntimeAudioPlayers()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"options-volume-{Guid.NewGuid():N}"
        );
        try
        {
            var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(path));
            paths.EnsureDirectories();
            var database = new DroidDatabase(paths.GetDatabasePath("debug"));
            database.EnsureCreated();
            var preview = new RecordingPreviewPlayer();
            var sfx = new RecordingMenuSfxPlayer();
            var core = new OsuDroidGameCore(
                new GameServices(
                    database,
                    paths,
                    "debug",
                    "1.0",
                    BeatmapPreviewPlayer: preview,
                    MenuSfxPlayer: sfx,
                    SettingsStore: new JsonGameSettingsStore(
                        Path.Combine(paths.CoreRoot, "config", "settings.json")
                    )
                )
            );
            var viewport = VirtualViewport.FromSurface(1280, 720);

            core.TapMainMenuCookie();
            core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
            _ = core.TapMainMenu(MainMenuButtonSlot.Second);
            core.HandleUiAction(UiAction.OptionsSectionAudio, viewport);
            core.HandleUiAction(UiAction.OptionsActiveRow0, viewport);
            core.HandleUiAction(UiAction.OptionsActiveRow1, viewport);

            Assert.That(preview.Volume, Is.EqualTo(0f));
            Assert.That(sfx.Volume, Is.EqualTo(0f));
        }
        finally
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    [Test]
    public void CoreBacksUpAndRestoresNonSensitiveOptions()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"options-backup-{Guid.NewGuid():N}"
        );
        try
        {
            var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(path));
            paths.EnsureDirectories();
            var database = new DroidDatabase(paths.GetDatabasePath("debug"));
            database.EnsureCreated();
            var settings = new JsonGameSettingsStore(
                Path.Combine(paths.CoreRoot, "config", "settings.json")
            );
            settings.SetInt("bgmvolume", 50);
            settings.SetString("onlinePassword", "secret");
            var preview = new RecordingPreviewPlayer();
            var core = new OsuDroidGameCore(
                new GameServices(
                    database,
                    paths,
                    "debug",
                    "1.0",
                    BeatmapPreviewPlayer: preview,
                    SettingsStore: settings
                )
            );
            var viewport = VirtualViewport.FromSurface(1280, 720);

            core.TapMainMenuCookie();
            core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
            _ = core.TapMainMenu(MainMenuButtonSlot.Second);
            core.HandleUiAction(UiAction.OptionsActiveRow8, viewport);
            core.HandleUiAction(UiAction.OptionsSectionAudio, viewport);
            core.HandleUiAction(UiAction.OptionsActiveRow0, viewport);
            core.HandleUiAction(UiAction.OptionsSectionGeneral, viewport);
            core.HandleUiAction(UiAction.OptionsActiveRow9, viewport);
            UiFrameSnapshot frame = core.CreateFrame(viewport).UiFrame;

            string backup = File.ReadAllText(Path.Combine(paths.CoreRoot, "osudroid.cfg"));
            Assert.That(backup, Does.Contain("\"bgmvolume\""));
            Assert.That(backup, Does.Not.Contain("onlinePassword"));
            Assert.That(settings.GetInt("bgmvolume", 0), Is.EqualTo(50));
            Assert.That(preview.Volume, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(
                frame.Elements.Single(element => element.Id == "options-status-message").Text,
                Is.EqualTo("Successfully imported the options file")
            );
        }
        finally
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    [Test]
    public void CoreClearsBeatmapCacheAndMapSpecificProperties()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"options-clear-{Guid.NewGuid():N}"
        );
        try
        {
            var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(path));
            paths.EnsureDirectories();
            var database = new DroidDatabase(paths.GetDatabasePath("debug"));
            database.EnsureCreated();
            var repository = new BeatmapLibraryRepository(database);
            repository.UpsertBeatmaps([CreateBeatmap()]);
            repository.UpsertBeatmapOptions(new BeatmapOptions("1 Artist - Title", true, 12));
            repository.SetDifficultyMetadata("test", 1);
            var core = OsuDroidGameCore.Create(DroidPathRoots.FromCoreRoot(path), "debug");
            var viewport = VirtualViewport.FromSurface(1280, 720);

            core.TapMainMenuCookie();
            core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
            _ = core.TapMainMenu(MainMenuButtonSlot.Second);
            core.HandleUiAction(UiAction.OptionsSectionLibrary, viewport);
            core.HandleUiAction(UiAction.OptionsActiveRow7, viewport);
            UiFrameSnapshot cacheFrame = core.CreateFrame(viewport).UiFrame;
            core.HandleUiAction(UiAction.OptionsActiveRow8, viewport);

            Assert.That(repository.LoadLibrary().Sets, Is.Empty);
            Assert.That(repository.GetDifficultyMetadata("test"), Is.Zero);
            Assert.That(
                cacheFrame.Elements.Single(element => element.Id == "options-status-message").Text,
                Is.EqualTo("Cache cleared")
            );
            Assert.That(
                repository.GetBeatmapOptions("1 Artist - Title"),
                Is.EqualTo(new BeatmapOptions("1 Artist - Title"))
            );
        }
        finally
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    private static BeatmapInfo CreateBeatmap() =>
        new(
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
            false
        );
}
