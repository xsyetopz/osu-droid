using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime.Diagnostics;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private void AddBeatmapBackground(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (SelectedBackgroundPath is not { } backgroundPath)
        {
            elements.Add(
                new UiElementSnapshot(
                    "songselect-fallback-background",
                    UiElementKind.Sprite,
                    new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                    s_white,
                    1f,
                    DroidAssets.MenuBackground,
                    SpriteFit: UiSpriteFit.Cover
                )
            );
        }
        else
        {
            byte channel = (byte)
                Math.Clamp((int)MathF.Round(selectedBackgroundLuminance * 255f), 0, 255);
            elements.Add(
                new UiElementSnapshot(
                    "songselect-beatmap-background",
                    UiElementKind.Sprite,
                    new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                    UiColor.Opaque(channel, channel, channel),
                    1f,
                    ExternalAssetPath: backgroundPath,
                    SpriteFit: UiSpriteFit.Cover
                )
            );
        }
    }

    private void AddTopPanel(List<UiElementSnapshot> elements)
    {
        elements.Add(
            new UiElementSnapshot(
                "songselect-top-overlay",
                UiElementKind.Sprite,
                new UiRect(SongSelectTopX, 0f, SongSelectTopWidth, TopPanelHeight),
                s_white,
                0.6f,
                DroidAssets.SongSelectTop,
                SpriteFit: UiSpriteFit.Stretch
            )
        );

        BeatmapInfo? beatmap = SelectedBeatmap;
        if (beatmap is null)
        {
            elements.Add(
                Text(
                    "songselect-empty",
                    "There are no songs in library, try using the beatmap downloader.",
                    70f,
                    2f,
                    850f,
                    36f,
                    24f,
                    s_white
                )
            );
            return;
        }

        AddTopPanelText(elements, beatmap);
    }

    private void AddBeatmapRows(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        long start = PerfDiagnostics.Start();
        if (_visibleSnapshot.Sets.Count == 0)
        {
            return;
        }

        int visibleSlot = 0;
        float y = -scrollY;
        for (int setIndex = 0; setIndex < _visibleSnapshot.Sets.Count; setIndex++)
        {
            BeatmapSetInfo set = _visibleSnapshot.Sets[setIndex];
            BeatmapInfo? firstBeatmap = set.Beatmaps.FirstOrDefault();
            if (firstBeatmap is null)
            {
                continue;
            }

            float height =
                setIndex == selectedSetIndex ? CalculateSelectedSetHeight(set) : CollapsedRowHeight;
            float rowY = RowBaseY + y;
            float x = CalculateRowX(rowY + viewport.VirtualHeight * 0.5f + height * 0.5f, viewport);
            if (setIndex == selectedSetIndex)
            {
                AddSelectedDifficultyRows(elements, set, rowY, x, viewport);
            }
            else if (
                rowY > -RowHeight
                && rowY < viewport.VirtualHeight
                && visibleSlot < VisibleSetSlots
            )
            {
                UiAction action = SetAction(visibleSlot);
                _visibleSetIndices[visibleSlot] = setIndex;
                AddSetRow(
                    elements,
                    $"songselect-set-{visibleSlot}",
                    firstBeatmap,
                    new UiRect(x, rowY, RowWidth, RowHeight),
                    action
                );
                visibleSlot++;
            }

            y += height;
            if (
                rowY > viewport.VirtualHeight
                && setIndex > selectedSetIndex
                && visibleSlot >= VisibleSetSlots
            )
            {
                break;
            }
        }
        PerfDiagnostics.Log(
            "songSelect.addRows",
            start,
            $"visibleSlots={visibleSlot} sets={_visibleSnapshot.Sets.Count}"
        );
    }

    private void AddSelectedDifficultyRows(
        List<UiElementSnapshot> elements,
        BeatmapSetInfo set,
        float anchorY,
        float anchorX,
        VirtualViewport viewport
    )
    {
        BeatmapInfo[] beatmaps = set.Beatmaps.Take(VisibleDifficultySlots).ToArray();
        float y = anchorY;
        for (int index = 0; index < beatmaps.Length; index++)
        {
            BeatmapInfo beatmap = beatmaps[index];
            float centerY = y + viewport.VirtualHeight * 0.5f + RowHeight * 0.5f;
            float x =
                anchorX
                + 170f * MathF.Abs(MathF.Cos(centerY * MathF.PI / (viewport.VirtualHeight * 2f)))
                - 100f;
            bool isSelected = index == selectedDifficultyIndex;
            UiAction action = DifficultyAction(index);
            _visibleDifficultyIndices[index] = index;
            AddDifficultyRow(
                elements,
                $"songselect-diff-row-{index}",
                beatmap,
                new UiRect(x, y, RowWidth, RowHeight),
                isSelected,
                action
            );
            y += ExpandedRowSpacing * selectedSetExpansion;
        }
    }

    private void AddSetRow(
        List<UiElementSnapshot> elements,
        string id,
        BeatmapInfo beatmap,
        UiRect bounds,
        UiAction action
    )
    {
        elements.Add(
            Sprite(id, DroidAssets.SongSelectButtonBackground, bounds, s_setRowTint, 0.8f, action)
        );
        elements.Add(
            Text(
                $"{id}-title",
                $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}",
                bounds.X + 32f,
                bounds.Y + 25f,
                620f,
                34f,
                24f,
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
        elements.Add(
            Text(
                $"{id}-creator",
                _localizer.Format("SongSelect_Creator", beatmap.Creator),
                bounds.X + 150f,
                bounds.Y + 60f,
                500f,
                28f,
                20f,
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
    }

    private void AddDifficultyRow(
        List<UiElementSnapshot> elements,
        string id,
        BeatmapInfo beatmap,
        UiRect bounds,
        bool isSelected,
        UiAction action
    )
    {
        UiColor tint = isSelected ? s_selectedRowTint : s_difficultyRowTint;
        UiColor textColor = isSelected ? s_black : s_white;
        elements.Add(
            Sprite(id, DroidAssets.SongSelectButtonBackground, bounds, tint, 0.8f, action)
        );
        elements.Add(
            Text(
                $"{id}-title",
                $"{beatmap.Version} ({beatmap.Creator})",
                bounds.X + 32f,
                bounds.Y + 22f,
                540f,
                34f,
                24f,
                textColor,
                UiTextAlignment.Left,
                action
            )
        );

        // Android source: menu/BeatmapItem.java.
        // Fractional stars are scaled around AndEngine's default center, not cropped.
        float stars = Math.Clamp(CurrentStarRating(beatmap) ?? 0f, 0f, 10f);
        int fullStars = Math.Min(10, (int)MathF.Floor(stars));
        float starY = bounds.Y + 50f;
        for (int star = 0; star < fullStars; star++)
        {
            elements.Add(
                Sprite(
                    $"{id}-star-{star}",
                    DroidAssets.SongSelectStar,
                    new UiRect(bounds.X + 60f + star * 52f, starY, 46f, 47f),
                    s_white,
                    1f,
                    action
                )
            );
        }

        float fraction = stars - fullStars;
        if (fraction > 0f && fullStars < 10)
        {
            const float starWidth = 46f;
            const float starHeight = 47f;
            float slotX = bounds.X + 60f + fullStars * 52f;
            float scaledWidth = starWidth * fraction;
            float scaledHeight = starHeight * fraction;
            elements.Add(
                Sprite(
                    $"{id}-star-half",
                    DroidAssets.SongSelectStar,
                    new UiRect(
                        slotX + (starWidth - scaledWidth) / 2f,
                        starY + (starHeight - scaledHeight) / 2f,
                        scaledWidth,
                        scaledHeight
                    ),
                    s_white,
                    1f,
                    action
                )
            );
        }
    }
}
