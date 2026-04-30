using NUnit.Framework;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.Scenes.Options;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Tests;

public sealed partial class OptionsSceneTests
{
    [Test]
    public void OptionsSceneUsesFullScreenAndroidSettingsLayout()
    {
        var scene = new OptionsScene(new GameLocalizer());
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        UiElementSnapshot root = frame.Elements.Single(element => element.Id == "options-root");
        UiElementSnapshot appBar = frame.Elements.Single(element => element.Id == "options-appbar");
        UiElementSnapshot back = frame.Elements.Single(element => element.Id == "options-back-hit");
        UiElementSnapshot backIcon = frame.Elements.Single(element => element.Id == "options-back");
        UiElementSnapshot selectedSection = frame.Elements.Single(element =>
            element.Id == "options-section-selected"
        );

        Assert.That(root.Bounds, Is.EqualTo(new UiRect(0f, 0f, 1280f, 720f)));
        Assert.That(root.Color, Is.EqualTo(UiColor.Opaque(19, 19, 26)));
        Assert.That(appBar.Bounds.Height, Is.EqualTo(DroidUiMetrics.AppBarHeight));
        Assert.That(back.Action, Is.EqualTo(UiAction.OptionsBack));
        Assert.That(backIcon.Kind, Is.EqualTo(UiElementKind.MaterialIcon));
        Assert.That(backIcon.MaterialIcon, Is.EqualTo(UiMaterialIcon.ArrowBack));
        Assert.That(
            backIcon.Bounds,
            Is.EqualTo(
                new UiRect(
                    16f * DroidUiMetrics.DpScale,
                    16f * DroidUiMetrics.DpScale,
                    DroidUiMetrics.SectionIconSize,
                    DroidUiMetrics.SectionIconSize
                )
            )
        );
        Assert.That(
            selectedSection.Bounds,
            Is.EqualTo(
                new UiRect(
                    DroidUiMetrics.ContentPaddingX,
                    DroidUiMetrics.ContentTop,
                    DroidUiMetrics.SectionRailWidth,
                    DroidUiMetrics.SectionHeight
                )
            )
        );
        Assert.That(frame.Elements.Any(element => element.Id == "options-panel"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-close-hit"), Is.False);
    }

    [Test]
    public void OptionsSceneUsesMaterialIconsForSettingsControls()
    {
        var scene = new OptionsScene(new GameLocalizer());
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame.Elements.Single(element => element.Id == "options-back").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.ArrowBack)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-section-0-icon").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.ViewGridOutline)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-section-1-icon").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.GamepadVariantOutline)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-section-2-icon").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.MonitorDashboard)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-section-3-icon").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.Headphones)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-section-4-icon").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.LibraryMusic)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-section-5-icon").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.GestureTapButton)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-section-6-icon").MaterialIcon,
            Is.EqualTo(UiMaterialIcon.Cogs)
        );
    }

    [Test]
    public void OptionsSceneDrawsAndroidRowsAndInteractiveControls()
    {
        var scene = new OptionsScene(new GameLocalizer());
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        UiElementSnapshot row = frame.Elements.Single(element => element.Id == "options-row-0");
        UiElementSnapshot rowLabel = frame.Elements.Single(element =>
            element.Id == "options-row-0-label"
        );
        UiElementSnapshot rowSummary = frame.Elements.Single(element =>
            element.Id == "options-row-0-summary"
        );
        UiElementSnapshot firstCheckbox = frame.Elements.Single(element =>
            element.Id == "options-row-0-checkbox"
        );
        UiElementSnapshot secondCheckbox = frame.Elements.Single(element =>
            element.Id == "options-row-1-checkbox"
        );

        Assert.That(row.IsEnabled, Is.True);
        Assert.That(row.Action, Is.EqualTo(UiAction.OptionsToggleServerConnection));
        Assert.That(row.Color, Is.EqualTo(UiColor.Opaque(22, 22, 34)));
        Assert.That(rowLabel.Text, Is.EqualTo("Server Connection"));
        Assert.That(rowSummary.Text, Is.EqualTo("Connect to osu!droid server"));
        Assert.That(
            rowSummary.TextStyle!.Size,
            Is.EqualTo(DroidUiMetrics.RowSummarySize).Within(0.001f)
        );
        Assert.That(rowSummary.ClipToBounds, Is.True);
        Assert.That(rowLabel.Bounds.Y, Is.GreaterThan(row.Bounds.Y));
        Assert.That(rowSummary.Bounds.Y, Is.GreaterThan(rowLabel.Bounds.Y));
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-0-lock"), Is.False);
        Assert.That(firstCheckbox.Kind, Is.EqualTo(UiElementKind.MaterialIcon));
        Assert.That(firstCheckbox.MaterialIcon, Is.EqualTo(UiMaterialIcon.CheckboxBlankOutline));
        Assert.That(firstCheckbox.Color, Is.EqualTo(UiColor.Opaque(178, 178, 204)));
        Assert.That(secondCheckbox.MaterialIcon, Is.EqualTo(UiMaterialIcon.CheckboxBlankOutline));
    }

    [Test]
    public void OptionsSceneLockedRowsStayVisibleAndIgnoreTap()
    {
        var settings = new MemorySettingsStore();
        var scene = new OptionsScene(new GameLocalizer(), settings);
        var viewport = VirtualViewport.FromSurface(1280, 720);
        scene.HandleAction(UiAction.OptionsSectionGameplay, viewport);
        bool initial = scene.GetBoolValue("showfirstapproachcircle");

        scene.HandleAction(UiAction.OptionsActiveRow0, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(scene.GetBoolValue("showfirstapproachcircle"), Is.EqualTo(initial));
        Assert.That(
            frame.Elements.Single(element => element.Id == "options-row-0-lock").Kind,
            Is.EqualTo(UiElementKind.MaterialIcon)
        );
        UiElementSnapshot row = frame.Elements.Single(element => element.Id == "options-row-0");
        UiElementSnapshot lockOverlay = frame.Elements.Single(element =>
            element.Id == "options-row-0-locked-overlay"
        );
        UiElementSnapshot lockIcon = frame.Elements.Single(element =>
            element.Id == "options-row-0-lock"
        );
        UiElementSnapshot summary = frame.Elements.Single(element =>
            element.Id == "options-row-0-summary"
        );

        Assert.That(
            lockIcon.Bounds.X + lockIcon.Bounds.Width / 2f,
            Is.EqualTo(row.Bounds.X + row.Bounds.Width / 2f).Within(0.001f)
        );
        Assert.That(
            lockIcon.Bounds.Y + lockIcon.Bounds.Height / 2f,
            Is.EqualTo(row.Bounds.Y + row.Bounds.Height / 2f).Within(0.001f)
        );
        Assert.That(lockOverlay.Bounds, Is.EqualTo(row.Bounds));
        Assert.That(row.IsEnabled, Is.False);
        Assert.That(
            summary.TextStyle!.Size,
            Is.EqualTo(DroidUiMetrics.RowSummarySize).Within(0.001f)
        );
        Assert.That(summary.ClipToBounds, Is.True);
    }

    [Test]
    public void OptionsSceneImplementedRowsDoNotDrawLockOverlay()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        scene.HandleAction(UiAction.OptionsSectionGeneral, viewport);
        frame = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-0-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-1-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-2-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-5-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-7-lock"), Is.False);

        scene.HandleAction(UiAction.OptionsSectionLibrary, viewport);
        frame = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-0-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-1-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-2-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-3-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-4-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-6-lock"), Is.False);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        frame = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-0-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-1-lock"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "options-row-7-lock"), Is.False);
    }

    [Test]
    public void OptionsSceneDrawsSliderInputButtonAndDisabledRows()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAudio, viewport);
        UiFrameSnapshot audioFrame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(
            audioFrame.Elements.Any(element =>
                element.Id.EndsWith("-slider-track", StringComparison.Ordinal)
            ),
            Is.True
        );
        Assert.That(
            audioFrame.Elements.Any(element =>
                element.Id.EndsWith("-slider-fill", StringComparison.Ordinal)
            ),
            Is.True
        );
        Assert.That(
            audioFrame.Elements.Any(element =>
                element.Id.EndsWith("-slider-thumb", StringComparison.Ordinal)
            ),
            Is.True
        );

        scene.HandleAction(UiAction.OptionsSectionGeneral, viewport);
        scene.Scroll(180f, viewport);
        UiFrameSnapshot generalFrame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot input = generalFrame.Elements.First(element =>
            element.Id.EndsWith("-input", StringComparison.Ordinal)
        );

        string inputRowId = input.Id[..input.Id.LastIndexOf("-input", StringComparison.Ordinal)];
        UiElementSnapshot inputRow = generalFrame.Elements.Single(element =>
            element.Id == inputRowId
        );
        UiElementSnapshot inputSummary = generalFrame.Elements.Single(element =>
            element.Id == inputRowId + "-summary"
        );

        Assert.That(input.Bounds.Height, Is.EqualTo(DroidUiMetrics.InputHeight));
        Assert.That(DroidUiMetrics.InputRowHeight, Is.GreaterThan(100f * DroidUiMetrics.DpScale));
        Assert.That(input.Bounds.Y, Is.GreaterThan(inputSummary.Bounds.Bottom));
        Assert.That(
            input.Bounds.Bottom,
            Is.LessThanOrEqualTo(inputRow.Bounds.Bottom - DroidUiMetrics.RowPadding + 0.01f)
        );
    }

    [Test]
    public void OptionsSceneAdvancedDirectoriesUseAndroidRowsWithoutFakeUnavailableText()
    {
        var defaults = new OptionsPathDefaults(
            "/tmp/osu!droid",
            "/tmp/osu!droid/Skin",
            "/tmp/osu!droid/Songs"
        );
        var scene = new OptionsScene(new GameLocalizer(), pathDefaults: defaults);
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionAdvanced, viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        string?[] texts = frame
            .Elements.Where(element => element.Kind == UiElementKind.Text)
            .Select(element => element.Text)
            .ToArray();

        Assert.That(scene.ActiveRows, Does.Contain("Main directory"));
        Assert.That(scene.ActiveRows, Does.Contain("Skin directory"));
        Assert.That(scene.ActiveRows, Does.Contain("Beatmap location"));
        Assert.That(texts, Does.Not.Contain("Unavailable in this build"));
        Assert.That(texts, Does.Not.Contain("Not available"));
        Assert.That(texts, Does.Contain(defaults.CorePath));
        Assert.That(texts, Does.Contain(defaults.SkinTopPath));
        Assert.That(texts, Does.Contain(defaults.SongsDirectory));
    }

    [Test]
    public void OptionsSceneSelectRowsUseAndroidDialogInsteadOfInlineValue()
    {
        var scene = new OptionsScene(new GameLocalizer());
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        UiElementSnapshot categoryTitle = frame.Elements.Single(element =>
            element.Id == "options-category-0-title"
        );
        UiElementSnapshot categoryHeader = frame.Elements.Single(element =>
            element.Id == "options-category-0-header"
        );
        UiElementSnapshot summary = frame.Elements.Single(element =>
            element.Id == "options-row-2-summary"
        );
        UiElementSnapshot accountTitle = frame.Elements.Single(element =>
            element.Id == "options-category-3-title"
        );

        Assert.That(frame.Elements.Any(element => element.Id == "options-row-2-value"), Is.False);
        Assert.That(
            frame.Elements.Any(element => element.Id == "options-row-2-dropdown"),
            Is.False
        );
        Assert.That(
            frame.Elements.Any(element => element.Id == "options-row-2-summary-1"),
            Is.False
        );
        Assert.That(
            summary.Text,
            Is.EqualTo("Choose the algorithm used to calculate difficulty and performance points")
        );
        Assert.That(categoryTitle.Text, Is.EqualTo("Online"));
        Assert.That(categoryTitle.TextStyle!.Alignment, Is.EqualTo(UiTextAlignment.Center));
        Assert.That(
            categoryTitle.Bounds.X + categoryTitle.Bounds.Width / 2f,
            Is.EqualTo(categoryHeader.Bounds.X + categoryHeader.Bounds.Width / 2f).Within(0.001f)
        );
        Assert.That(accountTitle.Text, Is.EqualTo("Account"));
        Assert.That(accountTitle.Bounds.Y, Is.LessThan(720f));
    }

    [Test]
    public void OptionsSceneSelectDialogMatchesAndroidChoiceBehavior()
    {
        var settings = new MemorySettingsStore();
        var scene = new OptionsScene(new GameLocalizer(), settings);
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsActiveRow2, viewport);
        UiFrameSnapshot dialogFrame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot title = dialogFrame.Elements.Single(element =>
            element.Id == "options-select-dialog-title"
        );
        UiElementSnapshot selectedOption = dialogFrame.Elements.Single(element =>
            element.Id == "options-select-dialog-option-0"
        );
        UiElementSnapshot selectedCheck = dialogFrame.Elements.Single(element =>
            element.Id == "options-select-dialog-option-0-check"
        );

        Assert.That(title.Text, Is.EqualTo("Difficulty algorithm"));
        Assert.That(title.TextStyle!.Alignment, Is.EqualTo(UiTextAlignment.Center));
        Assert.That(selectedOption.Action, Is.EqualTo(UiAction.OptionsSelectDialogOption0));
        Assert.That(selectedCheck.MaterialIcon, Is.EqualTo(UiMaterialIcon.Check));

        scene.HandleAction(UiAction.OptionsSelectDialogOption1, viewport);
        Assert.That(scene.GetIntValue("difficultyAlgorithm"), Is.EqualTo(1));
        Assert.That(settings.GetInt("difficultyAlgorithm", 0), Is.EqualTo(1));
        Assert.That(
            scene
                .CreateSnapshot(viewport)
                .UiFrame.Elements.Any(element => element.Id == "options-select-dialog"),
            Is.False
        );

        scene.HandleAction(UiAction.OptionsActiveRow2, viewport);
        scene.HandleAction(UiAction.OptionsSelectDialogBackdrop, viewport);
        Assert.That(scene.GetIntValue("difficultyAlgorithm"), Is.EqualTo(1));
        Assert.That(
            scene
                .CreateSnapshot(viewport)
                .UiFrame.Elements.Any(element => element.Id == "options-select-dialog"),
            Is.False
        );
    }
}

internal sealed class MemorySettingsStore : IGameSettingsStore
{
    private readonly Dictionary<string, bool> _boolValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _intValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _stringValues = new(StringComparer.Ordinal);

    public bool GetBool(string key, bool defaultValue) =>
        _boolValues.GetValueOrDefault(key, defaultValue);

    public int GetInt(string key, int defaultValue) =>
        _intValues.GetValueOrDefault(key, defaultValue);

    public string GetString(string key, string defaultValue) =>
        _stringValues.GetValueOrDefault(key, defaultValue);

    public void SetBool(string key, bool value) => _boolValues[key] = value;

    public void SetInt(string key, int value) => _intValues[key] = value;

    public void SetString(string key, string value) => _stringValues[key] = value;
}

internal sealed class CapturingTextInputService : ITextInputService
{
    public TextInputRequest? ActiveRequest { get; private set; }

    public void RequestTextInput(TextInputRequest request) => ActiveRequest = request;

    public void HideTextInput() { }
}
