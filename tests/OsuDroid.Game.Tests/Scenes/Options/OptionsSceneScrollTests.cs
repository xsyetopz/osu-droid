using NUnit.Framework;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Scenes.Options;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

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
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(scene.ActiveSection, Is.EqualTo(OptionsSection.Audio));
        Assert.That(scene.ContentScrollOffset, Is.Zero);
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-selected").Action, Is.EqualTo(UiAction.OptionsSectionAudio));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-3-text").Text, Is.EqualTo("Audio"));
        Assert.That(scene.ActiveCategories, Does.Contain("Offset"));
        Assert.That(scene.ActiveRows, Does.Contain("BGM volume"));
        Assert.That(scene.ActiveRows, Does.Contain("Offset Calibration"));
    }
    [Test]
    public void OptionsSceneContentScrollMovesRowsAndKeepsSectionsFixed()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var contentPoint = new UiPoint(DroidUiMetrics.ContentPaddingX + DroidUiMetrics.SectionRailWidth + DroidUiMetrics.ListGap + 10f, DroidUiMetrics.ContentTop);
        UiFrameSnapshot initial = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot initialRow = initial.Elements.Single(element => element.Id == "options-row-0");
        UiElementSnapshot initialSection = initial.Elements.Single(element => element.Id == "options-section-selected");

        scene.Scroll(180f, contentPoint, viewport);
        UiFrameSnapshot scrolled = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot? scrolledRow = scrolled.Elements.SingleOrDefault(element => element.Id == "options-row-0");
        UiElementSnapshot scrolledSection = scrolled.Elements.Single(element => element.Id == "options-section-selected");

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
        UiFrameSnapshot initial = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot initialRow = initial.Elements.Single(element => element.Id == "options-row-0");
        UiElementSnapshot initialSectionIcon = initial.Elements.Single(element => element.Id == "options-section-0-icon");

        scene.Scroll(160f, sectionPoint, viewport);
        UiFrameSnapshot scrolled = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot scrolledRow = scrolled.Elements.Single(element => element.Id == "options-row-0");
        UiElementSnapshot? scrolledSectionIcon = scrolled.Elements.SingleOrDefault(element => element.Id == "options-section-0-icon");

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

    [Test]
    public void OptionsSceneContentDragContinuesAfterRelease()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var start = new UiPoint(DroidUiMetrics.ContentPaddingX + DroidUiMetrics.SectionRailWidth + DroidUiMetrics.ListGap + 10f, 500f);

        Assert.That(scene.TryBeginScrollDrag(start, viewport), Is.True);
        Assert.That(scene.UpdateScrollDrag(new UiPoint(start.X, 420f), viewport), Is.True);
        float afterDrag = scene.ContentScrollOffset;
        scene.EndScrollDrag(new UiPoint(start.X, 400f), viewport);

        scene.Update(TimeSpan.FromSeconds(1d / 60d));

        Assert.That(scene.ContentScrollOffset, Is.GreaterThan(afterDrag));
    }
}
