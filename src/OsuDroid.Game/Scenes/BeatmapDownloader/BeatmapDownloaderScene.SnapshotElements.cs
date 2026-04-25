using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
#pragma warning disable IDE0072 // Mirror API enums default to pending/title labels for unknown values.
    private static void AddCompoundButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, UiMaterialIcon leadingIcon, UiMaterialIcon? trailingIcon, UiColor background, float radius)
    {
        elements.Add(Fill(id + "-hit", bounds, background, 1f, action, radius));
        float textWidth = EstimateTextWidth(text, 14f * Dp);
        float trailingWidth = trailingIcon is null ? 0f : 8f * Dp + 24f * Dp;
        float contentWidth = 24f * Dp + 8f * Dp + textWidth + trailingWidth;
        float x = bounds.X + (bounds.Width - contentWidth) / 2f;
        float iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(MaterialIcon(id + "-icon", leadingIcon, new UiRect(x, iconY, 24f * Dp, 24f * Dp), s_white, 1f, action));
        elements.Add(TextMiddle(id + "-text", text, x + 32f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, s_white, UiTextAlignment.Left, action));
        if (trailingIcon is not null)
        {
            elements.Add(MaterialIcon(id + "-trailing", trailingIcon.Value, new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp), s_white, 1f, action));
        }
    }

    private static void AddCompoundSpriteButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, string assetName, UiAction action, UiMaterialIcon trailingIcon)
    {
        elements.Add(Fill(id + "-hit", bounds, s_appBar, 0f, action, 0f));
        float textWidth = EstimateTextWidth(text, 14f * Dp);
        float contentWidth = 24f * Dp + 8f * Dp + textWidth + 8f * Dp + 24f * Dp;
        float x = bounds.X + (bounds.Width - contentWidth) / 2f;
        float iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(new UiElementSnapshot(id + "-logo", UiElementKind.Sprite, new UiRect(x, iconY, 24f * Dp, 24f * Dp), s_white, 1f, assetName, action));
        elements.Add(TextMiddle(id + "-text", text, x + 32f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, s_white, UiTextAlignment.Left, action));
        elements.Add(MaterialIcon(id + "-caret", trailingIcon, new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp), s_white, 1f, action));
    }

    private static void AddButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, s_field, 1f, action, Radius));
        elements.Add(TextMiddle(id + "-text", text, bounds.X + 16f * Dp, bounds.Y, bounds.Width - 32f * Dp, bounds.Height, MathF.Min(14f * Dp, bounds.Height * 0.45f), s_white, UiTextAlignment.Left, action));
    }

    private static void AddDropdownOption(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, bool isSelected)
    {
        if (isSelected)
        {
            elements.Add(Fill(id + "-selected", bounds, s_dropdownSelected, 1f, action, Radius));
        }

        elements.Add(TextMiddle(id + "-text", text, bounds.X + 12f * Dp, bounds.Y, Math.Max(1f, bounds.Width - 48f * Dp), bounds.Height, 14f * Dp, s_white, UiTextAlignment.Left, action));
        if (isSelected)
        {
            elements.Add(MaterialIcon(id + "-check", UiMaterialIcon.Check, new UiRect(bounds.Right - 34f * Dp, bounds.Y + 9f * Dp, 24f * Dp, 24f * Dp), s_accent, 1f, action));
        }
    }

    private static void AddDropdownButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, s_field, 1f, action, Radius));
        float textWidth = MathF.Min(bounds.Width - 56f * Dp, MathF.Max(24f * Dp, EstimateTextWidth(text, 14f * Dp)));
        elements.Add(TextMiddle(id + "-text", text, bounds.X + 16f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, s_white, UiTextAlignment.Left, action));
        elements.Add(MaterialIcon(id + "-caret", UiMaterialIcon.ArrowDropDown, new UiRect(bounds.Right - 34f * Dp, bounds.Y + 9f * Dp, 24f * Dp, 24f * Dp), s_white, 1f, action));
    }

    private static void AddButtonGroup(List<UiElementSnapshot> elements, List<DownloaderButtonSpec> buttons, float centerX, float y)
    {
        float totalWidth = buttons.Sum(button => button.Width) + Math.Max(0, buttons.Count - 1) * 8f * Dp;
        float x = centerX - totalWidth / 2f;
        foreach (DownloaderButtonSpec button in buttons)
        {
            AddIconButton(elements, button.Id, new UiRect(x, y, button.Width, 36f * Dp), button.Icon, button.Text, button.Action);
            x += button.Width + 8f * Dp;
        }
    }

    private static void AddIconButton(List<UiElementSnapshot> elements, string id, UiRect bounds, UiMaterialIcon icon, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, s_field, 1f, action, Radius));
        float textWidth = string.IsNullOrEmpty(text) ? 0f : MathF.Max(48f * Dp, text.Length * 7f * Dp);
        float contentWidth = 24f * Dp + (textWidth > 0f ? 6f * Dp + textWidth : 0f);
        float iconX = bounds.X + (bounds.Width - contentWidth) / 2f;
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(iconX, bounds.Y + 6f * Dp, 24f * Dp, 24f * Dp), s_white, 1f, action));
        if (textWidth > 0f)
        {
            elements.Add(TextMiddle(id + "-text", text, iconX + 30f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, s_white, UiTextAlignment.Left, action));
        }
    }

    private void AddStatusPill(List<UiElementSnapshot> elements, string id, BeatmapRankedStatus _status, float x, float y, UiAction action)
    {
        float width = StatusPillWidth(_status);
        var bounds = new UiRect(x, y, width, 20f * Dp);
        elements.Add(Fill(id + "-bg", bounds, StatusPillColor(), 1f, action, Radius));
        elements.Add(TextMiddle(id, RankedStatusText(_status), bounds.X + 12f * Dp, bounds.Y, bounds.Width - 24f * Dp, bounds.Height, 10f * Dp, RankedStatusColor(_status), UiTextAlignment.Center, action));
    }

    private float StatusPillWidth(BeatmapRankedStatus _status) => Math.Max(58f * Dp, EstimateTextWidth(RankedStatusText(_status), 10f * Dp) + 24f * Dp);

    private static UiColor StatusPillColor() => s_panel;

    private static UiColor RankedStatusColor(BeatmapRankedStatus status) => status switch
    {
        BeatmapRankedStatus.Ranked => DroidUiTheme.BeatmapStatus.Ranked,
        BeatmapRankedStatus.Approved => DroidUiTheme.BeatmapStatus.Ranked,
        BeatmapRankedStatus.Qualified => DroidUiTheme.BeatmapStatus.Qualified,
        BeatmapRankedStatus.Loved => DroidUiTheme.BeatmapStatus.Loved,
        BeatmapRankedStatus.Pending => DroidUiTheme.BeatmapStatus.Pending,
        BeatmapRankedStatus.WorkInProgress => DroidUiTheme.BeatmapStatus.Pending,
        BeatmapRankedStatus.Graveyard => DroidUiTheme.BeatmapStatus.Graveyard,
        _ => s_white,
    };

    private static void AddDifficultyDots(
        List<UiElementSnapshot> elements,
        string id,
        IReadOnlyList<BeatmapMirrorBeatmap> beatmaps,
        float x,
        float y,
        float width,
        int? selectedIndex,
        bool isCentered,
        UiAction fallbackAction)
    {
        const float cardCellWidth = 32f * Dp;
        const float detailsCellWidth = 56f * Dp;
        const float rowHeight = DifficultyGlyphRowHeight;
        const float margin = 6f * Dp;
        int count = Math.Min(beatmaps.Count, 16);
        if (count == 0)
        {
            return;
        }

        bool hasSelection = selectedIndex is not null;
        float gap = hasSelection ? margin : 0f;
        float preferredCellWidth = hasSelection ? detailsCellWidth : cardCellWidth;
        float availableWidth = Math.Max(0f, width - Math.Max(0, count - 1) * gap);
        float cellWidth = MathF.Min(preferredCellWidth, availableWidth / count);
        float glyphSize = MathF.Min(24f * Dp, MathF.Max(14f * Dp, cellWidth * 0.75f));
        float totalWidth = count * cellWidth + Math.Max(0, count - 1) * gap;
        float startX = isCentered ? x + (width - totalWidth) / 2f : x;
        float cursorX = startX;
        for (int i = 0; i < count; i++)
        {
            UiAction dotAction = hasSelection ? DifficultyAction(i) : fallbackAction;
            bool selected = selectedIndex == i;
            var hitBounds = new UiRect(cursorX, y, cellWidth, rowHeight);
            if (selected)
            {
                elements.Add(Fill($"{id}-{i}-selected", hitBounds, s_field, 1f, dotAction, Radius));
            }

            elements.Add(TextMiddle($"{id}-{i}", "⦿", hitBounds.X, hitBounds.Y, hitBounds.Width, hitBounds.Height, glyphSize, StarRatingColor(beatmaps[i].StarRating), UiTextAlignment.Center, dotAction));
            cursorX += cellWidth + gap;
        }
    }

    private static UiColor StarRatingColor(float starRating) => OsuDroidColors.StarRating(starRating);

    private static float EstimateTextWidth(string text, float size) => MathF.Max(1f, text.Length * size * 0.55f);

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) =>
        UiElementFactory.Fill(id, bounds, color, alpha, action, radius);

    private static UiElementSnapshot Text(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        UiElementFactory.Text(id, text, new UiRect(x, y, width, height), size, color, action, alignment: alignment);

    private static UiElementSnapshot TextMiddle(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        UiElementFactory.Text(id, text, new UiRect(x, y, width, height), size, color, action, alignment: alignment, verticalAlignment: UiTextVerticalAlignment.Middle);

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action);

    private static UiElementSnapshot ProgressRing(string id, UiRect bounds, UiColor color, float strokeWidth, float sweepDegrees, float rotationDegrees = 0f) =>
        UiElementFactory.ProgressRing(id, bounds, color, strokeWidth, sweepDegrees, rotationDegrees);

    private string RankedStatusText(BeatmapRankedStatus rankedStatus) => rankedStatus switch
    {
        BeatmapRankedStatus.Ranked => _localizer["BeatmapDownloader_Ranked"],
        BeatmapRankedStatus.Approved => _localizer["BeatmapDownloader_Approved"],
        BeatmapRankedStatus.Qualified => _localizer["BeatmapDownloader_Qualified"],
        BeatmapRankedStatus.Loved => _localizer["BeatmapDownloader_Loved"],
        BeatmapRankedStatus.WorkInProgress => _localizer["BeatmapDownloader_WorkInProgress"],
        BeatmapRankedStatus.Graveyard => _localizer["BeatmapDownloader_Graveyard"],
        _ => _localizer["BeatmapDownloader_Pending"],
    };

    private string SortText(BeatmapMirrorSort value) => value switch
    {
        BeatmapMirrorSort.Bpm => _localizer["Sort_Bpm"],
        BeatmapMirrorSort.DifficultyRating => _localizer["Sort_DifficultyRating"],
        BeatmapMirrorSort.HitLength => _localizer["Sort_HitLength"],
        BeatmapMirrorSort.PassCount => _localizer["Sort_PassCount"],
        BeatmapMirrorSort.PlayCount => _localizer["Sort_PlayCount"],
        BeatmapMirrorSort.TotalLength => _localizer["Sort_TotalLength"],
        BeatmapMirrorSort.FavouriteCount => _localizer["Sort_FavouriteCount"],
        BeatmapMirrorSort.LastUpdated => _localizer["Sort_LastUpdated"],
        BeatmapMirrorSort.RankedDate => _localizer["Sort_RankedDate"],
        BeatmapMirrorSort.SubmittedDate => _localizer["Sort_SubmittedDate"],
        _ => _localizer["Sort_Title"],
    };

    private static BeatmapMirrorSort Next(BeatmapMirrorSort value)
    {
        BeatmapMirrorSort[] values = Enum.GetValues<BeatmapMirrorSort>();
        return values[(Array.IndexOf(values, value) + 1) % values.Length];
    }

    private static string FormatDownloadInfo(BeatmapDownloadProgress? progress)
    {
        if (progress is null)
        {
            return string.Empty;
        }

        string percent = progress.Percent is double p ? FormattableString.Invariant($" ({p:0}%)") : string.Empty;
        string speed = progress.SpeedBytesPerSecond > 0 ? FormattableString.Invariant($"\n{progress.SpeedBytesPerSecond / 1024d / 1024d:0.###} mb/s") + percent : percent;
        return speed;
    }

    private string FormatDownloadText(BeatmapDownloadState state)
    {
        string key = state.Progress?.Phase switch
        {
            BeatmapDownloadPhase.Connecting => "BeatmapDownloader_Connecting",
            BeatmapDownloadPhase.Importing => "BeatmapDownloader_Importing",
            _ => "BeatmapDownloader_Downloading",
        };

        return !string.IsNullOrWhiteSpace(state.Filename)
            ? _localizer.Format(key, state.Filename)
            : _localizer[key]
                .Replace(" {0}", string.Empty, StringComparison.Ordinal)
                .Replace("{0}", string.Empty, StringComparison.Ordinal);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception)
        {
        }
    }
}
