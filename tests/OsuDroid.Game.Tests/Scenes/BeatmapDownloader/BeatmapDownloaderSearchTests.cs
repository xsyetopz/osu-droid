using System.Net;
using System.Reflection;
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
    public void SearchFocusRequestsPlatformTextInput()
    {
        var textInput = new CapturingTextInputService();
        var scene = CreateScene(textInput);

        scene.FocusSearch(VirtualViewport.LegacyLandscape);
        textInput.ActiveRequest!.OnTextChanged("camellia");

        Assert.That(textInput.ActiveRequest, Is.Not.Null);
        Assert.That(scene.Query, Is.EqualTo("camellia"));
    }
    [Test]
    public void SearchBarAndIconHitTestFocusInput()
    {
        var scene = CreateScene();
        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var search = frame.Elements.Single(element => element.Id == "downloader-search");
        var icon = frame.Elements.Single(element => element.Id == "downloader-search-icon");

        Assert.That(frame.HitTest(new UiPoint(search.Bounds.X + 12, search.Bounds.Y + search.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderSearchBox));
        Assert.That(frame.HitTest(new UiPoint(icon.Bounds.X + icon.Bounds.Width / 2, icon.Bounds.Y + icon.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderSearchBox));
    }
    [Test]
    public void FocusedSearchShowsVisibleFeedback()
    {
        var textInput = new CapturingTextInputService();
        var scene = CreateScene(textInput);

        scene.FocusSearch(VirtualViewport.LegacyLandscape);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-search-focus"), Is.True);
    }
    [Test]
    public void SearchCancelClearsVisibleFeedback()
    {
        var textInput = new CapturingTextInputService();
        var scene = CreateScene(textInput);

        scene.FocusSearch(VirtualViewport.LegacyLandscape);
        textInput.ActiveRequest!.OnCanceled?.Invoke();

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-search-focus"), Is.False);
    }
    [Test]
    public void CoreDownloaderSearchActionRequestsPlatformTextInput()
    {
        var textInput = new CapturingTextInputService();
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"downloader-search-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var core = new OsuDroidGameCore(new GameServices(
            database,
            new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)),
            "test",
            TextInputService: textInput));

        core.HandleUiAction(UiAction.MainMenuBeatmapDownloader);
        core.HandleUiAction(UiAction.DownloaderSearchBox, VirtualViewport.LegacyLandscape);

        Assert.That(textInput.ActiveRequest, Is.Not.Null);
        Assert.That(textInput.ActiveRequest!.SurfaceBounds, Is.Not.Null);
    }
    [Test]
    public void SearchFocusPassesSurfaceBoundsToPlatformInput()
    {
        var textInput = new CapturingTextInputService();
        var scene = CreateScene(textInput);

        scene.FocusSearch(VirtualViewport.LegacyLandscape);

        Assert.That(textInput.ActiveRequest?.SurfaceBounds, Is.Not.Null);
        Assert.That(textInput.ActiveRequest!.SurfaceBounds!.Value.Width, Is.GreaterThan(200));
        Assert.That(textInput.ActiveRequest.SurfaceBounds.Value.Height, Is.GreaterThan(20));
    }
}
