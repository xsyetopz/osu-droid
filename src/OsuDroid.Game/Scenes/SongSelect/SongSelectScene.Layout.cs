using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private UiFrameSnapshot CreateFrame(VirtualViewport viewport)
    {
        var start = PerfDiagnostics.Start();
        Array.Fill(visibleSetActions, -1);
        Array.Fill(visibleDifficultyActions, -1);
        Array.Fill(visibleCollectionActions, -1);

        var elements = new List<UiElementSnapshot>
        {
            Fill("songselect-base", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), Black),
        };

        AddBeatmapBackground(elements, viewport);
        elements.Add(Fill("songselect-dim", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), BackgroundShade));
        AddBeatmapRows(elements, viewport);
        AddTopPanel(elements, viewport);
        AddBottomControls(elements, viewport);
        AddScorePreview(elements, viewport);
        AddModal(elements, viewport);

        var frame = new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
        PerfDiagnostics.Log("songSelect.createFrame", start, $"elements={elements.Count} sets={visibleSnapshot.Sets.Count}");
        return frame;
    }



    private static void AddBottomControls(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var backY = viewport.VirtualHeight - BackButtonSize;
        elements.Add(Sprite("songselect-back", DroidAssets.SongSelectBack, new UiRect(0f, backY, BackButtonSize, BackButtonSize), White, 1f, UiAction.SongSelectBack));

        var smallY = viewport.VirtualHeight - SmallButtonSize;
        var modsX = BackButtonSize;
        elements.Add(Sprite("songselect-mods", DroidAssets.SongSelectMods, new UiRect(modsX, smallY, SmallButtonSize, SmallButtonSize), White, 1f, UiAction.SongSelectMods));
        elements.Add(Sprite("songselect-options", DroidAssets.SongSelectOptions, new UiRect(modsX + SmallButtonSize, smallY, SmallButtonSize, SmallButtonSize), White, 1f, UiAction.SongSelectBeatmapOptions));
        elements.Add(Sprite("songselect-random", DroidAssets.SongSelectRandom, new UiRect(modsX + SmallButtonSize * 2f, smallY, SmallButtonSize, SmallButtonSize), White, 1f, UiAction.SongSelectRandom));
    }

    private void AddScorePreview(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(Sprite("songselect-scoring-switcher", DroidAssets.RankingDisabled, new UiRect(10f, 10f, 50f, 50f), White, 1f));

        var panelX = BackButtonSize + SmallButtonSize * 3f + OnlinePanelGap;
        var panelY = viewport.VirtualHeight - OnlinePanelHeight;
        OnlineProfilePanelSnapshots.Add(
            elements,
            "songselect-score",
            new UiRect(panelX, panelY, OnlinePanelWidth, OnlinePanelHeight),
            OnlineAvatarFooterSize,
            profile);
    }

    private void AddTopPanelText(List<UiElementSnapshot> elements, BeatmapInfo beatmap)
    {
        var titleY = 2f;
        elements.Add(Text("songselect-title", $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)} [{beatmap.Version}]", 70f, titleY, 1024f, 32f, 24f, White));

        var creatorY = titleY + 32f + 2f;
        elements.Add(Text("songselect-creator", $"Beatmap by {beatmap.Creator}", 70f, creatorY, 1024f, 26f, 20f, White));

        var lengthY = creatorY + 26f + 2f;
        elements.Add(Text("songselect-length", FormatLengthLine(beatmap), 4f, lengthY, 1024f, 26f, 18f, White));

        var objectsY = lengthY + 26f + 2f;
        elements.Add(Text("songselect-objects", FormatObjectLine(beatmap), 4f, objectsY, 1120f, 26f, 18f, White));

        var difficultyY = objectsY + 26f + 2f;
        elements.Add(Text("songselect-difficulty", FormatDifficultyLine(beatmap), 4f, difficultyY, 1024f, 24f, 18f, White));
    }
}
