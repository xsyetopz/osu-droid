using OsuDroid.Game;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

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

        var warmup = scene.CreateSnapshotForSection(OptionsSection.Advanced, viewport);
        var active = scene.CreateSnapshot(viewport);

        Assert.That(warmup.SelectedIndex, Is.EqualTo((int)OptionsSection.Advanced));
        Assert.That(scene.ActiveSection, Is.EqualTo(OptionsSection.Audio));
        Assert.That(scene.ContentScrollOffset, Is.GreaterThan(0f));
        Assert.That(active.SelectedIndex, Is.EqualTo((int)OptionsSection.Audio));
    }
    [Test]
    public void CoreRoutesMainMenuOptionsToOptionsSceneBackAndScrolls()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"options-core-{Guid.NewGuid():N}");
        try
        {
            var core = OsuDroidGameCore.Create(path, "debug");
            var viewport = VirtualViewport.FromSurface(1280, 720);
            var contentPoint = new UiPoint(DroidUiMetrics.ContentPaddingX + DroidUiMetrics.SectionRailWidth + DroidUiMetrics.ListGap + 10f, DroidUiMetrics.ContentTop);
            core.TapMainMenuCookie();
            core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));

            Assert.That(core.TapMainMenu(MainMenuButtonSlot.Second), Is.EqualTo(MainMenuRoute.Settings));
            Assert.That(core.CreateFrame(viewport).Scene, Is.EqualTo("Options"));

            var beforeScroll = core.CreateFrame(viewport).UiFrame.Elements.Single(element => element.Id == "options-row-0");
            core.ScrollActiveScene(160f, contentPoint, viewport);
            var afterScroll = core.CreateFrame(viewport).UiFrame.Elements.Single(element => element.Id == "options-row-0");
            Assert.That(afterScroll.Bounds.Y, Is.LessThan(beforeScroll.Bounds.Y));

            core.HandleUiAction(UiAction.OptionsBack);

            Assert.That(core.CreateFrame(viewport).Scene, Is.EqualTo("MainMenu"));
        }
        finally
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
    [Test]
    public void CoreWarmupFramesIncludeMainMenuAboutAndEveryOptionsSection()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"options-warmup-{Guid.NewGuid():N}");
        try
        {
            var core = OsuDroidGameCore.Create(path, "debug");
            var viewport = VirtualViewport.FromSurface(1280, 720);

            var frames = core.CreateWarmupFrames(viewport);
            var optionFrames = frames
                .Where(frame => frame.Elements.Any(element => element.Id == "options-root"))
                .ToArray();
            var selectedActions = optionFrames
                .Select(frame => frame.Elements.Single(element => element.Id == "options-section-selected").Action)
                .ToArray();

            Assert.That(frames.Any(frame => frame.Elements.Any(element => element.Id == "about-panel")), Is.True);
            Assert.That(optionFrames.Length, Is.EqualTo(OptionsScene.AllSections.Count));
            Assert.That(selectedActions, Does.Contain(UiAction.OptionsSectionGeneral));
            Assert.That(selectedActions, Does.Contain(UiAction.OptionsSectionAdvanced));
        }
        finally
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
    [Test]
    public void CoreWarmupFramesDoNotChangeActiveScene()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"options-warmup-scene-{Guid.NewGuid():N}");
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

            var frame = core.CreateFrame(viewport);
            Assert.That(frame.Scene, Is.EqualTo("Options"));
            Assert.That(frame.SelectedIndex, Is.EqualTo((int)OptionsSection.Audio));
        }
        finally
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }

    [Test]
    public void CoreAppliesOptionsVolumeSlidersToRuntimeAudioPlayers()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"options-volume-{Guid.NewGuid():N}");
        try
        {
            var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(path));
            paths.EnsureDirectories();
            var database = new DroidDatabase(paths.GetDatabasePath("debug"));
            database.EnsureCreated();
            var preview = new RecordingPreviewPlayer();
            var sfx = new RecordingMenuSfxPlayer();
            var core = new OsuDroidGameCore(new GameServices(
                database,
                paths,
                "debug",
                "1.0",
                BeatmapPreviewPlayer: preview,
                MenuSfxPlayer: sfx,
                SettingsStore: new JsonGameSettingsStore(Path.Combine(paths.CoreRoot, "config", "settings.json"))));
            var viewport = VirtualViewport.FromSurface(1280, 720);

            core.TapMainMenuCookie();
            core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
            _ = core.TapMainMenu(MainMenuButtonSlot.Second);
            core.HandleUiAction(UiAction.OptionsSectionAudio, viewport);
            core.HandleUiAction(UiAction.OptionsRow0, viewport);
            core.HandleUiAction(UiAction.OptionsRow1, viewport);

            Assert.That(preview.Volume, Is.EqualTo(0f));
            Assert.That(sfx.Volume, Is.EqualTo(0f));
        }
        finally
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }

    private sealed class RecordingPreviewPlayer : IBeatmapPreviewPlayer
    {
        public float Volume { get; private set; } = 1f;

        public bool IsPlaying => false;

        public int PositionMilliseconds => 0;

        public BeatmapPreviewPlaybackSnapshot PlaybackSnapshot { get; } = new();

        public void Play(string audioPath, int previewTimeMilliseconds)
        {
        }

        public void Play(Uri previewUri)
        {
        }

        public void PausePreview()
        {
        }

        public void ResumePreview()
        {
        }

        public void StopPreview()
        {
        }

        public void SetVolume(float normalizedVolume) => Volume = normalizedVolume;

        public bool TryReadSpectrum1024(float[] destination) => false;
    }

    private sealed class RecordingMenuSfxPlayer : IMenuSfxPlayer
    {
        public float Volume { get; private set; } = 1f;

        public void Play(string key)
        {
        }

        public void SetVolume(float normalizedVolume) => Volume = normalizedVolume;
    }
}
