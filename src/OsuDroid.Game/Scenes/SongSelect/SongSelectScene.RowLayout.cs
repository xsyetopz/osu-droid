using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private void AddBeatmapBackground(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (SelectedBackgroundPath is not { } backgroundPath)
        {
            elements.Add(new UiElementSnapshot("songselect-fallback-background", UiElementKind.Sprite, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), White, 1f, DroidAssets.MenuBackground, SpriteFit: UiSpriteFit.Cover));
        }
        else
        {
            var channel = (byte)Math.Clamp((int)MathF.Round(selectedBackgroundLuminance * 255f), 0, 255);
            elements.Add(new UiElementSnapshot("songselect-beatmap-background", UiElementKind.Sprite, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), UiColor.Opaque(channel, channel, channel), 1f, ExternalAssetPath: backgroundPath, SpriteFit: UiSpriteFit.Cover));
        }
    }

    private void AddTopPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(new UiElementSnapshot(
            "songselect-top-overlay",
            UiElementKind.Sprite,
            new UiRect(SongSelectTopX, 0f, SongSelectTopWidth, TopPanelHeight),
            White,
            0.6f,
            DroidAssets.SongSelectTop,
            SpriteFit: UiSpriteFit.Stretch));

        var beatmap = SelectedBeatmap;
        if (beatmap is null)
        {
            elements.Add(Text("songselect-empty", "There are no songs in library, try using the beatmap downloader.", 70f, 2f, 850f, 36f, 24f, White));
            return;
        }

        AddTopPanelText(elements, beatmap);
    }

    private void AddBeatmapRows(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var start = PerfDiagnostics.Start();
        if (visibleSnapshot.Sets.Count == 0)
            return;

        var visibleSlot = 0;
        var y = -scrollY;
        for (var setIndex = 0; setIndex < visibleSnapshot.Sets.Count; setIndex++)
        {
            var set = visibleSnapshot.Sets[setIndex];
            var firstBeatmap = set.Beatmaps.FirstOrDefault();
            if (firstBeatmap is null)
                continue;

            var height = setIndex == selectedSetIndex
                ? CalculateSelectedSetHeight(set)
                : CollapsedRowHeight;
            var rowY = RowBaseY + y;
            var x = CalculateRowX(rowY + viewport.VirtualHeight * 0.5f + height * 0.5f, viewport);
            if (setIndex == selectedSetIndex)
            {
                AddSelectedDifficultyRows(elements, set, rowY, x, viewport);
            }
            else if (rowY > -RowHeight && rowY < viewport.VirtualHeight && visibleSlot < VisibleSetSlots)
            {
                var action = SetAction(visibleSlot);
                visibleSetIndices[visibleSlot] = setIndex;
                AddSetRow(elements, $"songselect-set-{visibleSlot}", firstBeatmap, new UiRect(x, rowY, RowWidth, RowHeight), action);
                visibleSlot++;
            }

            y += height;
            if (rowY > viewport.VirtualHeight && setIndex > selectedSetIndex && visibleSlot >= VisibleSetSlots)
                break;
        }
        PerfDiagnostics.Log("songSelect.addRows", start, $"visibleSlots={visibleSlot} sets={visibleSnapshot.Sets.Count}");
    }

    private void AddSelectedDifficultyRows(List<UiElementSnapshot> elements, BeatmapSetInfo set, float anchorY, float anchorX, VirtualViewport viewport)
    {
        var beatmaps = set.Beatmaps.Take(VisibleDifficultySlots).ToArray();
        var y = anchorY;
        for (var index = 0; index < beatmaps.Length; index++)
        {
            var beatmap = beatmaps[index];
            var centerY = y + viewport.VirtualHeight * 0.5f + RowHeight * 0.5f;
            var x = anchorX + 170f * MathF.Abs(MathF.Cos(centerY * MathF.PI / (viewport.VirtualHeight * 2f))) - 100f;
            var isSelected = index == selectedDifficultyIndex;
            var action = DifficultyAction(index);
            visibleDifficultyIndices[index] = index;
            AddDifficultyRow(elements, $"songselect-diff-row-{index}", beatmap, new UiRect(x, y, RowWidth, RowHeight), isSelected, action);
            y += ExpandedRowSpacing * selectedSetExpansion;
        }
    }

    private void AddSetRow(List<UiElementSnapshot> elements, string id, BeatmapInfo beatmap, UiRect bounds, UiAction action)
    {
        elements.Add(Sprite(id, DroidAssets.SongSelectButtonBackground, bounds, SetRowTint, 0.8f, action));
        elements.Add(Text($"{id}-title", $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}", bounds.X + 32f, bounds.Y + 25f, 620f, 34f, 24f, White, UiTextAlignment.Left, action));
        elements.Add(Text($"{id}-creator", $"Creator: {beatmap.Creator}", bounds.X + 150f, bounds.Y + 60f, 500f, 28f, 20f, White, UiTextAlignment.Left, action));
    }

    private void AddDifficultyRow(List<UiElementSnapshot> elements, string id, BeatmapInfo beatmap, UiRect bounds, bool isSelected, UiAction action)
    {
        var tint = isSelected ? SelectedRowTint : DifficultyRowTint;
        var textColor = isSelected ? Black : White;
        elements.Add(Sprite(id, DroidAssets.SongSelectButtonBackground, bounds, tint, 0.8f, action));
        elements.Add(Text($"{id}-title", $"{beatmap.Version} ({beatmap.Creator})", bounds.X + 32f, bounds.Y + 22f, 540f, 34f, 24f, textColor, UiTextAlignment.Left, action));

        // Legacy source: third_party/osu-droid-legacy/.../menu/BeatmapItem.java.
        // Fractional stars are scaled around AndEngine's default center, not cropped.
        var stars = Math.Clamp(CurrentStarRating(beatmap) ?? 0f, 0f, 10f);
        var fullStars = Math.Min(10, (int)MathF.Floor(stars));
        var starY = bounds.Y + 50f;
        for (var star = 0; star < fullStars; star++)
            elements.Add(Sprite($"{id}-star-{star}", DroidAssets.SongSelectStar, new UiRect(bounds.X + 60f + star * 52f, starY, 46f, 47f), White, 1f, action));

        var fraction = stars - fullStars;
        if (fraction > 0f && fullStars < 10)
        {
            const float starWidth = 46f;
            const float starHeight = 47f;
            var slotX = bounds.X + 60f + fullStars * 52f;
            var scaledWidth = starWidth * fraction;
            var scaledHeight = starHeight * fraction;
            elements.Add(Sprite(
                $"{id}-star-half",
                DroidAssets.SongSelectStar,
                new UiRect(
                    slotX + (starWidth - scaledWidth) / 2f,
                    starY + (starHeight - scaledHeight) / 2f,
                    scaledWidth,
                    scaledHeight),
                White,
                1f,
                action));
        }
    }
}
