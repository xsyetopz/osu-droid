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
    public void OptionsSceneImplementedSelectRowsCycleStoredValue()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsRow2, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot value = frame.Elements.Single(element => element.Id == "options-row-2-value");
        Assert.That(value.Text, Is.EqualTo("osu!standard"));
    }
    [Test]
    public void OptionsSceneSliderUsesAndroidSeekbarLayoutAndDrawableMetrics()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot sliderRow = frame.Elements.Single(element => element.Id == "options-row-0");
        UiElementSnapshot label = frame.Elements.Single(element => element.Id == "options-row-0-label");
        UiElementSnapshot summary = frame.Elements.Single(element => element.Id == "options-row-0-summary");
        UiElementSnapshot value = frame.Elements.Single(element => element.Id == "options-row-0-value");
        UiElementSnapshot track = frame.Elements.Single(element => element.Id == "options-row-0-slider-track");
        UiElementSnapshot fill = frame.Elements.Single(element => element.Id == "options-row-0-slider-fill");
        UiElementSnapshot thumb = frame.Elements.Single(element => element.Id == "options-row-0-slider-thumb");

        Assert.That(label.Bounds.X, Is.EqualTo(sliderRow.Bounds.X + DroidUiMetrics.SeekbarContainerMarginX).Within(0.001f));
        Assert.That(summary.Bounds.Right, Is.LessThan(value.Bounds.X));
        Assert.That(summary.Bounds.Bottom, Is.LessThan(track.Bounds.Y));
        Assert.That(value.Bounds.Right, Is.EqualTo(sliderRow.Bounds.Right - DroidUiMetrics.SeekbarContainerMarginX).Within(0.001f));
        Assert.That(value.TextStyle!.Alignment, Is.EqualTo(UiTextAlignment.Right));
        Assert.That(track.Bounds.X, Is.EqualTo(sliderRow.Bounds.X + DroidUiMetrics.SeekbarTrackMarginX).Within(0.001f));
        Assert.That(track.Bounds.Right, Is.EqualTo(sliderRow.Bounds.Right - DroidUiMetrics.SeekbarTrackMarginX).Within(0.001f));
        Assert.That(track.Bounds.Height, Is.EqualTo(DroidUiMetrics.SeekbarTrackHeight).Within(0.001f));
        Assert.That(track.CornerRadius, Is.EqualTo(12f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(fill.Color, Is.EqualTo(UiColor.Opaque(243, 115, 115)));
        Assert.That(fill.Bounds.X, Is.EqualTo(track.Bounds.X).Within(0.001f));
        Assert.That(fill.Bounds.Right, Is.EqualTo(track.Bounds.Right).Within(0.001f));
        Assert.That(thumb.Bounds.X + thumb.Bounds.Width / 2f, Is.EqualTo(track.Bounds.Right).Within(0.001f));
        Assert.That(thumb.Bounds.Width, Is.EqualTo(DroidUiMetrics.SeekbarThumbSize).Within(0.001f));
        Assert.That(thumb.Bounds.Height, Is.EqualTo(DroidUiMetrics.SeekbarThumbSize).Within(0.001f));
        Assert.That(thumb.Color, Is.EqualTo(UiColor.Opaque(255, 255, 255)));
    }
    [Test]
    public void OptionsSceneLongSliderSummaryWrapsWithoutShrinkingText()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        scene.Scroll(420f, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot row = frame.Elements.Single(element => element.Id == "options-row-3");
        UiElementSnapshot title = frame.Elements.Single(element => element.Id == "options-row-3-label");
        UiElementSnapshot firstSummaryLine = frame.Elements.Single(element => element.Id == "options-row-3-summary");
        UiElementSnapshot secondSummaryLine = frame.Elements.Single(element => element.Id == "options-row-3-summary-1");
        UiElementSnapshot finalSummaryLine = frame.Elements.Single(element => element.Id == "options-row-3-summary-3");
        UiElementSnapshot track = frame.Elements.Single(element => element.Id == "options-row-3-slider-track");

        Assert.That(row.Bounds.Height, Is.EqualTo(DroidUiMetrics.LongSliderRowHeight).Within(0.001f));
        Assert.That(title.Text, Is.EqualTo("Minimum Synchronization Limit"));
        Assert.That(title.TextStyle!.Size, Is.EqualTo(DroidUiMetrics.RowTitleSize).Within(0.001f));
        Assert.That(firstSummaryLine.TextStyle!.Size, Is.EqualTo(DroidUiMetrics.RowSummarySize).Within(0.001f));
        Assert.That(secondSummaryLine.Bounds.Y, Is.GreaterThan(firstSummaryLine.Bounds.Y));
        Assert.That(finalSummaryLine.Bounds.Bottom, Is.LessThan(track.Bounds.Y));
        Assert.That(track.Bounds.X, Is.EqualTo(row.Bounds.X + DroidUiMetrics.SeekbarTrackMarginX).Within(0.001f));
    }
    [Test]
    public void OptionsScenePathInputsUseStoredOverridesAndRestoreDefaultsOnBlankInput()
    {
        var defaults = new OptionsPathDefaults("/core", "/core/Skin", "/core/Songs");
        var settings = new MemorySettingsStore();
        settings.SetString("corePath", "/custom-core");
        var textInput = new CapturingTextInputService();
        var scene = new OptionsScene(new GameLocalizer(), settings, textInput, defaults);
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAdvanced, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(frame.Elements.Where(element => element.Kind == UiElementKind.Text).Select(element => element.Text), Does.Contain("/custom-core"));

        scene.HandleAction(UiAction.OptionsRow0, viewport);
        Assert.That(textInput.ActiveRequest!.Text, Is.EqualTo("/custom-core"));
        textInput.ActiveRequest!.OnSubmitted(string.Empty);

        Assert.That(scene.GetStringValue("corePath"), Is.EqualTo(defaults.CorePath));
        Assert.That(settings.GetString("corePath", string.Empty), Is.EqualTo(defaults.CorePath));
    }
    [Test]
    public void OptionsScenePathInputsEllipsizeIosContainerPathsForDisplayOnly()
    {
        string corePath = "/var/mobile/Containers/Data/Application/79AEE6E3-E6B9-47A2-980F-055F26C2F8B7/Library/osu!droid";
        string hyphenatedCorePath = corePath.Replace("/Library/osu!droid", "/Library/osu-droid", StringComparison.Ordinal);
        var defaults = new OptionsPathDefaults(corePath, $"{corePath}/Skin", $"{corePath}/Songs", UsesNativeDefaultSummaries: true);
        var settings = new MemorySettingsStore();
        settings.SetString("corePath", hyphenatedCorePath);
        settings.SetString("skinTopPath", $"{hyphenatedCorePath}/Skin");
        settings.SetString("directory", $"{hyphenatedCorePath}/Songs");
        var scene = new OptionsScene(new GameLocalizer(), settings, pathDefaults: defaults);
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAdvanced, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        string?[] values = frame.Elements
            .Where(element => element.Id.EndsWith("-input-value", StringComparison.Ordinal))
            .Select(element => element.Text)
            .ToArray();

        Assert.That(values, Does.Contain("/var/mobile/…/Library/osu!droid"));
        Assert.That(values, Does.Contain("/var/mobile/…/Library/osu!droid/Skin"));
        Assert.That(values, Does.Contain("/var/mobile/…/Library/osu!droid/Songs"));
        Assert.That(values, Does.Not.Contain(corePath));
        Assert.That(values, Does.Not.Contain("/var/mobile/…/Library/osu-droid/Skin"));
        Assert.That(scene.GetStringValue("corePath"), Is.EqualTo(corePath));
        Assert.That(settings.GetString("corePath", string.Empty), Is.EqualTo(corePath));
        Assert.That(settings.GetString("skinTopPath", string.Empty), Is.EqualTo(defaults.SkinTopPath));
        Assert.That(settings.GetString("directory", string.Empty), Is.EqualTo(defaults.SongsDirectory));

        string?[] summaries = frame.Elements
            .Where(element => element.Id.EndsWith("-summary", StringComparison.Ordinal))
            .Select(element => element.Text)
            .ToArray();

        Assert.That(summaries, Does.Contain("Path to directory containing skin files. (default: /var/mobile/…/Library/osu!droid/Skin)"));
        Assert.That(summaries, Does.Contain("Path to directory containing beatmaps (default: /var/mobile/…/Library/osu!droid/Songs)"));
        Assert.That(summaries, Does.Not.Contain("Path to directory containing skin files. (default: /sdcard/osu!droid/Skin)"));
        Assert.That(summaries, Does.Not.Contain("Path to directory containing beatmaps (default: /mnt/sdcard/osu!droid/Songs)"));
    }
    [Test]
    public void OptionsScenePathInputsMiddleEllipsizeCustomLongPaths()
    {
        string path = "/very/long/custom/location/with/many/segments/that/does/not/match/ios/container/osu!droid/Songs";
        var settings = new MemorySettingsStore();
        settings.SetString("directory", path);
        var scene = new OptionsScene(new GameLocalizer(), settings, pathDefaults: new OptionsPathDefaults("/core", "/core/Skin", "/core/Songs"));
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAdvanced, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        string?[] values = frame.Elements
            .Where(element => element.Id.EndsWith("-input-value", StringComparison.Ordinal))
            .Select(element => element.Text)
            .ToArray();

        Assert.That(values.Any(value => value is not null && value.Contains('…') && value.EndsWith("/container/osu!droid/Songs", StringComparison.Ordinal)), Is.True);
        Assert.That(scene.GetStringValue("directory"), Is.EqualTo(path));
    }

    [Test]
    public void OptionsSceneSliderDragUpdatesValueAndSuppressesScroll()
    {
        var settings = new MemorySettingsStore();
        var scene = new OptionsScene(new GameLocalizer(), settings);
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        float beforeScroll = scene.ContentScrollOffset;
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot track = frame.Elements.Single(element => element.Id == "options-row-0-slider-track");
        var dragPoint = new UiPoint(track.Bounds.X + track.Bounds.Width * 0.75f, track.Bounds.Y + track.Bounds.Height / 2f);

        Assert.That(scene.TryBeginSliderDrag("options-row-0-slider-thumb", dragPoint, viewport), Is.True);
        scene.Scroll(240f, viewport);
        scene.UpdateSliderDrag(dragPoint, viewport);
        scene.EndSliderDrag(dragPoint, viewport);

        Assert.That(scene.GetIntValue("bgmvolume"), Is.EqualTo(75).Within(1));
        Assert.That(settings.GetInt("bgmvolume", 0), Is.EqualTo(75).Within(1));
        Assert.That(scene.ContentScrollOffset, Is.EqualTo(beforeScroll));
    }
    [Test]
    public void OptionsSceneUsesAndroidHalfRoundedCategoryAndRows()
    {
        var scene = new OptionsScene(new GameLocalizer());
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        UiElementSnapshot category = frame.Elements.Single(element => element.Id == "options-category-0-header");
        UiElementSnapshot middleRow = frame.Elements.Single(element => element.Id == "options-row-0");
        UiElementSnapshot bottomRow = frame.Elements.Single(element => element.Id == "options-row-2");

        Assert.That(category.CornerMode, Is.EqualTo(UiCornerMode.Top));
        Assert.That(category.CornerRadius, Is.EqualTo(DroidUiMetrics.AndroidRoundedRectRadius));
        Assert.That(middleRow.CornerMode, Is.EqualTo(UiCornerMode.None));
        Assert.That(bottomRow.CornerMode, Is.EqualTo(UiCornerMode.Bottom));
        Assert.That(bottomRow.CornerRadius, Is.EqualTo(DroidUiMetrics.AndroidRoundedRectRadius));
        Assert.That(frame.Elements.Any(element => element.Id == "options-category-0-card"), Is.False);
    }
    [Test]
    public void OptionsSceneSidebarUnselectedTabsDoNotDrawVisibleBackgrounds()
    {
        var scene = new OptionsScene(new GameLocalizer());
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        UiElementSnapshot selected = frame.Elements.Single(element => element.Id == "options-section-selected");
        UiElementSnapshot unselectedHit = frame.Elements.Single(element => element.Id == "options-section-1-hit");

        Assert.That(selected.CornerRadius, Is.EqualTo(DroidUiMetrics.AndroidSidebarRadius));
        Assert.That(selected.CornerMode, Is.EqualTo(UiCornerMode.All));
        Assert.That(unselectedHit.Alpha, Is.Zero);
    }
}
