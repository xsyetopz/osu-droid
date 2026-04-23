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
    public void DownloaderCardDifficultyDotsStayInsideCardWhenThereAreManyDifficulties()
    {
        var scene = CreateScene();
        SetSets(scene, [CreateSetWithDifficulties(16)]);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var card = frame.Elements.Single(element => element.Id == "downloader-card-0");
        var dots = frame.Elements
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
        var scene = CreateScene();
        SetSets(scene, [CreateSetWithDifficulties(16)]);
        scene.SelectCard(0);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        var dots = frame.Elements
            .Where(element => element.Id.StartsWith("downloader-details-diff-", StringComparison.Ordinal) && !element.Id.EndsWith("-selected", StringComparison.Ordinal))
            .ToArray();

        Assert.That(dots, Has.Length.EqualTo(16));
        Assert.That(dots.Min(element => element.Bounds.X), Is.GreaterThanOrEqualTo(panel.Bounds.X + 12f * DroidUiMetrics.DpScale - 0.01f));
        Assert.That(dots.Max(element => element.Bounds.Right), Is.LessThanOrEqualTo(panel.Bounds.Right - 12f * DroidUiMetrics.DpScale + 0.01f));
    }

    [Test]
    public void DownloaderHeaderRendersAboveScrolledCards()
    {
        var scene = CreateScene();
        SetSets(scene, Enumerable.Range(0, 20).Select(_ => CreateSet()).ToArray());
        var viewport = VirtualViewport.LegacyLandscape;

        scene.Scroll(360f * DroidUiMetrics.DpScale, viewport);

        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var cardIndex = ElementIndex(frame, "downloader-card-0");
        var appBarIndex = ElementIndex(frame, "downloader-appbar");
        var searchIndex = ElementIndex(frame, "downloader-search");
        var backIndex = ElementIndex(frame, "downloader-back");
        var card = frame.Elements[cardIndex];

        Assert.That(card.Bounds.Y, Is.LessThan(DroidUiMetrics.AppBarHeight));
        Assert.That(cardIndex, Is.LessThan(appBarIndex));
        Assert.That(cardIndex, Is.LessThan(searchIndex));
        Assert.That(cardIndex, Is.LessThan(backIndex));
        Assert.That(frame.HitTest(new UiPoint(20f * DroidUiMetrics.DpScale, 20f * DroidUiMetrics.DpScale))!.Action, Is.EqualTo(UiAction.DownloaderBack));
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
}
