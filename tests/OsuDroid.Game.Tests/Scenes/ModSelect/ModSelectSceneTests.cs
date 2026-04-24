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
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-LegacyLanguagePack_mod_section_difficulty_reduction" && element.Text == "Difficulty Reduction"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-title-LegacyLanguagePack_mod_section_difficulty_automation" && element.Text == "Automation"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-NF"), Is.True);
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-stat-score-value").Text, Is.EqualTo("1.00x"));
    }

    [Test]
    public void ToggleAddsSelectedChipAndUpdatesScoreMultiplier()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        scene.ToggleMod(0);
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(scene.SelectedAcronyms, Does.Contain("NF"));
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-selected-NF-text" && element.Text == "NF"), Is.True);
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

        firstScene.ToggleMod(0);
        var secondScene = new ModSelectScene(settings, new NoOpTextInputService());

        Assert.That(secondScene.SelectedAcronyms, Does.Contain("NF"));
    }


    [Test]
    public void LayoutKeepsLegacySectionWidthAndDoesNotShrinkRows()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "modselect-section-LegacyLanguagePack_mod_section_difficulty_reduction").Bounds.Width, Is.EqualTo(340f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-NF").Bounds.Height, Is.EqualTo(82f));
        Assert.That(frame.Elements.Single(element => element.Id == "modselect-toggle-description-NF").TextStyle!.Size, Is.EqualTo(16f));
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-section-LegacyLanguagePack_mod_section_fun"), Is.False);
    }

    [Test]
    public void HorizontalRailScrollRevealsOffscreenSections()
    {
        var scene = new ModSelectScene(new MemorySettingsStore(), new NoOpTextInputService());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Scroll(500f, 0f, new UiPoint(640f, 200f), viewport);
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

        scene.Scroll(0f, 500f, new UiPoint(430f, 200f), viewport);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot smallCircle = frame.Elements.Single(element => element.Id == "modselect-toggle-SC");
        Assert.That(smallCircle.Bounds.Bottom, Is.LessThanOrEqualTo(636f));
        Assert.That(frame.Elements.Any(element => element.Id == "modselect-toggle-HR"), Is.False);
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

        Assert.That(core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).Scene, Is.EqualTo("ModSelect"));

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
