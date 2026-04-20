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

[TestFixture]
public sealed class BeatmapDownloaderTests
{
    [Test]
    public void OsuDirectSearchUrlMatchesLegacyEndpoint()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()));
        var uri = client.CreateSearchUri(new BeatmapMirrorSearchRequest("camellia", Offset: 50, Limit: 25));

        Assert.That(uri.GetLeftPart(UriPartial.Path), Is.EqualTo("https://osu.direct/api/v2/search"));
        Assert.That(uri.Query, Does.Contain("sort=ranked_date%3Adesc"));
        Assert.That(uri.Query, Does.Contain("mode=0"));
        Assert.That(uri.Query, Does.Contain("query=camellia"));
        Assert.That(uri.Query, Does.Contain("offset=50"));
        Assert.That(uri.Query, Does.Contain("amount=25"));
    }

    [Test]
    public void OsuDirectDownloadUrlSupportsNoVideo()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()));

        Assert.That(client.CreateDownloadUri(123, true).ToString(), Is.EqualTo("https://osu.direct/api/d/123"));
        Assert.That(client.CreateDownloadUri(123, false).ToString(), Is.EqualTo("https://osu.direct/api/d/123?noVideo=1"));
        Assert.That(client.CreatePreviewUri(BeatmapMirrorKind.OsuDirect, 456).ToString(), Is.EqualTo("https://osu.direct/api/media/preview/456"));
    }

    [Test]
    public void CatboyUrlsMatchLegacyEndpoints()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()));
        var uri = client.CreateSearchUri(new BeatmapMirrorSearchRequest("camellia", Offset: 50, Limit: 25, Mirror: BeatmapMirrorKind.Catboy, Status: BeatmapRankedStatus.Ranked));

        Assert.That(uri.GetLeftPart(UriPartial.Path), Is.EqualTo("https://catboy.best/api/v2/search"));
        Assert.That(uri.Query, Does.Contain("sort=ranked_date%3Adesc"));
        Assert.That(uri.Query, Does.Contain("mode=0"));
        Assert.That(uri.Query, Does.Contain("query=camellia"));
        Assert.That(uri.Query, Does.Contain("offset=50"));
        Assert.That(uri.Query, Does.Contain("limit=25"));
        Assert.That(uri.Query, Does.Contain("status=1"));
        Assert.That(client.CreateDownloadUri(BeatmapMirrorKind.Catboy, 123, false).ToString(), Is.EqualTo("https://catboy.best/d/123"));
        Assert.That(client.CreatePreviewUri(BeatmapMirrorKind.Catboy, 456).ToString(), Is.EqualTo("https://catboy.best/preview/audio/456"));
    }

    [Test]
    public async Task SearchParserReadsMirrorMetadata()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new JsonHandler("""
            [
              {
                "id": 123,
                "title": "Title",
                "title_unicode": "タイトル",
                "artist": "Artist",
                "artist_unicode": "アーティスト",
                "ranked": 4,
                "creator": "Mapper",
                "video": true,
                "covers": { "card": "https://example.test/card.jpg" },
                "beatmaps": [
                  { "id": 456, "version": "Hard", "difficulty_rating": 4.2, "ar": 9, "cs": 4, "drain": 5, "accuracy": 8, "bpm": 180, "hit_length": 95, "count_circles": 100, "count_sliders": 50, "count_spinners": 1 }
                ]
              }
            ]
            """)));

        var sets = await client.SearchAsync(new BeatmapMirrorSearchRequest("title"), CancellationToken.None).ConfigureAwait(false);

        Assert.That(sets, Has.Count.EqualTo(1));
        Assert.That(sets[0].Status, Is.EqualTo(BeatmapRankedStatus.Loved));
        Assert.That(sets[0].DisplayTitle, Is.EqualTo("タイトル"));
        Assert.That(sets[0].Beatmaps[0].CircleCount, Is.EqualTo(100));
        Assert.That(sets[0].Beatmaps[0].SliderCount, Is.EqualTo(50));
        Assert.That(sets[0].Beatmaps[0].SpinnerCount, Is.EqualTo(1));
    }

    [Test]
    public void DownloaderBackButtonUsesSharedMaterialIcon()
    {
        var scene = CreateScene();

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var backIcon = frame.Elements.Single(element => element.Id == "downloader-back");
        var refresh = frame.Elements.FirstOrDefault(element => element.Id == "downloader-refresh");

        Assert.That(backIcon.Kind, Is.EqualTo(UiElementKind.MaterialIcon));
        Assert.That(backIcon.MaterialIcon, Is.EqualTo(UiMaterialIcon.ArrowBack));
        Assert.That(backIcon.Action, Is.EqualTo(UiAction.DownloaderBack));
        Assert.That(refresh, Is.Null);
    }

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
    public void FilterPanelMatchesLegacyThreeControlShape()
    {
        var scene = CreateScene();
        scene.ToggleFilters();

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "downloader-filter-sort-bg"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-filter-order-bg"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-filter-status-bg"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id.StartsWith("downloader-status-option", StringComparison.Ordinal)), Is.False);
    }

    [Test]
    public void DownloaderCardsUseColoredDifficultyDotsAndMaterialButtons()
    {
        var scene = CreateScene();
        SetSets(scene, [CreateSet()]);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;

        var difficulty = frame.Elements.Where(element => element.Id.StartsWith("downloader-card-0-diff-", StringComparison.Ordinal)).ToArray();
        Assert.That(difficulty, Has.Length.EqualTo(2));
        Assert.That(difficulty.All(element => element.Kind == UiElementKind.Text), Is.True);
        Assert.That(difficulty.All(element => element.Text == "⦿"), Is.True);
        Assert.That(difficulty.All(element => element.TextStyle!.Size == 24f * DroidUiMetrics.DpScale), Is.True);
        Assert.That(difficulty.All(element => element.TextStyle!.Alignment == UiTextAlignment.Center), Is.True);
        Assert.That(difficulty.All(element => element.TextStyle!.VerticalAlignment == UiTextVerticalAlignment.Middle), Is.True);
        Assert.That(difficulty.Select(element => element.Color).Distinct().Count(), Is.EqualTo(2));
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-card-0-preview-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.PlayArrow));
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-card-0-download-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Download));
        Assert.That(frame.Elements.Any(element => element.Text?.Contains('⬇') == true || element.Text?.Contains('▶') == true), Is.False);
    }



    [Test]
    public void StatusPillUsesAndroidPanelColorAndCenteredText()
    {
        var scene = CreateScene();
        SetSets(scene, [CreateSet()]);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var statusBackground = frame.Elements.Single(element => element.Id == "downloader-card-0-status-bg");
        var statusText = frame.Elements.Single(element => element.Id == "downloader-card-0-status");

        Assert.That(statusBackground.Color, Is.EqualTo(UiColor.Opaque(22, 22, 34)));
        Assert.That(statusText.TextStyle!.Alignment, Is.EqualTo(UiTextAlignment.Center));
        Assert.That(statusText.TextStyle.VerticalAlignment, Is.EqualTo(UiTextVerticalAlignment.Middle));
    }

    [Test]
    public void FiltersButtonUsesCenteredCompoundLayout()
    {
        var scene = CreateScene();

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var button = frame.Elements.Single(element => element.Id == "downloader-filters-hit");
        var icon = frame.Elements.Single(element => element.Id == "downloader-filters-icon");
        var text = frame.Elements.Single(element => element.Id == "downloader-filters-text");
        var contentLeft = icon.Bounds.X;
        var contentRight = text.Bounds.Right;
        var buttonCenter = button.Bounds.X + button.Bounds.Width / 2f;
        var contentCenter = contentLeft + (contentRight - contentLeft) / 2f;

        Assert.That(Math.Abs(buttonCenter - contentCenter), Is.LessThan(1.5f));
        Assert.That(text.TextStyle!.VerticalAlignment, Is.EqualTo(UiTextVerticalAlignment.Middle));
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

    [Test]
    public void DetailsPanelBlocksShadeCloseAndDifficultyDotsStayInteractive()
    {
        var scene = CreateScene();
        SetSets(scene, [CreateSet()]);
        scene.SelectCard(0);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        var selectedDifficulty = frame.Elements.Single(element => element.Id == "downloader-details-diff-0-selected");
        var selectedGlyph = frame.Elements.Single(element => element.Id == "downloader-details-diff-0");

        Assert.That(frame.HitTest(new UiPoint(panel.Bounds.X + 24, panel.Bounds.Y + 24))!.Action, Is.EqualTo(UiAction.DownloaderDetailsPanel));
        Assert.That(selectedDifficulty.Bounds.Width, Is.EqualTo(56f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(frame.HitTest(new UiPoint(selectedDifficulty.Bounds.X + selectedDifficulty.Bounds.Width / 2, selectedDifficulty.Bounds.Y + selectedDifficulty.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderDetailsDifficulty0));
        Assert.That(selectedGlyph.Text, Is.EqualTo("⦿"));
        Assert.That(selectedGlyph.Kind, Is.EqualTo(UiElementKind.Text));
        Assert.That(frame.HitTest(new UiPoint(selectedGlyph.Bounds.X + selectedGlyph.Bounds.Width / 2, selectedGlyph.Bounds.Y + selectedGlyph.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderDetailsDifficulty0));
    }

    [Test]
    public void DetailsDifficultyRowIsCenteredAndUsesAndroidPaddedSelectionCell()
    {
        var scene = CreateScene();
        SetSets(scene, [CreateSet()]);
        scene.SelectCard(0);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        var dots = frame.Elements.Where(element => element.Id.StartsWith("downloader-details-diff-", StringComparison.Ordinal) && !element.Id.EndsWith("-selected", StringComparison.Ordinal)).ToArray();
        var selected = frame.Elements.Single(element => element.Id == "downloader-details-diff-0-selected");
        var left = dots.Min(element => element.Bounds.X);
        var right = dots.Max(element => element.Bounds.Right);

        Assert.That(selected.Bounds.Width, Is.EqualTo(56f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(left + (right - left) / 2f, Is.EqualTo(panel.Bounds.X + panel.Bounds.Width / 2f).Within(1f));
    }

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

    private sealed class EmptyHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });
    }

    private sealed class JsonHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
    }

    private static BeatmapDownloaderScene CreateScene(ITextInputService? textInput = null, IBeatmapDownloadService? downloadService = null) => new(
        new OsuDirectMirrorClient(new HttpClient(new EmptyHandler())),
        downloadService ?? new NoOpDownloadService(),
        textInput ?? new NoOpTextInputService(),
        new NoOpBeatmapPreviewPlayer(),
        Path.Combine(Path.GetTempPath(), "osudroid-tests", Guid.NewGuid().ToString("N")));

    private static void SetSets(BeatmapDownloaderScene scene, IReadOnlyList<BeatmapMirrorSet> sets)
    {
        typeof(BeatmapDownloaderScene).GetField("sets", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(scene, sets);
    }

    private static BeatmapMirrorSet CreateSet() => new(
        BeatmapMirrorKind.OsuDirect,
        100,
        "Title",
        "Title",
        "Artist",
        "Artist",
        BeatmapRankedStatus.Qualified,
        "Mapper",
        null,
        false,
        [
            new BeatmapMirrorBeatmap(1, "Normal", 2.4f, 5, 4, 5, 5, 120, 90, 10, 20, 0),
            new BeatmapMirrorBeatmap(2, "Hard", 5.1f, 8, 4, 6, 8, 180, 120, 50, 40, 1),
        ]);

    private sealed class CapturingTextInputService : ITextInputService
    {
        public TextInputRequest? ActiveRequest { get; private set; }
        public int HideCount { get; private set; }

        public void RequestTextInput(TextInputRequest request) => ActiveRequest = request;

        public void HideTextInput()
        {
            HideCount++;
        }
    }

    private sealed class NoOpDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new();

        public Task<BeatmapImportResult> DownloadAndImportAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken) =>
            Task.FromResult(BeatmapImportResult.Failed("Not used."));

        public void CancelActiveDownload()
        {
        }
    }

    private sealed class ActiveDownloadService : IBeatmapDownloadService
    {
        public BeatmapDownloadState State { get; } = new(
            2524875,
            "2524875 LaXal - Dam Dadi Doo",
            new BeatmapDownloadProgress(128, 1024, "Downloading", 2048),
            IsActive: true);

        public Task<BeatmapImportResult> DownloadAndImportAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken) =>
            Task.FromResult(BeatmapImportResult.Failed("Not used."));

        public void CancelActiveDownload()
        {
        }
    }
}
