using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes.ModSelect;

namespace OsuDroid.Game.Tests;

public sealed class ModSelectSceneTests
{
    [Test]
    public void SnapshotUsesLegacySectionsAndControls()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "modselect-back-text" && element.Text == "Back"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-customize" && !element.IsEnabled), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-clear-text" && element.Text == "Clear"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-search-text" && element.Text == "Search..."), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-presets" && element.Text == "Presets"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-LegacyLanguagePack_mod_section_difficulty_reduction" && element.Text == "Difficulty Reduction"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-LegacyLanguagePack_mod_section_difficulty_automation" && element.Text == "Automation"), Is.True);
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
    public void LayoutKeepsLegacySectionWidthAndDoesNotShrinkRows()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-section-presets").Bounds.Width, Is.EqualTo(300f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-section-LegacyLanguagePack_mod_section_difficulty_reduction").Bounds.Width, Is.EqualTo(340f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Height, Is.EqualTo(82f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-description-NF").TextStyle!.Size, Is.EqualTo(16f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-section-presets").Alpha, Is.EqualTo(0.9f));
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-LegacyLanguagePack_mod_section_fun"), Is.False);
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

        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-LegacyLanguagePack_mod_section_fun"), Is.True);
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
        Assert.That(icon.Bounds.Width, Is.EqualTo(icon.Bounds.Height));
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
        UiElementSnapshot scrollbar = frame.Elements.Single(element => element.Id == "modselect-section-LegacyLanguagePack_mod_section_difficulty_conversion-scrollbar");

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

    private static void OpenSoloRoute(OsuDroidGameCore core)
    {
        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuFirst);
        core.HandleUiAction(UiAction.MainMenuFirst);
    }

    private static BeatmapInfo CreateBeatmap() => new(
        "Insane.osu",
        "1 capsule - JUMPER",
        "md5",
        null,
        "audio.mp3",
        null,
        null,
        1,
        "JUMPER",
        string.Empty,
        "capsule",
        string.Empty,
        "Mafiamaster",
        "Insane",
        string.Empty,
        string.Empty,
        0,
        7,
        7,
        5,
        7,
        3.96f,
        4.7f,
        130,
        130,
        130,
        238000,
        0,
        258,
        221,
        1,
        766,
        false);

    private sealed class RecordingTextInputService : ITextInputService
    {
        public TextInputRequest? LastRequest { get; private set; }

        public void RequestTextInput(TextInputRequest request) => LastRequest = request;

        public void HideTextInput()
        {
        }
    }

    private sealed class MemorySettingsStore : IGameSettingsStore
    {
        private readonly Dictionary<string, GameSettingValue> _settings = new(StringComparer.Ordinal);

        public bool GetBool(string key, bool defaultValue) =>
            _settings.TryGetValue(key, out GameSettingValue setting) && setting.Kind == GameSettingValueKind.Flag ? setting.BoolValue : defaultValue;

        public int GetInt(string key, int defaultValue) =>
            _settings.TryGetValue(key, out GameSettingValue setting) && setting.Kind == GameSettingValueKind.Number ? setting.IntValue : defaultValue;

        public string GetString(string key, string defaultValue) =>
            _settings.TryGetValue(key, out GameSettingValue setting) && setting.Kind == GameSettingValueKind.Text ? setting.TextValue : defaultValue;

        public void SetBool(string key, bool value) => _settings[key] = GameSettingValue.FromBool(value);

        public void SetInt(string key, int value) => _settings[key] = GameSettingValue.FromInt(value);

        public void SetString(string key, string value) => _settings[key] = GameSettingValue.FromString(value);
    }

    private sealed class NoPendingBeatmapProcessingService : IBeatmapProcessingService
    {
        public BeatmapProcessingState State { get; } = new();

        public bool HasPendingWork() => false;

        public void EnqueueArchive(string archivePath, BeatmapOnlineMetadata? metadata = null)
        {
        }

        public void Start()
        {
        }

        public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
        {
            snapshot = BeatmapLibrarySnapshot.Empty;
            return false;
        }
    }

    private sealed class SingleBeatmapLibrary : IBeatmapLibrary
    {
        private readonly BeatmapLibrarySnapshot _snapshot = new([
            new BeatmapSetInfo(1, "1 Artist - Title", [
                new BeatmapInfo(
                    "Easy.osu",
                    "1 Artist - Title",
                    "md5",
                    null,
                    "audio.mp3",
                    null,
                    null,
                    1,
                    "Title",
                    string.Empty,
                    "Artist",
                    string.Empty,
                    "Mapper",
                    "Easy",
                    string.Empty,
                    string.Empty,
                    0,
                    5,
                    5,
                    5,
                    5,
                    1,
                    1,
                    120,
                    120,
                    120,
                    1000,
                    0,
                    1,
                    0,
                    0,
                    1,
                    false)
            ])
        ]);

        public BeatmapLibrarySnapshot Snapshot => _snapshot;

        public BeatmapLibrarySnapshot Load() => _snapshot;

        public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null) => _snapshot;

        public void ApplyOnlineMetadata(string setDirectory, BeatmapOnlineMetadata metadata)
        {
        }

        public bool NeedsScanRefresh() => false;

        public BeatmapOptions GetOptions(string setDirectory) => new(setDirectory);

        public void SaveOptions(BeatmapOptions options)
        {
        }

        public IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null) => [];

        public IReadOnlySet<string> GetCollectionSetDirectories(string name) => new HashSet<string>(StringComparer.Ordinal);

        public bool CreateCollection(string name) => true;

        public void DeleteCollection(string name)
        {
        }

        public void ToggleCollectionMembership(string name, string setDirectory)
        {
        }

        public void DeleteBeatmapSet(string directory)
        {
        }

        public void ClearBeatmapCache()
        {
        }

        public void ClearProperties()
        {
        }
    }
}
