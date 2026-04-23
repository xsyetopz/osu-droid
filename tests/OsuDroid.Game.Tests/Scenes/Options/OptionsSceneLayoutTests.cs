using OsuDroid.Game;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class OptionsSceneTests
{

    [Test]
    public void OptionsSceneUsesFullScreenAndroidSettingsLayout()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        var root = frame.Elements.Single(element => element.Id == "options-root");
        var appBar = frame.Elements.Single(element => element.Id == "options-appbar");
        var back = frame.Elements.Single(element => element.Id == "options-back-hit");
        var backIcon = frame.Elements.Single(element => element.Id == "options-back");
        var selectedSection = frame.Elements.Single(element => element.Id == "options-section-selected");

        Assert.That(root.Bounds, Is.EqualTo(new UiRect(0f, 0f, 1280f, 720f)));
        Assert.That(root.Color, Is.EqualTo(UiColor.Opaque(19, 19, 26)));
        Assert.That(appBar.Bounds.Height, Is.EqualTo(DroidUiMetrics.AppBarHeight));
        Assert.That(back.Action, Is.EqualTo(UiAction.OptionsBack));
        Assert.That(backIcon.Kind, Is.EqualTo(UiElementKind.MaterialIcon));
        Assert.That(backIcon.MaterialIcon, Is.EqualTo(UiMaterialIcon.ArrowBack));
        Assert.That(backIcon.Bounds, Is.EqualTo(new UiRect(16f * DroidUiMetrics.DpScale, 16f * DroidUiMetrics.DpScale, DroidUiMetrics.SectionIconSize, DroidUiMetrics.SectionIconSize)));
        Assert.That(selectedSection.Bounds, Is.EqualTo(new UiRect(DroidUiMetrics.ContentPaddingX, DroidUiMetrics.ContentTop, DroidUiMetrics.SectionRailWidth, DroidUiMetrics.SectionHeight)));
        Assert.That(frame.Elements.Any(element => element.Id == "options-panel"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-close-hit"), Is.False);
    }
    [Test]
    public void OptionsSceneUsesMaterialIconsForSettingsControls()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "options-back").MaterialIcon, Is.EqualTo(UiMaterialIcon.ArrowBack));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-0-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.ViewGridOutline));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-1-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.GamepadVariantOutline));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-2-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.MonitorDashboard));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-3-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Headphones));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-4-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.LibraryMusic));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-5-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.GestureTapButton));
        Assert.That(frame.Elements.Single(element => element.Id == "options-section-6-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Cogs));
    }
    [Test]
    public void OptionsSceneDrawsAndroidRowsAndVisualOnlyControls()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        var row = frame.Elements.Single(element => element.Id == "options-row-0");
        var rowLabel = frame.Elements.Single(element => element.Id == "options-row-0-label");
        var rowSummary = frame.Elements.Single(element => element.Id == "options-row-0-summary");
        var firstCheckbox = frame.Elements.Single(element => element.Id == "options-row-0-checkbox");
        var secondCheckbox = frame.Elements.Single(element => element.Id == "options-row-1-checkbox");
        var selectDropdown = frame.Elements.Single(element => element.Id == "options-row-2-dropdown");

        Assert.That(row.IsEnabled, Is.True);
        Assert.That(row.Action, Is.EqualTo(UiAction.OptionsToggleServerConnection));
        Assert.That(row.Color, Is.EqualTo(UiColor.Opaque(22, 22, 34)));
        Assert.That(rowLabel.Text, Is.EqualTo("Server Connection"));
        Assert.That(rowSummary.Text, Is.EqualTo("Connect to osu!droid server"));
        Assert.That(rowLabel.Bounds.Y, Is.GreaterThan(row.Bounds.Y));
        Assert.That(rowSummary.Bounds.Y, Is.GreaterThan(rowLabel.Bounds.Y));
        Assert.That(firstCheckbox.Kind, Is.EqualTo(UiElementKind.MaterialIcon));
        Assert.That(firstCheckbox.MaterialIcon, Is.EqualTo(UiMaterialIcon.Check));
        Assert.That(firstCheckbox.Color, Is.EqualTo(UiColor.Opaque(32, 32, 46)));
        Assert.That(secondCheckbox.MaterialIcon, Is.EqualTo(UiMaterialIcon.Check));
        Assert.That(selectDropdown.Kind, Is.EqualTo(UiElementKind.MaterialIcon));
        Assert.That(selectDropdown.MaterialIcon, Is.EqualTo(UiMaterialIcon.ArrowDropDown));
    }
    [Test]
    public void OptionsSceneDrawsSliderInputButtonAndDisabledRows()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        var audioFrame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(audioFrame.Elements.Any(element => element.Id.EndsWith("-slider-track", StringComparison.Ordinal)), Is.True);
        Assert.That(audioFrame.Elements.Any(element => element.Id.EndsWith("-slider-fill", StringComparison.Ordinal)), Is.True);
        Assert.That(audioFrame.Elements.Any(element => element.Id.EndsWith("-slider-thumb", StringComparison.Ordinal)), Is.True);

        scene.HandleAction(UiAction.OptionsSectionGeneral, viewport);
        scene.Scroll(180f, viewport);
        var generalFrame = scene.CreateSnapshot(viewport).UiFrame;
        var input = generalFrame.Elements.First(element => element.Id.EndsWith("-input", StringComparison.Ordinal));

        var inputRowId = input.Id[..input.Id.LastIndexOf("-input", StringComparison.Ordinal)];
        var inputRow = generalFrame.Elements.Single(element => element.Id == inputRowId);
        var inputSummary = generalFrame.Elements.Single(element => element.Id == inputRowId + "-summary");

        Assert.That(input.Bounds.Height, Is.EqualTo(DroidUiMetrics.InputHeight));
        Assert.That(DroidUiMetrics.InputRowHeight, Is.GreaterThan(100f * DroidUiMetrics.DpScale));
        Assert.That(input.Bounds.Y, Is.GreaterThan(inputSummary.Bounds.Bottom));
        Assert.That(input.Bounds.Bottom, Is.LessThanOrEqualTo(inputRow.Bounds.Bottom - DroidUiMetrics.RowPadding + 0.01f));
    }
    [Test]
    public void OptionsSceneAdvancedDirectoriesUseAndroidRowsWithoutFakeUnavailableText()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAdvanced, viewport);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var texts = frame.Elements.Where(element => element.Kind == UiElementKind.Text).Select(element => element.Text).ToArray();

        Assert.That(scene.ActiveRows, Does.Contain("Core path"));
        Assert.That(scene.ActiveRows, Does.Contain("Skin top path"));
        Assert.That(scene.ActiveRows, Does.Contain("Songs directory"));
        Assert.That(texts, Does.Not.Contain("Unavailable in this build"));
        Assert.That(texts, Does.Contain("/sdcard/osu!droid/Songs"));
    }
    [Test]
    public void OptionsSceneSelectRowsKeepValueAndDropdownOnRight()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        var row = frame.Elements.Single(element => element.Id == "options-row-2");
        var summary = frame.Elements.Single(element => element.Id == "options-row-2-summary");
        var value = frame.Elements.Single(element => element.Id == "options-row-2-value");
        var dropdown = frame.Elements.Single(element => element.Id == "options-row-2-dropdown");

        Assert.That(summary.Bounds.Right, Is.LessThan(value.Bounds.X));
        Assert.That(value.Bounds.Right, Is.LessThanOrEqualTo(dropdown.Bounds.X));
        Assert.That(dropdown.Bounds.X, Is.GreaterThan(row.Bounds.Right - DroidUiMetrics.RowPadding - DroidUiMetrics.SectionIconSize - 0.01f));
        Assert.That(dropdown.Bounds.Y + dropdown.Bounds.Height / 2f, Is.EqualTo(row.Bounds.Y + row.Bounds.Height / 2f).Within(0.001f));
    }
    [Test]
    public void OptionsSceneSliderUsesAndroidSeekbarLayoutAndDrawableMetrics()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        var frame = scene.CreateSnapshot(viewport).UiFrame;

        var sliderRow = frame.Elements.Single(element => element.Id == "options-row-0");
        var label = frame.Elements.Single(element => element.Id == "options-row-0-label");
        var summary = frame.Elements.Single(element => element.Id == "options-row-0-summary");
        var value = frame.Elements.Single(element => element.Id == "options-row-0-value");
        var track = frame.Elements.Single(element => element.Id == "options-row-0-slider-track");
        var fill = frame.Elements.Single(element => element.Id == "options-row-0-slider-fill");
        var thumb = frame.Elements.Single(element => element.Id == "options-row-0-slider-thumb");

        Assert.That(label.Bounds.X, Is.EqualTo(sliderRow.Bounds.X + DroidUiMetrics.SeekbarContainerMarginX).Within(0.001f));
        Assert.That(summary.Bounds.Right, Is.LessThan(track.Bounds.X));
        Assert.That(value.Bounds.Right, Is.EqualTo(track.Bounds.Right).Within(0.001f));
        Assert.That(track.Bounds.Right, Is.EqualTo(sliderRow.Bounds.Right - DroidUiMetrics.SeekbarContainerMarginX).Within(0.001f));
        Assert.That(track.Bounds.Width, Is.LessThanOrEqualTo(DroidUiMetrics.ControlColumnWidth + 0.001f));
        Assert.That(track.Bounds.Width, Is.GreaterThanOrEqualTo(96f * DroidUiMetrics.DpScale - 0.001f));
        Assert.That(track.Bounds.Height, Is.EqualTo(DroidUiMetrics.SeekbarTrackHeight).Within(0.001f));
        Assert.That(track.CornerRadius, Is.EqualTo(12f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(fill.Color, Is.EqualTo(UiColor.Opaque(243, 115, 115)));
        Assert.That(thumb.Bounds.Width, Is.EqualTo(DroidUiMetrics.SeekbarThumbSize).Within(0.001f));
        Assert.That(thumb.Bounds.Height, Is.EqualTo(DroidUiMetrics.SeekbarThumbSize).Within(0.001f));
        Assert.That(thumb.Color, Is.EqualTo(UiColor.Opaque(255, 255, 255)));
    }
    [Test]
    public void OptionsSceneUsesAndroidHalfRoundedCategoryAndRows()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        var category = frame.Elements.Single(element => element.Id == "options-category-0-header");
        var middleRow = frame.Elements.Single(element => element.Id == "options-row-0");
        var bottomRow = frame.Elements.Single(element => element.Id == "options-row-2");

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
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        var selected = frame.Elements.Single(element => element.Id == "options-section-selected");
        var unselectedHit = frame.Elements.Single(element => element.Id == "options-section-1-hit");

        Assert.That(selected.CornerRadius, Is.EqualTo(DroidUiMetrics.AndroidSidebarRadius));
        Assert.That(selected.CornerMode, Is.EqualTo(UiCornerMode.All));
        Assert.That(unselectedHit.Alpha, Is.Zero);
    }
}
