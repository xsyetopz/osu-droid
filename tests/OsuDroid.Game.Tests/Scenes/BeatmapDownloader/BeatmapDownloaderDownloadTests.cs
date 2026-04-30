using System.Reflection;
using NUnit.Framework;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes.BeatmapDownloader;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    [Test]
    public void DownloadOverlayMatchesOsuDroidBottomDialog()
    {
        var download = new ActiveDownloadService();
        BeatmapDownloaderScene scene = CreateScene(downloadService: download);

        VirtualViewport viewport = VirtualViewport.AndroidReferenceLandscape;
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot panel = frame.Elements.Single(element =>
            element.Id == "downloader-download-panel"
        );

        UiElementSnapshot spinner = frame.Elements.Single(element =>
            element.Id == "downloader-download-spinner"
        );
        UiElementSnapshot text = frame.Elements.Single(element =>
            element.Id == "downloader-download-text"
        );
        UiElementSnapshot cancelIcon = frame.Elements.Single(element =>
            element.Id == "downloader-download-cancel-icon"
        );

        Assert.That(
            panel.Bounds.X + panel.Bounds.Width / 2f,
            Is.EqualTo(viewport.VirtualWidth / 2f).Within(0.001f)
        );
        Assert.That(
            panel.Bounds.Bottom,
            Is.EqualTo(viewport.VirtualHeight - 20f * DroidUiMetrics.DpScale).Within(0.001f)
        );
        Assert.That(spinner.Kind, Is.EqualTo(UiElementKind.ProgressRing));
        Assert.That(spinner.ProgressRing!.SweepDegrees, Is.EqualTo(45f).Within(0.001f));
        Assert.That(
            text.Text,
            Is.EqualTo("Downloading beatmap 2524875 LaXal - Dam Dadi Doo...\n0.002 mb/s (13%)")
        );
        Assert.That(cancelIcon.Icon, Is.EqualTo(UiIcon.Close));
        Assert.That(
            frame
                .HitTest(
                    new UiPoint(
                        panel.Bounds.X + panel.Bounds.Width / 2f,
                        panel.Bounds.Bottom - 20f * DroidUiMetrics.DpScale
                    )
                )!
                .Action,
            Is.EqualTo(UiAction.DownloaderDownloadCancel)
        );
    }

    [Test]
    public void DownloadOverlayUsesGenericCopyWhenFilenameIsMissing()
    {
        BeatmapDownloaderScene scene = CreateScene(
            downloadService: new ActiveDownloadServiceWithoutFilename()
        );

        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.AndroidReferenceLandscape)
            .UiFrame;
        UiElementSnapshot text = frame.Elements.Single(element =>
            element.Id == "downloader-download-text"
        );

        Assert.That(text.Text, Is.EqualTo("Downloading beatmap..."));
    }

    [Test]
    public void DownloadOverlayUsesImportingCopyWhenProgressPhaseIsImporting()
    {
        BeatmapDownloaderScene scene = CreateScene(downloadService: new ImportingDownloadService());

        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.AndroidReferenceLandscape)
            .UiFrame;
        UiElementSnapshot text = frame.Elements.Single(element =>
            element.Id == "downloader-download-text"
        );

        Assert.That(text.Text, Is.EqualTo("Importing beatmap 2524875 LaXal - Dam Dadi Doo..."));
        Assert.That(
            frame.Elements.Any(element => element.Id == "downloader-download-cancel-hit"),
            Is.False
        );
        Assert.That(
            frame.Elements.Any(element => element.Id == "downloader-download-cancel-icon"),
            Is.False
        );
        Assert.That(
            frame.Elements.Any(element => element.Id == "downloader-download-cancel-text"),
            Is.False
        );
    }

    [Test]
    public void DownloadOverlayUsesConnectingCopyBeforeFirstDownloadUpdate()
    {
        BeatmapDownloaderScene scene = CreateScene(
            downloadService: new ConnectingDownloadService()
        );

        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.AndroidReferenceLandscape)
            .UiFrame;
        UiElementSnapshot text = frame.Elements.Single(element =>
            element.Id == "downloader-download-text"
        );
        UiElementSnapshot spinner = frame.Elements.Single(element =>
            element.Id == "downloader-download-spinner"
        );

        Assert.That(text.Text, Is.EqualTo("Connecting to server..."));
        Assert.That(spinner.ProgressRing!.SweepDegrees, Is.EqualTo(96f).Within(0.001f));
        Assert.That(
            frame.Elements.Any(element => element.Id == "downloader-download-cancel-hit"),
            Is.True
        );
    }

    [Test]
    public void DownloadFailureExceptionDoesNotEscapeFireAndForgetTask()
    {
        BeatmapDownloaderScene scene = CreateScene(downloadService: new ThrowingDownloadService());
        SetSets(scene, [CreateSet()]);

        scene.Download(0, true);
        Assert.That(
            scene
                .CreateSnapshot(VirtualViewport.AndroidReferenceLandscape)
                .UiFrame.Elements.Any(element => element.Text == "download exploded"),
            Is.False
        );

        scene.Update(TimeSpan.FromMilliseconds(16));

        Assert.That(
            scene
                .CreateSnapshot(VirtualViewport.AndroidReferenceLandscape)
                .UiFrame.Elements.Any(element => element.Text == "download exploded"),
            Is.True
        );
    }

    [Test]
    public void CoreCardNoVideoActionPassesFalseToDownloadService()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"downloader-action-{Guid.NewGuid():N}"
        );
        try
        {
            var downloadService = new RecordingDownloadService();
            OsuDroidGameCore core = CreateCoreForDownloaderAction(path, downloadService);
            BeatmapDownloaderScene downloader = GetCoreDownloader(core);
            SetSets(downloader, [CreateSet(hasVideo: true)]);
            SetCoreActiveScene(core, "BeatmapDownloader");

            core.HandleUiAction(UiAction.DownloaderResultDownloadWithoutVideoSlot0);
            SpinUntil(() => downloadService.CallCount == 1);

            Assert.That(downloadService.LastWithVideo, Is.False);
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
    public void CoreDetailsNoVideoActionPassesFalseToDownloadService()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"downloader-details-action-{Guid.NewGuid():N}"
        );
        try
        {
            var downloadService = new RecordingDownloadService();
            OsuDroidGameCore core = CreateCoreForDownloaderAction(path, downloadService);
            BeatmapDownloaderScene downloader = GetCoreDownloader(core);
            SetSets(downloader, [CreateSet(hasVideo: true)]);
            downloader.SelectCard(0);
            SetCoreActiveScene(core, "BeatmapDownloader");

            core.HandleUiAction(UiAction.DownloaderDetailsDownloadNoVideo);
            SpinUntil(() => downloadService.CallCount == 1);

            Assert.That(downloadService.LastWithVideo, Is.False);
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
    public void PreferNoVideoAppliesToDetailsDownloadAction()
    {
        var downloadService = new RecordingDownloadService();
        BeatmapDownloaderScene scene = CreateScene(downloadService: downloadService);
        SetSets(scene, [CreateSet(hasVideo: true)]);
        scene.SelectCard(0);
        scene.SetPreferNoVideoDownloads(true);

        scene.DownloadDetails(true);
        SpinUntil(() => downloadService.CallCount == 1);

        Assert.That(downloadService.LastWithVideo, Is.False);
    }

    [Test]
    public void StatusDropdownIsViewportConstrainedAndScrollable()
    {
        BeatmapDownloaderScene scene = CreateScene();
        scene.ToggleFilters();
        scene.ToggleStatusDropdown();

        VirtualViewport viewport = VirtualViewport.AndroidReferenceLandscape;
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot panel = frame.Elements.Single(element =>
            element.Id == "downloader-status-option-panel"
        );

        Assert.That(panel.Bounds.Bottom, Is.LessThanOrEqualTo(viewport.VirtualHeight));
        Assert.That(
            frame.Elements.Any(element => element.Id == "downloader-status-option-0-text"),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element => element.Id == "downloader-status-option-0-selected"),
            Is.True
        );

        scene.Scroll(500, viewport);
        frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(
            frame.Elements.Any(element => element.Id == "downloader-status-option-7-text"),
            Is.True
        );
    }

    [Test]
    public void CoreStaysInDownloaderAfterSuccessfulDownload()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"downloader-stay-{Guid.NewGuid():N}"
        );
        try
        {
            var pathLayout = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(path));
            pathLayout.EnsureDirectories();
            var database = new DroidDatabase(pathLayout.GetDatabasePath("test"));
            database.EnsureCreated();
            var core = new OsuDroidGameCore(
                new GameServices(
                    database,
                    pathLayout,
                    "test",
                    BeatmapDownloadService: new ImmediateSuccessDownloadService(),
                    BeatmapMirrorClient: new OsuDirectMirrorClient(
                        new HttpClient(new EmptyHandler())
                    )
                )
            );
            var downloader = (BeatmapDownloaderScene)
                typeof(OsuDroidGameCore)
                    .GetField("_beatmapDownloader", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(core)!;
            SetSets(downloader, [CreateSet()]);

            core.HandleUiAction(UiAction.MainMenuBeatmapDownloader);
            core.HandleUiAction(UiAction.DownloaderResultDownloadSlot0);
            SpinUntil(() =>
            {
                core.Update(TimeSpan.FromMilliseconds(16));
                GameFrameSnapshot frame = core.CreateFrame(
                    VirtualViewport.AndroidReferenceLandscape
                );
                return frame.Scene == "BeatmapDownloader"
                    && frame.UiFrame.Elements.Any(element => element.Text == "Beatmap downloaded");
            });

            GameFrameSnapshot current = core.CreateFrame(VirtualViewport.AndroidReferenceLandscape);
            Assert.That(current.Scene, Is.EqualTo("BeatmapDownloader"));
            Assert.That(
                current.UiFrame.Elements.Any(element => element.Id == "downloader-search"),
                Is.True
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
    public void SuccessfulDownloadPublishesLastImportedSetDirectoryNotification()
    {
        BeatmapDownloaderScene scene = CreateScene(
            downloadService: new ImmediateSuccessDownloadService()
        );
        SetSets(scene, [CreateSet()]);

        scene.Download(0, true);
        Assert.That(
            scene
                .CreateSnapshot(VirtualViewport.AndroidReferenceLandscape)
                .UiFrame.Elements.Any(element => element.Text == "Beatmap downloaded"),
            Is.False
        );
        Assert.That(scene.ConsumeLastImportedSetDirectoryNotification(), Is.Null);

        scene.Update(TimeSpan.FromMilliseconds(16));

        Assert.That(
            scene.ConsumeLastImportedSetDirectoryNotification(),
            Is.EqualTo("100 Artist - Title")
        );
        Assert.That(scene.ConsumeLastImportedSetDirectoryNotification(), Is.Null);
    }

    [Test]
    public void SuccessfulDownloadWritesCompletionBreadcrumbs()
    {
        string tracePath = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"downloader-trace-{Guid.NewGuid():N}.log"
        );
        BeatmapDownloaderScene scene = CreateScene(
            downloadService: new ImmediateSuccessDownloadService(),
            downloadTracePath: tracePath
        );
        SetSets(scene, [CreateSet()]);

        scene.Download(0, true);
        scene.Update(TimeSpan.FromMilliseconds(16));

        string trace = File.ReadAllText(tracePath);
        Assert.That(trace, Does.Contain("osu!droid downloader started"));
        Assert.That(trace, Does.Contain("osu!droid downloader queued-completion"));
        Assert.That(trace, Does.Contain("osu!droid downloader applied-success"));
    }

    private static void SpinUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            Thread.Sleep(10);
        }

        Assert.Fail("Condition was not met.");
    }

    private static OsuDroidGameCore CreateCoreForDownloaderAction(
        string path,
        IBeatmapDownloadService downloadService
    )
    {
        var pathLayout = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(path));
        pathLayout.EnsureDirectories();
        var database = new DroidDatabase(pathLayout.GetDatabasePath("test"));
        database.EnsureCreated();
        return new OsuDroidGameCore(
            new GameServices(
                database,
                pathLayout,
                "test",
                BeatmapDownloadService: downloadService,
                BeatmapMirrorClient: new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()))
            )
        );
    }

    private static BeatmapDownloaderScene GetCoreDownloader(OsuDroidGameCore core) =>
        (BeatmapDownloaderScene)
            typeof(OsuDroidGameCore)
                .GetField("_beatmapDownloader", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(core)!;

    private static void SetCoreActiveScene(OsuDroidGameCore core, string scene)
    {
        FieldInfo activeSceneField = typeof(OsuDroidGameCore).GetField(
            "_activeScene",
            BindingFlags.Instance | BindingFlags.NonPublic
        )!;
        activeSceneField.SetValue(core, Enum.Parse(activeSceneField.FieldType, scene));
    }
}
