using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed class SongSelectScene(IBeatmapLibrary library, IBeatmapPreviewPlayer initialPreviewPlayer, string songsPath)
{
    private static readonly UiColor Background = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor AppBar = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor Panel = UiColor.Opaque(22, 22, 34);
    private static readonly UiColor Selected = UiColor.Opaque(67, 67, 105);
    private static readonly UiColor Accent = UiColor.Opaque(243, 115, 115);
    private static readonly UiColor White = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor Secondary = UiColor.Opaque(190, 190, 215);

    private BeatmapLibrarySnapshot snapshot = BeatmapLibrarySnapshot.Empty;
    private IBeatmapPreviewPlayer previewPlayer = initialPreviewPlayer;
    private BeatmapInfo? selectedBeatmap;

    public BeatmapInfo? SelectedBeatmap => selectedBeatmap;

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player) => previewPlayer = player;

    public void Enter(string? preferredSetDirectory = null)
    {
        snapshot = library.Scan();
        selectedBeatmap = SelectInitialBeatmap(preferredSetDirectory);
        PlaySelectedPreview();
    }

    public void Leave() => previewPlayer.StopPreview();

    public void SelectSet(int index)
    {
        if (index < 0 || index >= snapshot.Sets.Count)
            return;

        selectedBeatmap = snapshot.Sets[index].Beatmaps.FirstOrDefault();
        PlaySelectedPreview();
    }

    public void SelectFirstSet() => SelectSet(0);

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new("SongSelect", "Song Select", string.Empty, Array.Empty<string>(), 0, false, CreateFrame(viewport));


    public static int SetIndex(UiAction action) => action switch
    {
        UiAction.SongSelectSet0 or UiAction.SongSelectFirstSet => 0,
        UiAction.SongSelectSet1 => 1,
        UiAction.SongSelectSet2 => 2,
        UiAction.SongSelectSet3 => 3,
        UiAction.SongSelectSet4 => 4,
        UiAction.SongSelectSet5 => 5,
        UiAction.SongSelectSet6 => 6,
        UiAction.SongSelectSet7 => 7,
        _ => -1,
    };

    private static UiAction SetAction(int index) => index switch
    {
        0 => UiAction.SongSelectSet0,
        1 => UiAction.SongSelectSet1,
        2 => UiAction.SongSelectSet2,
        3 => UiAction.SongSelectSet3,
        4 => UiAction.SongSelectSet4,
        5 => UiAction.SongSelectSet5,
        6 => UiAction.SongSelectSet6,
        7 => UiAction.SongSelectSet7,
        _ => UiAction.None,
    };

    private UiFrameSnapshot CreateFrame(VirtualViewport viewport)
    {
        var elements = new List<UiElementSnapshot>
        {
            Fill("songselect-background", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), Background),
            Fill("songselect-appbar", new UiRect(0f, 0f, viewport.VirtualWidth, 92f), AppBar),
            Fill("songselect-back-hit", new UiRect(0f, 0f, 92f, 92f), Selected, 1f, UiAction.SongSelectBack),
            Text("songselect-back", "‹", 30f, 6f, 52f, 72f, 58f, White, UiTextAlignment.Center, UiAction.SongSelectBack),
            Text("songselect-title", "Song Select", 120f, 24f, 400f, 42f, 30f, White),
        };

        if (selectedBeatmap?.GetBackgroundPath(songsPath) is { } backgroundPath && File.Exists(backgroundPath))
            elements.Add(new UiElementSnapshot("songselect-beatmap-background", UiElementKind.Sprite, new UiRect(0f, 92f, viewport.VirtualWidth, viewport.VirtualHeight - 92f), White, 0.32f, ExternalAssetPath: backgroundPath));

        var listX = 56f;
        var listY = 128f;
        var rowWidth = 470f;
        var rowHeight = 78f;
        var count = Math.Min(snapshot.Sets.Count, 7);

        for (var index = 0; index < count; index++)
        {
            var set = snapshot.Sets[index];
            var beatmap = set.Beatmaps.FirstOrDefault();
            if (beatmap is null)
                continue;

            var y = listY + index * (rowHeight + 10f);
            var isSelected = selectedBeatmap?.SetDirectory == set.Directory;
            var action = SetAction(index);
            elements.Add(Fill($"songselect-set-{index}", new UiRect(listX, y, rowWidth, rowHeight), isSelected ? Selected : Panel, 1f, action, 10f));
            elements.Add(Text($"songselect-set-{index}-title", beatmap.Title, listX + 18f, y + 10f, rowWidth - 36f, 28f, 22f, White, UiTextAlignment.Left, action));
            elements.Add(Text($"songselect-set-{index}-artist", beatmap.Artist, listX + 18f, y + 42f, rowWidth - 36f, 22f, 17f, Secondary, UiTextAlignment.Left, action));
        }

        AddSelectedDetails(elements, viewport);
        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private void AddSelectedDetails(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var panel = new UiRect(560f, 128f, viewport.VirtualWidth - 620f, 404f);
        elements.Add(Fill("songselect-detail-panel", panel, Panel, 0.94f, UiAction.None, 14f));

        if (selectedBeatmap is null)
        {
            elements.Add(Text("songselect-empty", "No beatmaps found. Use Download Beatmaps first.", panel.X + 30f, panel.Y + 34f, panel.Width - 60f, 42f, 24f, White));
            return;
        }

        elements.Add(Text("songselect-detail-title", selectedBeatmap.Title, panel.X + 30f, panel.Y + 28f, panel.Width - 60f, 44f, 32f, White));
        elements.Add(Text("songselect-detail-artist", selectedBeatmap.Artist, panel.X + 30f, panel.Y + 78f, panel.Width - 60f, 34f, 24f, Secondary));
        elements.Add(Text("songselect-detail-creator", $"mapped by {selectedBeatmap.Creator}", panel.X + 30f, panel.Y + 122f, panel.Width - 60f, 28f, 20f, Secondary));
        elements.Add(Text("songselect-detail-version", $"[{selectedBeatmap.Version}]", panel.X + 30f, panel.Y + 166f, panel.Width - 60f, 34f, 24f, White));
        elements.Add(Text("songselect-detail-stats", $"AR {selectedBeatmap.ApproachRate:0.#}  OD {selectedBeatmap.OverallDifficulty:0.#}  CS {selectedBeatmap.CircleSize:0.#}  HP {selectedBeatmap.HpDrainRate:0.#}", panel.X + 30f, panel.Y + 216f, panel.Width - 60f, 28f, 20f, Secondary));
        elements.Add(Text("songselect-detail-bpm", $"BPM {selectedBeatmap.MostCommonBpm:0.#}  Length {TimeSpan.FromMilliseconds(selectedBeatmap.Length):m\\:ss}", panel.X + 30f, panel.Y + 256f, panel.Width - 60f, 28f, 20f, Secondary));
        elements.Add(Fill("songselect-play-disabled", new UiRect(panel.Right - 190f, panel.Bottom - 74f, 160f, 46f), Accent, 0.55f, UiAction.None, 10f));
        elements.Add(Text("songselect-play-disabled-text", "Play later", panel.Right - 190f, panel.Bottom - 64f, 160f, 26f, 20f, White, UiTextAlignment.Center));
    }

    private BeatmapInfo? SelectInitialBeatmap(string? preferredSetDirectory)
    {
        if (preferredSetDirectory is not null)
        {
            var preferred = snapshot.Sets.FirstOrDefault(set => string.Equals(set.Directory, preferredSetDirectory, StringComparison.Ordinal));
            if (preferred?.Beatmaps.Count > 0)
                return preferred.Beatmaps[0];
        }

        return snapshot.Sets.FirstOrDefault()?.Beatmaps.FirstOrDefault();
    }

    private void PlaySelectedPreview()
    {
        if (selectedBeatmap is null)
            return;

        var audioPath = selectedBeatmap.GetAudioPath(songsPath);
        if (File.Exists(audioPath))
            previewPlayer.Play(audioPath, selectedBeatmap.PreviewTime);
    }

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) => new(id, UiElementKind.Fill, bounds, color, alpha, Action: action, CornerRadius: radius);

    private static UiElementSnapshot Text(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) => new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment));
}
