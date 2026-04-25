using NUnit.Framework;
using OsuDroid.Game.Scenes.ModSelect;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game.Tests;

public sealed partial class ModSelectSceneTests
{
    [Test]
    public void SnapshotUsesOsuDroidSectionsAndControls()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "modselect-back-text" && element.Text == "Back"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-customize" && !element.IsEnabled), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-clear-text" && element.Text == "Clear"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-search-text" && element.Text == "Search..."), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-presets" && element.Text == "Presets"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-OsuDroidLanguagePack_mod_section_difficulty_reduction" && element.Text == "Difficulty Reduction"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-OsuDroidLanguagePack_mod_section_difficulty_automation" && element.Text == "Automation"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-NF"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-icon-NF" && element.AssetName == DroidAssets.ModNoFail), Is.True);
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-score-value").Text, Is.EqualTo("1.00x"));
    }

    [Test]
    public void ToggleAddsSelectedChipAndUpdatesScoreMultiplier()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(2);
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(scene.SelectedAcronyms, Does.Contain("NF"));
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-selected-NF" && element.AssetName == DroidAssets.ModNoFail), Is.True);
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-score-value").Text, Is.EqualTo("0.50x"));
    }

    [Test]
    public void ClearRemovesSelectedMods()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(0);
        scene.Clear();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(scene.SelectedAcronyms, Is.Empty);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-selected-NF-text"), Is.False);
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-score-value").Text, Is.EqualTo("1.00x"));
    }

    [Test]
    public void SearchFiltersByAcronymAndName()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.SetSearchTerm("muted");
        scene.Update(TimeSpan.FromMilliseconds(200));
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-MU"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-NF"), Is.False);
    }

    [Test]
    public void SelectedModsPersistThroughSettingsStore()
    {
        var settings = new MemorySettingsStore();
        var firstScene = new ModSelectScene(settings, new NoOpTextInputService());

        firstScene.ToggleMod(2);
        var secondScene = new ModSelectScene(settings, new NoOpTextInputService());

        Assert.That(secondScene.SelectedAcronyms, Does.Contain("NF"));
    }


    [Test]
    public void LayoutKeepsOsuDroidSectionWidthAndDoesNotShrinkRows()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-section-presets").Bounds.Width, Is.EqualTo(300f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-section-OsuDroidLanguagePack_mod_section_difficulty_reduction").Bounds.Width, Is.EqualTo(340f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Height, Is.EqualTo(82f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-description-NF").TextStyle!.Size, Is.EqualTo(16f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-section-presets").Alpha, Is.EqualTo(0.9f));
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-OsuDroidLanguagePack_mod_section_fun"), Is.False);
    }

    [Test]
    public void DefaultLayoutUsesAndroidModOrder()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-EZ").Bounds.Y, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-toggle-HT").Bounds.Y));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-HT").Bounds.Y, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Y));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Y, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-toggle-RE").Bounds.Y));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-DT").Bounds.Y, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-toggle-FL").Bounds.Y));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-FL").Bounds.Y, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-toggle-HR").Bounds.Y));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-HR").Bounds.Y, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-toggle-HD").Bounds.Y));
    }

    [Test]
    public void HorizontalRailScrollRevealsOffscreenSections()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Scroll(700f, 0f, new UiPoint(640f, 200f), viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-OsuDroidLanguagePack_mod_section_fun"), Is.True);
    }

    [Test]
    public void VerticalSectionScrollKeepsOverflowAwayFromBottomBadges()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        UiFrameSnapshot initial = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(initial.Elements.Any(element => element.Id == "modselect-toggle-SC"), Is.False);

        scene.Scroll(0f, 500f, new UiPoint(780f, 200f), viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot smallCircle = frame.Elements.Single(element => element.Id == "modselect-toggle-SC");
        Assert.That(smallCircle.Bounds.Bottom, Is.LessThanOrEqualTo(636f));
        Assert.That(smallCircle.ClipBounds, Is.Not.Null);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-HR"), Is.False);
    }

    [Test]
    public void SectionScrollRendersPartiallyVisibleRowsWithClipBounds()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Scroll(0f, 470f, new UiPoint(780f, 200f), viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot nightcore = frame.Elements.Single(element => element.Id == "modselect-toggle-NC");
        Assert.That(nightcore.Bounds.Y, Is.LessThan(138f));
        Assert.That(nightcore.ClipBounds, Is.EqualTo(new UiRect(732f, 138f, 340f, 502f)));
    }

    [Test]
    public void DescriptionTextClipsAndAutoScrolls()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.Update(TimeSpan.FromSeconds(4));
        UiElementSnapshot description = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Single(element => element.Id == "modselect-toggle-description-EZ" && element.Kind == UiElementKind.Text);

        Assert.That(description.ClipToBounds, Is.True);
        Assert.That(description.TextStyle!.AutoScroll, Is.Not.Null);
        Assert.That(description.TextStyle.AutoScroll!.ElapsedSeconds, Is.GreaterThan(3d));
    }


}
