using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private static void AddCompoundButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, UiMaterialIcon leadingIcon, UiMaterialIcon? trailingIcon, UiColor background, float radius)
    {
        elements.Add(Fill(id + "-hit", bounds, background, 1f, action, radius));
        var textWidth = EstimateTextWidth(text, 14f * Dp);
        var trailingWidth = trailingIcon is null ? 0f : 8f * Dp + 24f * Dp;
        var contentWidth = 24f * Dp + 8f * Dp + textWidth + trailingWidth;
        var x = bounds.X + (bounds.Width - contentWidth) / 2f;
        var iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(MaterialIcon(id + "-icon", leadingIcon, new UiRect(x, iconY, 24f * Dp, 24f * Dp), White, 1f, action));
        elements.Add(TextMiddle(id + "-text", text, x + 32f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
        if (trailingIcon is not null)
            elements.Add(MaterialIcon(id + "-trailing", trailingIcon.Value, new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp), White, 1f, action));
    }

    private static void AddCompoundSpriteButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, string assetName, UiAction action, UiMaterialIcon trailingIcon)
    {
        elements.Add(Fill(id + "-hit", bounds, AppBar, 0f, action, 0f));
        var textWidth = EstimateTextWidth(text, 14f * Dp);
        var contentWidth = 24f * Dp + 8f * Dp + textWidth + 8f * Dp + 24f * Dp;
        var x = bounds.X + (bounds.Width - contentWidth) / 2f;
        var iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(new UiElementSnapshot(id + "-logo", UiElementKind.Sprite, new UiRect(x, iconY, 24f * Dp, 24f * Dp), White, 1f, assetName, action));
        elements.Add(TextMiddle(id + "-text", text, x + 32f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
        elements.Add(MaterialIcon(id + "-caret", trailingIcon, new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp), White, 1f, action));
    }

    private static void AddButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, Field, 1f, action, Radius));
        elements.Add(TextMiddle(id + "-text", text, bounds.X + 4f * Dp, bounds.Y, bounds.Width - 8f * Dp, bounds.Height, MathF.Min(14f * Dp, bounds.Height * 0.45f), White, UiTextAlignment.Center, action));
    }

    private static void AddDropdownButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, Field, 1f, action, Radius));
        var textWidth = MathF.Min(bounds.Width - 48f * Dp, MathF.Max(40f * Dp, EstimateTextWidth(text, 14f * Dp)));
        elements.Add(TextMiddle(id + "-text", text, bounds.X + (bounds.Width - textWidth) / 2f - 8f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Center, action));
        elements.Add(MaterialIcon(id + "-caret", UiMaterialIcon.ArrowDropDown, new UiRect(bounds.Right - 34f * Dp, bounds.Y + 9f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
    }

    private static void AddButtonGroup(List<UiElementSnapshot> elements, List<DownloaderButtonSpec> buttons, float centerX, float y)
    {
        var totalWidth = buttons.Sum(button => button.Width) + Math.Max(0, buttons.Count - 1) * 8f * Dp;
        var x = centerX - totalWidth / 2f;
        foreach (var button in buttons)
        {
            AddIconButton(elements, button.Id, new UiRect(x, y, button.Width, 36f * Dp), button.Icon, button.Text, button.Action);
            x += button.Width + 8f * Dp;
        }
    }

    private static void AddIconButton(List<UiElementSnapshot> elements, string id, UiRect bounds, UiMaterialIcon icon, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, Field, 1f, action, Radius));
        var textWidth = string.IsNullOrEmpty(text) ? 0f : MathF.Max(48f * Dp, text.Length * 7f * Dp);
        var contentWidth = 24f * Dp + (textWidth > 0f ? 6f * Dp + textWidth : 0f);
        var iconX = bounds.X + (bounds.Width - contentWidth) / 2f;
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(iconX, bounds.Y + 6f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
        if (textWidth > 0f)
            elements.Add(TextMiddle(id + "-text", text, iconX + 30f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
    }

    private static void AddStatusPill(List<UiElementSnapshot> elements, string id, BeatmapRankedStatus status, float x, float y, UiAction action)
    {
        var width = StatusPillWidth(status);
        var bounds = new UiRect(x, y, width, 20f * Dp);
        elements.Add(Fill(id + "-bg", bounds, StatusPillColor(status), 1f, action, Radius));
        elements.Add(TextMiddle(id, RankedStatusText(status), bounds.X + 12f * Dp, bounds.Y, bounds.Width - 24f * Dp, bounds.Height, 10f * Dp, White, UiTextAlignment.Center, action));
    }

    private static float StatusPillWidth(BeatmapRankedStatus status) => Math.Max(58f * Dp, EstimateTextWidth(RankedStatusText(status), 10f * Dp) + 24f * Dp);

    private static UiColor StatusPillColor(BeatmapRankedStatus status) => Panel;

    private static void AddDifficultyDots(
        List<UiElementSnapshot> elements,
        string id,
        IReadOnlyList<BeatmapMirrorBeatmap> beatmaps,
        float x,
        float y,
        float width,
        UiAction action,
        int? selectedIndex,
        bool isCentered,
        UiAction fallbackAction)
    {
        const string glyph = "⦿";
        const float cardCellWidth = 32f * Dp;
        const float detailsCellWidth = 56f * Dp;
        const float rowHeight = DifficultyGlyphRowHeight;
        const float margin = 6f * Dp;
        var count = Math.Min(beatmaps.Count, 16);
        if (count == 0)
            return;

        var hasSelection = selectedIndex is not null;
        var gap = hasSelection ? margin : 0f;
        var preferredCellWidth = hasSelection ? detailsCellWidth : cardCellWidth;
        var availableWidth = Math.Max(0f, width - Math.Max(0, count - 1) * gap);
        var cellWidth = MathF.Min(preferredCellWidth, availableWidth / count);
        var glyphSize = MathF.Min(24f * Dp, MathF.Max(14f * Dp, cellWidth * 0.75f));
        var totalWidth = count * cellWidth + Math.Max(0, count - 1) * gap;
        var startX = isCentered ? x + (width - totalWidth) / 2f : x;
        var cursorX = startX;
        for (var i = 0; i < count; i++)
        {
            var dotAction = hasSelection ? DifficultyAction(i) : fallbackAction;
            var selected = selectedIndex == i;
            var hitBounds = new UiRect(cursorX, y, cellWidth, rowHeight);
            if (selected)
                elements.Add(Fill($"{id}-{i}-selected", hitBounds, Field, 1f, dotAction, Radius));

            elements.Add(TextMiddle(
                $"{id}-{i}",
                glyph,
                hitBounds.X,
                hitBounds.Y,
                hitBounds.Width,
                hitBounds.Height,
                glyphSize,
                StarRatingColor(beatmaps[i].StarRating),
                UiTextAlignment.Center,
                dotAction));
            cursorX += cellWidth + gap;
        }
    }

    private static UiColor StarRatingColor(float starRating)
    {
        var points = new (float Rating, UiColor Color)[]
        {
            (0.1f, UiColor.Opaque(170, 170, 170)),
            (0.1f, UiColor.Opaque(66, 144, 251)),
            (1.25f, UiColor.Opaque(79, 192, 255)),
            (2.0f, UiColor.Opaque(79, 255, 213)),
            (2.5f, UiColor.Opaque(124, 255, 79)),
            (3.3f, UiColor.Opaque(246, 240, 92)),
            (4.2f, UiColor.Opaque(255, 128, 104)),
            (4.9f, UiColor.Opaque(255, 78, 111)),
            (5.8f, UiColor.Opaque(198, 69, 184)),
            (6.7f, UiColor.Opaque(101, 99, 222)),
            (7.7f, UiColor.Opaque(24, 21, 142)),
            (9.0f, UiColor.Opaque(0, 0, 0)),
        };
        var rounded = MathF.Ceiling(starRating * 100f) / 100f;
        if (rounded < 0.1f)
            return UiColor.Opaque(170, 170, 170);
        for (var i = 0; i < points.Length - 1; i++)
        {
            var current = points[i];
            var next = points[i + 1];
            if (rounded > next.Rating)
                continue;

            var amount = Math.Clamp((rounded - current.Rating) / Math.Max(0.001f, next.Rating - current.Rating), 0f, 1f);
            return new UiColor(
                (byte)MathF.Round(current.Color.Red + (next.Color.Red - current.Color.Red) * amount),
                (byte)MathF.Round(current.Color.Green + (next.Color.Green - current.Color.Green) * amount),
                (byte)MathF.Round(current.Color.Blue + (next.Color.Blue - current.Color.Blue) * amount),
                255);
        }
        return points[^1].Color;
    }

    private static float EstimateTextWidth(string text, float size) => MathF.Max(1f, text.Length * size * 0.55f);

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) => new(id, UiElementKind.Fill, bounds, color, alpha, Action: action, CornerRadius: radius);

    private static UiElementSnapshot Text(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) => new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment));

    private static UiElementSnapshot TextMiddle(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) => new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment, false, UiTextVerticalAlignment.Middle));

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) => new(
        id,
        UiElementKind.MaterialIcon,
        bounds,
        color,
        alpha,
        Action: action,
        MaterialIcon: icon);

    private static string DifficultyDots(BeatmapMirrorSet set) => string.Join(' ', set.Beatmaps.Select(_ => "⦿"));

    private static string RankedStatusText(BeatmapRankedStatus rankedStatus) => rankedStatus switch
    {
        BeatmapRankedStatus.Ranked => "Ranked",
        BeatmapRankedStatus.Approved => "Approved",
        BeatmapRankedStatus.Qualified => "Qualified",
        BeatmapRankedStatus.Loved => "Loved",
        BeatmapRankedStatus.WorkInProgress => "WIP",
        BeatmapRankedStatus.Graveyard => "Graveyard",
        _ => "Pending",
    };

    private static string SortText(BeatmapMirrorSort value) => value switch
    {
        BeatmapMirrorSort.Bpm => "BPM",
        BeatmapMirrorSort.DifficultyRating => "Difficulty rating",
        BeatmapMirrorSort.HitLength => "Hit length",
        BeatmapMirrorSort.PassCount => "Pass count",
        BeatmapMirrorSort.PlayCount => "Play count",
        BeatmapMirrorSort.TotalLength => "Total length",
        BeatmapMirrorSort.FavouriteCount => "Favourite count",
        BeatmapMirrorSort.LastUpdated => "Last updated",
        BeatmapMirrorSort.RankedDate => "Ranked date",
        BeatmapMirrorSort.SubmittedDate => "Submitted date",
        _ => value.ToString(),
    };

    private static BeatmapMirrorSort Next(BeatmapMirrorSort value)
    {
        var values = Enum.GetValues<BeatmapMirrorSort>();
        return values[(Array.IndexOf(values, value) + 1) % values.Length];
    }

    private static string FormatDownloadInfo(BeatmapDownloadProgress? progress)
    {
        if (progress is null)
            return string.Empty;

        var percent = progress.Percent is double p ? $" ({p:0}%)" : string.Empty;
        var speed = progress.SpeedBytesPerSecond > 0 ? $"\n{progress.SpeedBytesPerSecond / 1024d / 1024d:0.###} mb/s{percent}" : percent;
        return speed;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception)
        {
        }
    }
}
