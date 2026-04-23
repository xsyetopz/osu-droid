using System.Net;
using System.Reflection;
using OsuDroid.Game;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{

    [Test]
    public void DownloadOverlayIsCenteredModal()
    {
        var download = new ActiveDownloadService();
        var scene = CreateScene(downloadService: download);

        var viewport = VirtualViewport.LegacyLandscape;
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "downloader-download-panel");

        Assert.That(panel.Bounds.X + panel.Bounds.Width / 2f, Is.EqualTo(viewport.VirtualWidth / 2f).Within(0.001f));
        Assert.That(panel.Bounds.Y + panel.Bounds.Height / 2f, Is.EqualTo(viewport.VirtualHeight / 2f).Within(0.001f));
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-download-track"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-download-progress"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-download-spinner"), Is.True);
        Assert.That(frame.HitTest(new UiPoint(panel.Bounds.X + panel.Bounds.Width / 2f, panel.Bounds.Bottom - 20f * DroidUiMetrics.DpScale))!.Action, Is.EqualTo(UiAction.DownloaderDownloadCancel));
    }

    [Test]
    public void DownloadOverlayUsesImportingCopyWhenProgressPhaseIsImporting()
    {
        var scene = CreateScene(downloadService: new ImportingDownloadService());

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var text = frame.Elements.Single(element => element.Id == "downloader-download-text");

        Assert.That(text.Text, Is.EqualTo("Importing 2524875 LaXal - Dam Dadi Doo"));
    }

    [Test]
    public void StatusDropdownIsViewportConstrainedAndScrollable()
    {
        var scene = CreateScene();
        scene.ToggleFilters();
        scene.ToggleStatusDropdown();

        var viewport = VirtualViewport.LegacyLandscape;
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "downloader-status-option-panel");

        Assert.That(panel.Bounds.Bottom, Is.LessThanOrEqualTo(viewport.VirtualHeight));
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-status-option-0-bg"), Is.True);

        scene.Scroll(500, viewport);
        frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "downloader-status-option-7-bg"), Is.True);
    }

    [Test]
    public void CoreStaysInDownloaderAfterSuccessfulDownload()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"downloader-stay-{Guid.NewGuid():N}");
        try
        {
            var pathLayout = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(path));
            pathLayout.EnsureDirectories();
            var database = new DroidDatabase(pathLayout.GetDatabasePath("test"));
            database.EnsureCreated();
            var core = new OsuDroidGameCore(new GameServices(
                database,
                pathLayout,
                "test",
                BeatmapDownloadService: new ImmediateSuccessDownloadService(),
                BeatmapMirrorClient: new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()))));
            var downloader = (BeatmapDownloaderScene)typeof(OsuDroidGameCore)
                .GetField("beatmapDownloader", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(core)!;
            SetSets(downloader, [CreateSet()]);

            core.HandleUiAction(UiAction.MainMenuBeatmapDownloader);
            core.HandleUiAction(UiAction.DownloaderDownload0);
            SpinUntil(() =>
            {
                core.Update(TimeSpan.FromMilliseconds(16));
                var frame = core.CreateFrame(VirtualViewport.LegacyLandscape);
                return frame.Scene == "BeatmapDownloader" &&
                    frame.UiFrame.Elements.Any(element => element.Text == "Beatmap downloaded");
            });

            var current = core.CreateFrame(VirtualViewport.LegacyLandscape);
            Assert.That(current.Scene, Is.EqualTo("BeatmapDownloader"));
            Assert.That(current.UiFrame.Elements.Any(element => element.Id == "downloader-search"), Is.True);
        }
        finally
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }

    [Test]
    public void SuccessfulDownloadPublishesLastImportedSetDirectoryNotification()
    {
        var scene = CreateScene(downloadService: new ImmediateSuccessDownloadService());
        SetSets(scene, [CreateSet()]);

        scene.Download(0, true);
        SpinUntil(() => scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame.Elements.Any(element => element.Text == "Beatmap downloaded"));

        Assert.That(scene.ConsumeLastImportedSetDirectoryNotification(), Is.EqualTo("100 Artist - Title"));
        Assert.That(scene.ConsumeLastImportedSetDirectoryNotification(), Is.Null);
    }

    private static void SpinUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return;

            Thread.Sleep(10);
        }

        Assert.Fail("Condition was not met.");
    }
}
