using OsuDroid.Game.Beatmaps;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private UiFrameSnapshot CreateFrame(VirtualViewport viewport)
    {
        long start = PerfDiagnostics.Start();
        Array.Fill(_visibleSetIndices, -1);
        Array.Fill(_visibleDifficultyIndices, -1);
        Array.Fill(_visibleCollectionIndices, -1);

        var elements = new List<UiElementSnapshot>
        {
            Fill("songselect-base", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_black),
        };

        AddBeatmapBackground(elements, viewport);
        elements.Add(Fill("songselect-dim", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_backgroundShade));
        AddBeatmapRows(elements, viewport);
        AddTopPanel(elements);
        AddBottomControls(elements, viewport);
        AddScorePreview(elements, viewport);
        AddModal(elements, viewport);

        var frame = new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
        PerfDiagnostics.Log("songSelect.createFrame", start, $"elements={elements.Count} sets={_visibleSnapshot.Sets.Count}");
        return frame;
    }



    private static void AddBottomControls(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float backY = viewport.VirtualHeight - BackButtonSize;
        elements.Add(Sprite("songselect-back", DroidAssets.SongSelectBack, new UiRect(0f, backY, BackButtonSize, BackButtonSize), s_white, 1f, UiAction.SongSelectBack));

        float smallY = viewport.VirtualHeight - SmallButtonSize;
        float modsX = BackButtonSize;
        elements.Add(Sprite("songselect-mods", DroidAssets.SongSelectMods, new UiRect(modsX, smallY, SmallButtonSize, SmallButtonSize), s_white, 1f, UiAction.SongSelectMods));
        elements.Add(Sprite("songselect-options", DroidAssets.SongSelectOptions, new UiRect(modsX + SmallButtonSize, smallY, SmallButtonSize, SmallButtonSize), s_white, 1f, UiAction.SongSelectBeatmapOptions));
        elements.Add(Sprite("songselect-random", DroidAssets.SongSelectRandom, new UiRect(modsX + SmallButtonSize * 2f, smallY, SmallButtonSize, SmallButtonSize), s_white, 1f, UiAction.SongSelectRandom));
    }

    private void AddScorePreview(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(Sprite("songselect-scoring-switcher", DroidAssets.RankingDisabled, new UiRect(10f, 10f, 50f, 50f), s_white, 1f));
        if (_onlinePanelState is null)
        {
            return;
        }

        float panelX = BackButtonSize + SmallButtonSize * 3f + OnlinePanelGap;
        float panelY = viewport.VirtualHeight - OnlinePanelHeight;
        OnlineProfilePanelSnapshots.Add(
            elements,
            "songselect-score",
            new UiRect(panelX, panelY, OnlinePanelWidth, OnlinePanelHeight),
            OnlineAvatarFooterSize,
            _onlinePanelState);
    }

    private void AddTopPanelText(List<UiElementSnapshot> elements, BeatmapInfo beatmap)
    {
        float titleY = 2f;
        elements.Add(Text("songselect-title", $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)} [{beatmap.Version}]", 70f, titleY, 1024f, 32f, 24f, s_white));

        float creatorY = titleY + 32f + 2f;
        elements.Add(Text("songselect-creator", _localizer.Format("SongSelect_BeatmapBy", beatmap.Creator), 70f, creatorY, 1024f, 26f, 20f, s_white));

        float lengthY = creatorY + 26f + 2f;
        elements.Add(Text("songselect-length", FormatLengthLine(beatmap), 4f, lengthY, 1024f, 26f, 18f, s_white));

        float objectsY = lengthY + 26f + 2f;
        elements.Add(Text("songselect-objects", FormatObjectLine(beatmap), 4f, objectsY, 1120f, 26f, 18f, s_white));

        float difficultyY = objectsY + 26f + 2f;
        elements.Add(Text("songselect-difficulty", FormatDifficultyLine(beatmap), 4f, difficultyY, 1024f, 24f, 18f, s_white));
    }
}
