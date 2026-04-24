using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{

    [Test]
    public void DownloaderBackButtonUsesSharedMaterialIcon()
    {
        BeatmapDownloaderScene scene = CreateScene();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot backIcon = frame.Elements.Single(element => element.Id == "downloader-back");
        UiElementSnapshot? refresh = frame.Elements.FirstOrDefault(element => element.Id == "downloader-refresh");

        Assert.That(backIcon.Kind, Is.EqualTo(UiElementKind.MaterialIcon));
        Assert.That(backIcon.MaterialIcon, Is.EqualTo(UiMaterialIcon.ArrowBack));
        Assert.That(backIcon.Action, Is.EqualTo(UiAction.DownloaderBack));
        Assert.That(refresh, Is.Null);
    }

    [Test]
    public void DownloaderSearchLoadingUsesLegacyAppBarIndicator()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSearching(scene, true);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot indicator = frame.Elements.Single(element => element.Id == "downloader-searching-indicator");
        UiElementSnapshot search = frame.Elements.Single(element => element.Id == "downloader-search");

        Assert.That(indicator.Bounds.X, Is.LessThan(search.Bounds.X));
        Assert.That(indicator.Kind, Is.EqualTo(UiElementKind.ProgressRing));
        Assert.That(indicator.ProgressRing!.SweepDegrees, Is.EqualTo(96f).Within(0.001f));
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-status"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Text?.StartsWith("Searching", StringComparison.Ordinal) == true), Is.False);
        Assert.That(frame.Elements.Any(element => element.Text == "◌"), Is.False);
    }

    [Test]
    public void FilterPanelMatchesLegacyThreeControlShape()
    {
        BeatmapDownloaderScene scene = CreateScene();
        scene.ToggleFilters();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "downloader-filter-sort-bg"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-filter-order-bg"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-filter-status-bg"), Is.True);
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-filter-status-label").Text, Is.EqualTo("Status"));
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-filter-status-bg").Bounds.Width, Is.LessThan(96f * DroidUiMetrics.DpScale));
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-filter-sort-bg").Bounds.Width, Is.LessThan(160f * DroidUiMetrics.DpScale));
        Assert.That(frame.Elements.Any(element => element.Id.StartsWith("downloader-status-option", StringComparison.Ordinal)), Is.False);
    }

    [Test]
    public void FilterDropdownUsesLegacyPlainRowsAndSelectedCheck()
    {
        BeatmapDownloaderScene scene = CreateScene();
        scene.SetSort(BeatmapMirrorSort.Title);
        scene.ToggleFilters();
        scene.ToggleSortDropdown();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "downloader-sort-option-panel"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-sort-option-0-selected"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-sort-option-0-check"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-sort-option-0-bg"), Is.False);
    }

    [Test]
    public void StatusDropdownUsesNaturalWidthForLegacyRows()
    {
        BeatmapDownloaderScene scene = CreateScene();
        scene.ToggleFilters();
        scene.ToggleStatusDropdown();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot button = frame.Elements.Single(element => element.Id == "downloader-filter-status-bg");
        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "downloader-status-option-panel");
        UiElementSnapshot ranked = frame.Elements.Single(element => element.Id == "downloader-status-option-1-text");
        UiElementSnapshot qualified = frame.Elements.Single(element => element.Id == "downloader-status-option-3-text");

        Assert.That(panel.Bounds.Width, Is.GreaterThan(button.Bounds.Width));
        Assert.That(panel.Bounds.Right, Is.LessThanOrEqualTo(VirtualViewport.LegacyLandscape.VirtualWidth + 0.001f));
        Assert.That(ranked.TextStyle!.Size, Is.EqualTo(14f * DroidUiMetrics.DpScale));
        Assert.That(qualified.Bounds.Width, Is.GreaterThanOrEqualTo(qualified.Text!.Length * qualified.TextStyle!.Size * 0.5f));
    }

    [Test]
    public void MirrorSelectorUsesLegacyCenteredDialog()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSet(BeatmapRankedStatus.Ranked)]);
        scene.ToggleMirrorSelector();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot dialog = frame.Elements.Single(element => element.Id == "downloader-mirror-dialog");
        UiElementSnapshot title = frame.Elements.Single(element => element.Id == "downloader-mirror-title");

        Assert.That(frame.Elements.Any(element => element.Id == "downloader-mirror-panel"), Is.False);
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-mirror-scrim").Color, Is.EqualTo(new UiColor(0, 0, 0, 128)));
        Assert.That(dialog.Bounds.X + dialog.Bounds.Width / 2f, Is.EqualTo(VirtualViewport.LegacyLandscape.VirtualWidth / 2f).Within(0.001f));
        Assert.That(dialog.Bounds.Y + dialog.Bounds.Height / 2f, Is.EqualTo(VirtualViewport.LegacyLandscape.VirtualHeight / 2f).Within(0.001f));
        Assert.That(title.Text, Is.EqualTo("Select a beatmap mirror"));
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-mirror-OsuDirect-url").Text, Is.EqualTo("https://osu.direct"));
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-mirror-OsuDirect-selected"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "downloader-mirror-OsuDirect-check"), Is.True);
    }

    [Test]
    public void StatusPillsUseLegacyLanguagePackColors()
    {
        AssertStatusColor(BeatmapRankedStatus.Ranked, UiColor.Opaque(65, 255, 100));
        AssertStatusColor(BeatmapRankedStatus.Approved, UiColor.Opaque(65, 255, 100));
        AssertStatusColor(BeatmapRankedStatus.Qualified, UiColor.Opaque(100, 242, 255));
        AssertStatusColor(BeatmapRankedStatus.Loved, UiColor.Opaque(250, 100, 255));
        AssertStatusColor(BeatmapRankedStatus.Pending, UiColor.Opaque(255, 172, 100));
        AssertStatusColor(BeatmapRankedStatus.WorkInProgress, UiColor.Opaque(255, 172, 100));
        AssertStatusColor(BeatmapRankedStatus.Graveyard, UiColor.Opaque(255, 255, 255));
    }
    [Test]
    public void DownloaderCardsUseColoredDifficultyDotsAndMaterialButtons()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSet()]);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;

        UiElementSnapshot[] difficulty = frame.Elements.Where(element => element.Id.StartsWith("downloader-card-0-diff-", StringComparison.Ordinal)).ToArray();
        Assert.That(difficulty, Has.Length.EqualTo(2));
        Assert.That(difficulty.All(element => element.Kind == UiElementKind.Text), Is.True);
        Assert.That(difficulty.All(element => element.Text == "⦿"), Is.True);
        Assert.That(difficulty.All(element => element.TextStyle!.Size <= 24f * DroidUiMetrics.DpScale), Is.True);
        Assert.That(difficulty.Select(element => element.Color).Distinct().Count(), Is.EqualTo(2));
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-card-0-preview-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.PlayArrow));
        Assert.That(frame.Elements.Single(element => element.Id == "downloader-card-0-download-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Download));
        Assert.That(frame.Elements.Any(element => element.Text?.Contains('⬇') == true || element.Text?.Contains('▶') == true), Is.False);
    }

    [Test]
    public void DownloaderCardDifficultyDotsStayInsideCardWhenThereAreManyDifficulties()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSetWithDifficulties(16)]);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot card = frame.Elements.Single(element => element.Id == "downloader-card-0");
        UiElementSnapshot[] dots = frame.Elements
            .Where(element => element.Id.StartsWith("downloader-card-0-diff-", StringComparison.Ordinal))
            .ToArray();

        Assert.That(dots, Has.Length.EqualTo(16));
        Assert.That(dots.Min(element => element.Bounds.X), Is.GreaterThanOrEqualTo(card.Bounds.X + 8f * DroidUiMetrics.DpScale - 0.01f));
        Assert.That(dots.Max(element => element.Bounds.Right), Is.LessThanOrEqualTo(card.Bounds.Right - 8f * DroidUiMetrics.DpScale + 0.01f));
        Assert.That(dots.All(element => element.TextStyle!.Size <= 24f * DroidUiMetrics.DpScale), Is.True);
    }

    [Test]
    public void DownloaderDetailsDifficultyDotsStayInsidePanelWhenThereAreManyDifficulties()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSetWithDifficulties(16)]);
        scene.SelectCard(0);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        UiElementSnapshot[] dots = frame.Elements
            .Where(element => element.Id.StartsWith("downloader-details-diff-", StringComparison.Ordinal) && !element.Id.EndsWith("-selected", StringComparison.Ordinal))
            .ToArray();

        Assert.That(dots, Has.Length.EqualTo(16));
        Assert.That(dots.Min(element => element.Bounds.X), Is.GreaterThanOrEqualTo(panel.Bounds.X + 12f * DroidUiMetrics.DpScale - 0.01f));
        Assert.That(dots.Max(element => element.Bounds.Right), Is.LessThanOrEqualTo(panel.Bounds.Right - 12f * DroidUiMetrics.DpScale + 0.01f));
    }

    [Test]
    public void DownloaderHeaderRendersAboveScrolledCards()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, Enumerable.Range(0, 20).Select(_ => CreateSet()).ToArray());
        VirtualViewport viewport = VirtualViewport.LegacyLandscape;

        scene.Scroll(360f * DroidUiMetrics.DpScale, viewport);

        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        int cardIndex = ElementIndex(frame, "downloader-card-0");
        int appBarIndex = ElementIndex(frame, "downloader-appbar");
        int searchIndex = ElementIndex(frame, "downloader-search");
        int backIndex = ElementIndex(frame, "downloader-back");
        UiElementSnapshot card = frame.Elements[cardIndex];

        Assert.That(card.Bounds.Y, Is.LessThan(DroidUiMetrics.AppBarHeight));
        Assert.That(cardIndex, Is.LessThan(appBarIndex));
        Assert.That(cardIndex, Is.LessThan(searchIndex));
        Assert.That(cardIndex, Is.LessThan(backIndex));
        Assert.That(frame.HitTest(new UiPoint(20f * DroidUiMetrics.DpScale, 20f * DroidUiMetrics.DpScale))!.Action, Is.EqualTo(UiAction.DownloaderBack));
    }
    [Test]
    public void StatusPillUsesAndroidPanelColorAndCenteredText()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSet(BeatmapRankedStatus.Ranked)]);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot statusBackground = frame.Elements.Single(element => element.Id == "downloader-card-0-status-bg");
        UiElementSnapshot statusText = frame.Elements.Single(element => element.Id == "downloader-card-0-status");

        Assert.That(statusBackground.Color, Is.EqualTo(UiColor.Opaque(22, 22, 34)));
        Assert.That(statusText.Color, Is.EqualTo(UiColor.Opaque(65, 255, 100)));
        Assert.That(statusText.TextStyle!.Alignment, Is.EqualTo(UiTextAlignment.Center));
        Assert.That(statusText.TextStyle.VerticalAlignment, Is.EqualTo(UiTextVerticalAlignment.Middle));
    }
    [Test]
    public void FiltersButtonUsesCenteredCompoundLayout()
    {
        BeatmapDownloaderScene scene = CreateScene();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot button = frame.Elements.Single(element => element.Id == "downloader-filters-hit");
        UiElementSnapshot icon = frame.Elements.Single(element => element.Id == "downloader-filters-icon");
        UiElementSnapshot text = frame.Elements.Single(element => element.Id == "downloader-filters-text");
        float contentLeft = icon.Bounds.X;
        float contentRight = text.Bounds.Right;
        float buttonCenter = button.Bounds.X + button.Bounds.Width / 2f;
        float contentCenter = contentLeft + (contentRight - contentLeft) / 2f;

        Assert.That(Math.Abs(buttonCenter - contentCenter), Is.LessThan(1.5f));
        Assert.That(text.TextStyle!.VerticalAlignment, Is.EqualTo(UiTextVerticalAlignment.Middle));
    }

    private static void AssertStatusColor(BeatmapRankedStatus status, UiColor expectedColor)
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSet(status)]);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot statusText = frame.Elements.Single(element => element.Id == "downloader-card-0-status");

        Assert.That(statusText.Color, Is.EqualTo(expectedColor));
    }
}
