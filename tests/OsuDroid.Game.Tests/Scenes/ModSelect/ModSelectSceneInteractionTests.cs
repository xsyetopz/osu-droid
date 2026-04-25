using NUnit.Framework;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes.ModSelect;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Tests;

public sealed partial class ModSelectSceneTests
{
    [Test]
    public void SearchDebouncesAndUsesContiguousNameMatch()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.SetSearchTerm("hrd");
        UiFrameSnapshot beforeDebounce = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        scene.Update(TimeSpan.FromMilliseconds(200));
        UiFrameSnapshot afterDebounce = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(beforeDebounce.Elements.Any(element => element.Id == "modselect-toggle-NF"), Is.True);
        Assert.That(afterDebounce.Elements.Any(element => element.Id == "modselect-toggle-HR"), Is.True);
        Assert.That(afterDebounce.Elements.Any(element => element.Id == "modselect-toggle-NF"), Is.False);
    }

    [Test]
    public void SelectedModsIndicatorScrollsPastEightMods()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        for (int index = 0; index < ModCatalog.Entries.Count; index++)
        {
            scene.ToggleMod(index);
        }

        UiFrameSnapshot initial = scene.CreateSnapshot(viewport).UiFrame;
        scene.Scroll(800f, 0f, new UiPoint(540f, 40f), viewport);
        UiFrameSnapshot scrolled = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(initial.Elements.Any(element => element.Id == "modselect-selected-SY"), Is.False);
        Assert.That(scrolled.Elements.Any(element => element.Id == "modselect-selected-SY"), Is.True);
    }

    [Test]
    public void CustomizeEnablesWhenSelectedModHasSettings()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(17);
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-customize").IsEnabled, Is.True);
    }

    [Test]
    public void SelectedModUsesLightCardWithDarkText()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(4);
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-DT").Color, Is.EqualTo(DroidUiTheme.ModMenu.Accent));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-name-DT").Color, Is.EqualTo(DroidUiTheme.ModMenu.SelectedText));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-icon-DT").Color, Is.EqualTo(DroidUiColors.TextPrimary));
    }

    [Test]
    public void SearchUsesAndroidInputThemeAndIconAspect()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-search").Color, Is.EqualTo(DroidUiTheme.ModMenu.Search));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-search-text").Color, Is.EqualTo(DroidUiTheme.ModMenu.SearchPlaceholder));
        UiElementSnapshot icon = frame.Elements.Single(element => element.Id == "modselect-search-icon");
        Assert.That(icon.SpriteFit, Is.EqualTo(UiSpriteFit.Contain));
        Assert.That(icon.Bounds.Width, Is.EqualTo(52f));
        Assert.That(icon.Bounds.Height, Is.EqualTo(28f));
    }

    [Test]
    public void ToggleRemovesIncompatibleModsAndFadesBlockedChoices()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(1);
        scene.ToggleMod(4);
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(scene.SelectedAcronyms, Does.Not.Contain("HT"));
        Assert.That(scene.SelectedAcronyms, Does.Contain("DT"));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-HT").Alpha, Is.EqualTo(0.5f));
    }

    [Test]
    public void ModMenuScrollShowsTemporaryScrollbars()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Scroll(700f, 0f, new UiPoint(640f, 200f), viewport);
        UiFrameSnapshot horizontal = scene.CreateSnapshot(viewport).UiFrame;
        var verticalScene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        verticalScene.Scroll(0f, 500f, new UiPoint(780f, 200f), viewport);
        UiFrameSnapshot vertical = verticalScene.CreateSnapshot(viewport).UiFrame;

        Assert.That(horizontal.Elements.Any(element => element.Id == "modselect-rail-scrollbar"), Is.True);
        Assert.That(vertical.Elements.Any(element => element.Id.StartsWith("modselect-section-", StringComparison.Ordinal) && element.Id.EndsWith("-scrollbar", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void ModMenuSectionDragContinuesAfterRelease()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var start = new UiPoint(780f, 300f);

        Assert.That(scene.TryBeginScrollDrag(start, viewport), Is.True);
        Assert.That(scene.UpdateScrollDrag(new UiPoint(780f, 220f), viewport), Is.True);
        UiFrameSnapshot afterDrag = scene.CreateSnapshot(viewport).UiFrame;
        float hardRockY = afterDrag.Elements.Single(element => element.Id == "modselect-toggle-HR").Bounds.Y;
        scene.EndScrollDrag(new UiPoint(780f, 200f), viewport);

        scene.Update(TimeSpan.FromSeconds(1d / 60d));
        UiFrameSnapshot afterInertia = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(afterInertia.Elements.Single(element => element.Id == "modselect-toggle-HR").Bounds.Y, Is.LessThan(hardRockY));
    }

    [Test]
    public void ConversionSectionScrollsOnShortViewportAndClampsScrollbarInsideList()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 600);

        scene.Scroll(700f, 0f, new UiPoint(640f, 200f), viewport);
        Assert.That(scene.TryBeginScrollDrag(new UiPoint(900f, 360f), viewport, 0d), Is.True);
        Assert.That(scene.UpdateScrollDrag(new UiPoint(900f, 220f), viewport, 0.05d), Is.True);
        scene.EndScrollDrag(new UiPoint(900f, 200f), viewport, 0.06d);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot scoreV2 = frame.Elements.Single(element => element.Id == "modselect-toggle-V2");
        UiElementSnapshot scrollbar = frame.Elements.Single(element => element.Id == "modselect-section-OsuDroidLanguagePack_mod_section_difficulty_conversion-scrollbar");

        Assert.That(scoreV2.Bounds.Bottom, Is.LessThanOrEqualTo(scrollbar.ClipBounds!.Value.Bottom));
        Assert.That(scrollbar.Bounds.Y, Is.GreaterThanOrEqualTo(scrollbar.ClipBounds.Value.Y));
        Assert.That(scrollbar.Bounds.Bottom, Is.LessThanOrEqualTo(scrollbar.ClipBounds.Value.Bottom));
    }

    [Test]
    public void DifficultyIncreaseBottomDoesNotBounceBackOnShortViewport()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 600);

        scene.Scroll(0f, 1000f, new UiPoint(780f, 220f), viewport);
        UiFrameSnapshot beforeUpdate = scene.CreateSnapshot(viewport).UiFrame;
        float traceableBefore = beforeUpdate.Elements.Single(element => element.Id == "modselect-toggle-TC").Bounds.Y;

        scene.Update(TimeSpan.FromSeconds(1d / 60d));
        UiFrameSnapshot afterUpdate = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(afterUpdate.Elements.Single(element => element.Id == "modselect-toggle-TC").Bounds.Y, Is.EqualTo(traceableBefore));
    }

    [Test]
    public void FooterUsesSelectedBeatmapStats()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        scene.SetSelectedBeatmap(CreateBeatmap());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-ar-value").Text, Is.EqualTo("7.00"));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-ar").Bounds.Y, Is.EqualTo(664f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-ar").Color, Is.EqualTo(DroidUiTheme.ModMenu.Badge));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-ar-label-bg").Bounds.Width, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-stat-ar").Bounds.Width));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-od-value").Text, Is.EqualTo("7.00"));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-cs-value").Text, Is.EqualTo("5.00"));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-hp-value").Text, Is.EqualTo("7.00"));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-bpm-value").Text, Is.EqualTo("130"));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-star-value").Text, Is.EqualTo("3.96"));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-ranked-badge").Alpha, Is.EqualTo(1f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-ranked-badge").Color, Is.EqualTo(DroidUiTheme.ModMenu.Ranked));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-ranked-badge-text").Color, Is.EqualTo(DroidUiTheme.ModMenu.RankedText));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-star-badge").Bounds.Right, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-ranked-badge").Bounds.X));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-ranked-badge").Bounds.Right, Is.LessThan(frame.Elements.Single(element => element.Id == "modselect-stat-score").Bounds.X));
    }

    [Test]
    public void UnrankedModUpdatesRankedBadge()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(14);
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-ranked-badge-text").Text, Is.EqualTo("Unranked"));
    }

    [Test]
    public void PresetAddPersistsSelectedMods()
    {
        var settings = new MemorySettingsStore();
        var textInput = new RecordingTextInputService();
        var scene = new ModSelectScene(settings, textInput);
        scene.ToggleMod(2);

        scene.FocusPresetName(VirtualViewport.FromSurface(1280, 720));
        textInput.LastRequest!.OnSubmitted("Safe");
        var restoredScene = new ModSelectScene(settings, new NoOpTextInputService());
        UiFrameSnapshot frame = restoredScene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "modselect-preset-Safe-name" && element.Text == "Safe"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-preset-Safe-NF" && element.AssetName == DroidAssets.ModNoFail), Is.True);
    }

    [Test]
    public void SongSelectModsOpensModSelectAndBackReturnsToSongSelect()
    {
        string root = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"modselect-route-{Guid.NewGuid():N}");
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        var database = new DroidDatabase(Path.Combine(root, "test.db"));
        database.EnsureCreated();
        var core = new OsuDroidGameCore(new GameServices(
            database,
            paths,
            "test",
            "1.0",
            BeatmapLibrary: new SingleBeatmapLibrary(),
            BeatmapProcessingService: new NoPendingBeatmapProcessingService()));

        OpenSoloRoute(core);
        core.HandleUiAction(UiAction.SongSelectMods);

        GameFrameSnapshot modFrame = core.CreateFrame(VirtualViewport.FromSurface(1280, 720));

        Assert.That(modFrame.Scene, Is.EqualTo("ModSelect"));
        Assert.That(modFrame.UiFrame.Elements.Any(element => element.Id == "songselect-base"), Is.True);
        Assert.That(modFrame.UiFrame.Elements.Single(element => element.Id == "songselect-mods").Action, Is.EqualTo(UiAction.None));
        Assert.That(modFrame.UiFrame.Elements.Single(element => element.Id == "modselect-background").Alpha, Is.EqualTo(0.9f));
        Assert.That(modFrame.UiFrame.Elements.Single(element => element.Id == "modselect-background").Color, Is.EqualTo(DroidUiTheme.ModMenu.SelectedText));

        core.HandleUiAction(UiAction.ModSelectBack);

        Assert.That(core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).Scene, Is.EqualTo("SongSelect"));
    }

}
