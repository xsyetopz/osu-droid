using OsuDroid.Game;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class OptionsSceneTests
{

    [Test]
    public void OptionsSceneSelectsSidebarSectionsAndResetsContentScroll()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var contentPoint = new UiPoint(DroidUiMetrics.ContentPaddingX + DroidUiMetrics.SectionRailWidth + DroidUiMetrics.ListGap + 10f, DroidUiMetrics.ContentTop);

        scene.Scroll(200f, contentPoint, viewport);
        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        var frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(scene.ActiveSection, Is.EqualTo(OptionsSection.Audio));
        Assert.That(scene.ContentScrollOffset, Is.Zero);
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-selected").Action, Is.EqualTo(UiAction.OptionsSectionAudio));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-3-text").Text, Is.EqualTo("Audio"));
        Assert.That(scene.ActiveCategories, Does.Contain("Offset"));
        Assert.That(scene.ActiveRows, Does.Contain("Music volume"));
        Assert.That(scene.ActiveRows, Does.Contain("Offset calibration"));
    }
    [Test]
    public void OptionsSceneContentScrollMovesRowsAndKeepsSectionsFixed()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var contentPoint = new UiPoint(DroidUiMetrics.ContentPaddingX + DroidUiMetrics.SectionRailWidth + DroidUiMetrics.ListGap + 10f, DroidUiMetrics.ContentTop);
        var initial = scene.CreateSnapshot(viewport).UiFrame;
        var initialRow = initial.Elements.Single(element => element.Id == "options-row-0");
        var initialSection = initial.Elements.Single(element => element.Id == "options-section-selected");

        scene.Scroll(180f, contentPoint, viewport);
        var scrolled = scene.CreateSnapshot(viewport).UiFrame;
        var scrolledRow = scrolled.Elements.SingleOrDefault(element => element.Id == "options-row-0");
        var scrolledSection = scrolled.Elements.Single(element => element.Id == "options-section-selected");

        Assert.That(scene.ContentScrollOffset, Is.EqualTo(180f));
        Assert.That(scene.SectionScrollOffset, Is.Zero);
        Assert.That(scrolledRow?.Bounds.Y ?? float.NegativeInfinity, Is.LessThan(initialRow.Bounds.Y));
        Assert.That(scrolledSection.Bounds, Is.EqualTo(initialSection.Bounds));
    }
    [Test]
    public void OptionsSceneSectionScrollMovesSidebarAndKeepsRowsFixed()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var sectionPoint = new UiPoint(DroidUiMetrics.ContentPaddingX + 10f, DroidUiMetrics.ContentTop);
        var initial = scene.CreateSnapshot(viewport).UiFrame;
        var initialRow = initial.Elements.Single(element => element.Id == "options-row-0");
        var initialSectionIcon = initial.Elements.Single(element => element.Id == "options-section-0-icon");

        scene.Scroll(160f, sectionPoint, viewport);
        var scrolled = scene.CreateSnapshot(viewport).UiFrame;
        var scrolledRow = scrolled.Elements.Single(element => element.Id == "options-row-0");
        var scrolledSectionIcon = scrolled.Elements.SingleOrDefault(element => element.Id == "options-section-0-icon");

        Assert.That(scene.SectionScrollOffset, Is.GreaterThan(0f));
        Assert.That(scene.ContentScrollOffset, Is.Zero);
        Assert.That(scrolledSectionIcon?.Bounds.Y ?? float.NegativeInfinity, Is.LessThan(initialSectionIcon.Bounds.Y));
        Assert.That(scrolledRow.Bounds, Is.EqualTo(initialRow.Bounds));
    }
    [Test]
    public void OptionsSceneScrollClampsToAvailableContent()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var contentPoint = new UiPoint(DroidUiMetrics.ContentPaddingX + DroidUiMetrics.SectionRailWidth + DroidUiMetrics.ListGap + 10f, DroidUiMetrics.ContentTop);
        var sectionPoint = new UiPoint(DroidUiMetrics.ContentPaddingX + 10f, DroidUiMetrics.ContentTop);

        scene.Scroll(10_000f, contentPoint, viewport);
        Assert.That(scene.ContentScrollOffset, Is.EqualTo(OptionsScene.MaxContentScrollOffset(viewport)));

        scene.Scroll(-10_000f, contentPoint, viewport);
        Assert.That(scene.ContentScrollOffset, Is.Zero);

        scene.Scroll(10_000f, sectionPoint, viewport);
        Assert.That(scene.SectionScrollOffset, Is.EqualTo(OptionsScene.MaxSectionScrollOffset(viewport)));

        scene.Scroll(-10_000f, sectionPoint, viewport);
        Assert.That(scene.SectionScrollOffset, Is.Zero);
    }
}
