using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
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

        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-back-text" && element.Text == "Back"
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-customize" && !element.IsEnabled
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-clear-text" && element.Text == "Clear"
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-search-text" && element.Text == "Search..."
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-section-title-presets" && element.Text == "Presets"
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id
                    == "modselect-section-title-OsuDroidLanguagePack_mod_section_difficulty_reduction"
                && element.Text == "Difficulty Reduction"
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id
                    == "modselect-section-title-OsuDroidLanguagePack_mod_section_difficulty_automation"
                && element.Text == "Automation"
            ),
            Is.True
        );
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-NF"), Is.True);
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-toggle-icon-NF"
                && element.AssetName == DroidAssets.ModNoFail
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-stat-score-value").Text,
            Is.EqualTo("1.00x")
        );
    }

    [Test]
    public void ToggleAddsSelectedChipAndUpdatesScoreMultiplier()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(2);
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(scene.SelectedAcronyms, Does.Contain("NF"));
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-selected-NF" && element.AssetName == DroidAssets.ModNoFail
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-stat-score-value").Text,
            Is.EqualTo("0.50x")
        );
    }

    [Test]
    public void ClearRemovesSelectedMods()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(0);
        scene.Clear();
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(scene.SelectedAcronyms, Is.Empty);
        Assert.That(
            frame.Elements.Any(element => element.Id == "modselect-selected-NF-text"),
            Is.False
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-stat-score-value").Text,
            Is.EqualTo("1.00x")
        );
    }

    [Test]
    public void SearchFiltersByAcronymAndName()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.SetSearchTerm("muted");
        scene.Update(TimeSpan.FromMilliseconds(200));
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

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

        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame
                .Elements.Single(element => element.Id == "modselect-section-presets")
                .Bounds.Width,
            Is.EqualTo(300f)
        );
        Assert.That(
            frame
                .Elements.Single(element =>
                    element.Id
                    == "modselect-section-OsuDroidLanguagePack_mod_section_difficulty_reduction"
                )
                .Bounds.Width,
            Is.EqualTo(340f)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Height,
            Is.EqualTo(82f)
        );
        Assert.That(
            frame
                .Elements.Single(element => element.Id == "modselect-toggle-description-NF")
                .TextStyle!.Size,
            Is.EqualTo(16f)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-section-presets").Alpha,
            Is.EqualTo(1f)
        );
        Assert.That(
            frame
                .Elements.Single(element =>
                    element.Id
                    == "modselect-section-OsuDroidLanguagePack_mod_section_difficulty_reduction"
                )
                .Alpha,
            Is.EqualTo(1f)
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-section-OsuDroidLanguagePack_mod_section_fun"
            ),
            Is.False
        );
    }

    [Test]
    public void DefaultLayoutUsesAndroidModOrder()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-toggle-EZ").Bounds.Y,
            Is.LessThan(
                frame.Elements.Single(element => element.Id == "modselect-toggle-HT").Bounds.Y
            )
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-toggle-HT").Bounds.Y,
            Is.LessThan(
                frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Y
            )
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Y,
            Is.LessThan(
                frame.Elements.Single(element => element.Id == "modselect-toggle-RE").Bounds.Y
            )
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-toggle-DT").Bounds.Y,
            Is.LessThan(
                frame.Elements.Single(element => element.Id == "modselect-toggle-FL").Bounds.Y
            )
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-toggle-FL").Bounds.Y,
            Is.LessThan(
                frame.Elements.Single(element => element.Id == "modselect-toggle-HR").Bounds.Y
            )
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-toggle-HR").Bounds.Y,
            Is.LessThan(
                frame.Elements.Single(element => element.Id == "modselect-toggle-HD").Bounds.Y
            )
        );
    }

    [Test]
    public void HorizontalRailScrollRevealsOffscreenSections()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Scroll(700f, 0f, new UiPoint(640f, 200f), viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-section-OsuDroidLanguagePack_mod_section_fun"
            ),
            Is.True
        );
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

        UiElementSnapshot suddenDeath = frame.Elements.Single(element =>
            element.Id == "modselect-toggle-SD"
        );
        Assert.That(suddenDeath.Bounds.Bottom, Is.LessThanOrEqualTo(636f));
        Assert.That(suddenDeath.ClipBounds, Is.Not.Null);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-HR"), Is.False);
    }

    [Test]
    public void SectionScrollRendersPartiallyVisibleRowsWithClipBounds()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Scroll(0f, 490f, new UiPoint(780f, 200f), viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot nightcore = frame.Elements.Single(element =>
            element.Id == "modselect-toggle-NC"
        );
        Assert.That(nightcore.Bounds.Y, Is.LessThan(220f));
        Assert.That(nightcore.ClipBounds, Is.EqualTo(new UiRect(732f, 138f, 340f, 502f)));
    }

    [Test]
    public void DescriptionTextClipsAndAutoScrolls()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.Update(TimeSpan.FromSeconds(4));
        UiElementSnapshot description = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame.Elements.Single(element =>
                element.Id == "modselect-toggle-description-EZ"
                && element.Kind == UiElementKind.Text
            );

        Assert.That(description.ClipToBounds, Is.True);
        Assert.That(description.TextStyle!.AutoScroll, Is.Not.Null);
        Assert.That(description.TextStyle.AutoScroll!.ElapsedSeconds, Is.GreaterThan(3d));
    }

    [Test]
    public void CatalogUsesOsuDroidPlayableSectionOrder()
    {
        string[] expected =
        [
            "EZ",
            "HT",
            "NF",
            "RE",
            "DT",
            "FL",
            "HR",
            "HD",
            "NC",
            "PF",
            "PR",
            "SD",
            "TC",
            "AT",
            "AP",
            "RX",
            "CS",
            "DA",
            "MR",
            "RD",
            "V2",
            "AD",
            "FR",
            "MU",
            "SY",
            "WD",
            "WU",
        ];

        Assert.That(ModCatalog.Entries.Select(entry => entry.Acronym), Is.EqualTo(expected));
    }

    [Test]
    public void CustomizePanelShowsSelectedModSettingsAndAppliesValues()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        scene.ToggleMod(16);

        Assert.That(scene.ToggleCustomizePanel(), Is.True);
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame.Elements.Any(element => element.Id == "modselect-customize-dialog"),
            Is.True
        );
        Assert.That(
            frame.Elements.Any(element =>
                element.Id == "modselect-customize-row-0-name"
                && element.Text == "Track rate multiplier"
            ),
            Is.True
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "modselect-customize-row-0-value").Text,
            Is.EqualTo("1x")
        );

        Assert.That(scene.AdjustCustomizeSetting(0, 1), Is.True);
        UiFrameSnapshot updated = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            updated
                .Elements.Single(element => element.Id == "modselect-customize-row-0-value")
                .Text,
            Is.EqualTo("1.05x")
        );
        Assert.That(
            updated.Elements.Single(element => element.Id == "modselect-stat-score-value").Text,
            Is.EqualTo("1.01x")
        );
    }

    [Test]
    public void CustomizeSliderHitTestsAndDragsLikeSharedSlider()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        scene.ToggleMod(16);
        Assert.That(scene.ToggleCustomizePanel(), Is.True);

        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot track = frame.Elements.Single(element =>
            element.Id == "modselect-customize-slider-0"
        );
        UiElementSnapshot progress = frame.Elements.Single(element =>
            element.Id == "modselect-customize-slider-0-progress"
        );
        UiElementSnapshot thumb = frame.Elements.Single(element =>
            element.Id == "modselect-customize-slider-0-thumb"
        );

        UiPoint trackPoint = new(
            track.Bounds.Right - 8f,
            track.Bounds.Y + track.Bounds.Height / 2f
        );
        UiPoint progressPoint = new(
            progress.Bounds.X + 8f,
            progress.Bounds.Y + progress.Bounds.Height / 2f
        );
        UiPoint thumbPoint = new(
            thumb.Bounds.X + thumb.Bounds.Width / 2f,
            thumb.Bounds.Y + thumb.Bounds.Height / 2f
        );

        Assert.That(frame.HitTest(trackPoint)?.Id, Is.EqualTo(track.Id));
        Assert.That(frame.HitTest(progressPoint)?.Id, Is.EqualTo(progress.Id));
        Assert.That(frame.HitTest(thumbPoint)?.Id, Is.EqualTo(thumb.Id));

        Assert.That(scene.TryBeginCustomizeSliderDrag(thumb.Id, thumbPoint, viewport), Is.True);
        UiPoint dragPoint = new(track.Bounds.X + track.Bounds.Width * 0.7f, thumbPoint.Y);
        Assert.That(scene.UpdateCustomizeSliderDrag(dragPoint, viewport), Is.True);
        scene.EndCustomizeSliderDrag(dragPoint, viewport);

        UiFrameSnapshot updated = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(
            updated
                .Elements.Single(element => element.Id == "modselect-customize-row-0-value")
                .Text,
            Is.EqualTo("1.55x")
        );
    }

    [Test]
    public void FixedRateAndFreezeFrameDoNotOpenCustomizePanel()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(4);
        Assert.That(scene.SelectedAcronyms, Does.Contain("DT"));
        Assert.That(scene.ToggleCustomizePanel(), Is.False);

        scene.Clear();
        scene.ToggleMod(22);
        Assert.That(scene.SelectedAcronyms, Does.Contain("FR"));
        Assert.That(scene.ToggleCustomizePanel(), Is.False);
    }

    [Test]
    public void ModStatsExposeOsuDroidSongSelectLineDirections()
    {
        BeatmapInfo beatmap = TestBeatmap();

        ModStatSnapshot doubleTime = ModStatCalculator.FromBeatmap(
            beatmap,
            new ModSelectionState(
                ["DT"],
                new Dictionary<string, IReadOnlyDictionary<string, string>>()
            )
        );
        Assert.That(doubleTime.BpmDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(doubleTime.ApproachRateDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(doubleTime.OverallDifficultyDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(doubleTime.CircleSizeDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(doubleTime.HpDrainRateDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(doubleTime.ApproachRate, Is.EqualTo(9f).Within(0.001f));
        Assert.That(doubleTime.OverallDifficulty, Is.EqualTo(11.333f).Within(0.001f));
        Assert.That(doubleTime.DifficultyLineDirection, Is.EqualTo(ModStatDirection.Increased));

        ModStatSnapshot halfTime = ModStatCalculator.FromBeatmap(
            beatmap,
            new ModSelectionState(
                ["HT"],
                new Dictionary<string, IReadOnlyDictionary<string, string>>()
            )
        );
        Assert.That(halfTime.BpmDirection, Is.EqualTo(ModStatDirection.Decreased));
        Assert.That(halfTime.ApproachRateDirection, Is.EqualTo(ModStatDirection.Decreased));
        Assert.That(halfTime.OverallDifficultyDirection, Is.EqualTo(ModStatDirection.Decreased));
        Assert.That(halfTime.CircleSizeDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(halfTime.HpDrainRateDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(halfTime.DifficultyLineDirection, Is.EqualTo(ModStatDirection.Decreased));

        ModStatSnapshot customSpeed = ModStatCalculator.FromBeatmap(
            beatmap,
            new ModSelectionState(
                ["CS"],
                new Dictionary<string, IReadOnlyDictionary<string, string>>
                {
                    ["CS"] = new Dictionary<string, string> { ["rateMultiplier"] = "1.05" },
                }
            )
        );
        Assert.That(customSpeed.BpmDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(customSpeed.ApproachRateDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(customSpeed.OverallDifficultyDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(customSpeed.CircleSizeDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(customSpeed.HpDrainRateDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(customSpeed.DifficultyLineDirection, Is.EqualTo(ModStatDirection.Increased));

        ModStatSnapshot easy = ModStatCalculator.FromBeatmap(
            beatmap,
            new ModSelectionState(
                ["EZ"],
                new Dictionary<string, IReadOnlyDictionary<string, string>>()
            )
        );
        Assert.That(easy.BpmDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(easy.ApproachRateDirection, Is.EqualTo(ModStatDirection.Decreased));
        Assert.That(easy.OverallDifficultyDirection, Is.EqualTo(ModStatDirection.Decreased));
        Assert.That(easy.CircleSizeDirection, Is.EqualTo(ModStatDirection.Decreased));
        Assert.That(easy.HpDrainRateDirection, Is.EqualTo(ModStatDirection.Decreased));
        Assert.That(easy.DifficultyLineDirection, Is.EqualTo(ModStatDirection.Decreased));

        ModStatSnapshot hardRock = ModStatCalculator.FromBeatmap(
            beatmap,
            new ModSelectionState(
                ["HR"],
                new Dictionary<string, IReadOnlyDictionary<string, string>>()
            )
        );
        Assert.That(hardRock.BpmDirection, Is.EqualTo(ModStatDirection.Unchanged));
        Assert.That(hardRock.ApproachRateDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(hardRock.OverallDifficultyDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(hardRock.CircleSizeDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(hardRock.HpDrainRateDirection, Is.EqualTo(ModStatDirection.Increased));
        Assert.That(hardRock.DifficultyLineDirection, Is.EqualTo(ModStatDirection.Increased));
    }

    [Test]
    public void CustomizeModalConsumesBackgroundScrollAndDrag()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);
        scene.ToggleMod(16);
        Assert.That(scene.ToggleCustomizePanel(), Is.True);

        UiFrameSnapshot before = scene.CreateSnapshot(viewport).UiFrame;
        float beforeAutomationX = before
            .Elements.Single(element =>
                element.Id
                == "modselect-section-OsuDroidLanguagePack_mod_section_difficulty_automation"
            )
            .Bounds.X;

        scene.Scroll(700f, 0f, new UiPoint(320f, 230f), viewport);
        Assert.That(scene.TryBeginScrollDrag(new UiPoint(320f, 230f), viewport, 0d), Is.True);
        Assert.That(scene.UpdateScrollDrag(new UiPoint(120f, 230f), viewport, 0.1d), Is.True);
        scene.EndScrollDrag(new UiPoint(80f, 230f), viewport, 0.2d);

        UiFrameSnapshot after = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(
            after
                .Elements.Single(element =>
                    element.Id
                    == "modselect-section-OsuDroidLanguagePack_mod_section_difficulty_automation"
                )
                .Bounds.X,
            Is.EqualTo(beforeAutomationX)
        );
    }

    [Test]
    public void StarRatingTextIsCentered()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        UiElementSnapshot starText = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame.Elements.Single(element => element.Id == "modselect-star-value");

        Assert.That(starText.TextStyle!.Alignment, Is.EqualTo(UiTextAlignment.Center));
    }
}
