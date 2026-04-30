using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private void AddBeatmapOptionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiRect search = BeatmapOptionsSearchBounds(viewport);
        elements.Add(
            Fill(
                "songselect-beatmap-options-search",
                search,
                s_beatmapOptionsSearchPanel,
                1f,
                UiAction.SongSelectBeatmapOptionsSearch,
                BeatmapOptionsRadius
            )
        );
        elements.Add(
            TextMiddle(
                "songselect-beatmap-options-search-text",
                searchQuery.Length == 0 ? _localizer["SongSelect_SearchPlaceholder"] : searchQuery,
                search.X + 16f * Dp,
                search.Y,
                search.Width - 64f * Dp,
                search.Height,
                16f * Dp,
                searchQuery.Length == 0 ? s_propertiesSecondary : s_white,
                UiTextAlignment.Left,
                UiAction.SongSelectBeatmapOptionsSearch
            )
        );
        elements.Add(
            MaterialIcon(
                "songselect-beatmap-options-search-icon",
                UiMaterialIcon.Search,
                new UiRect(search.Right - 40f * Dp, search.Y + 16f * Dp, 24f * Dp, 24f * Dp),
                s_propertiesSecondary,
                1f,
                UiAction.SongSelectBeatmapOptionsSearch
            )
        );

        float optionsY = search.Bottom + 12f * Dp;
        string algorithmText =
            _displayAlgorithm == DifficultyAlgorithm.Standard ? "osu!standard" : "osu!droid";
        string sortText = SortLabel(sortMode);
        string folderText = collectionFilter ?? _localizer["SongSelect_DefaultFavoriteFolder"];
        float favoriteWidth = IconOnlyOptionsButtonWidth();
        float algorithmWidth = TextOptionsButtonWidth(algorithmText);
        float sortWidth = TextOptionsButtonWidth(sortText);
        float folderWidth = TextOptionsButtonWidth(folderText, BeatmapOptionsFolderEndPadding);
        float stripWidth =
            favoriteWidth
            + algorithmWidth
            + sortWidth
            + folderWidth
            + BeatmapOptionsDividerWidth * 3f;
        float x = search.X;
        elements.Add(
            Fill(
                "songselect-beatmap-options-strip",
                new UiRect(search.X, optionsY, stripWidth, BeatmapOptionsRowHeight),
                s_propertiesPanel,
                1f,
                UiAction.SongSelectPropertiesPanel,
                BeatmapOptionsRadius
            )
        );

        AddOptionsButton(
            elements,
            "songselect-beatmap-options-favorite",
            new UiRect(x, optionsY, favoriteWidth, BeatmapOptionsRowHeight),
            favoriteOnlyFilter ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline,
            string.Empty,
            UiAction.SongSelectBeatmapOptionsFavorite,
            favoriteOnlyFilter ? s_beatmapOptionsAccent : s_beatmapOptionsInactiveCheckbox
        );
        x += favoriteWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-favorite", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(
            elements,
            "songselect-beatmap-options-algorithm",
            new UiRect(x, optionsY, algorithmWidth, BeatmapOptionsRowHeight),
            UiMaterialIcon.StarOutline,
            algorithmText,
            UiAction.SongSelectBeatmapOptionsAlgorithm,
            s_white
        );
        x += algorithmWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-algorithm", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(
            elements,
            "songselect-beatmap-options-sort",
            new UiRect(x, optionsY, sortWidth, BeatmapOptionsRowHeight),
            UiMaterialIcon.Sort,
            sortText,
            UiAction.SongSelectBeatmapOptionsSort,
            s_white
        );
        x += sortWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-sort", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(
            elements,
            "songselect-beatmap-options-folder",
            new UiRect(x, optionsY, folderWidth, BeatmapOptionsRowHeight),
            UiMaterialIcon.FolderOutline,
            folderText,
            UiAction.SongSelectBeatmapOptionsFolder,
            s_white
        );
    }

    private static void AddOptionsButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        UiMaterialIcon icon,
        string text,
        UiAction action,
        UiColor iconColor
    )
    {
        elements.Add(Fill(id + "-hit", bounds, s_propertiesPanel, 0f, action));
        float iconX = bounds.X + BeatmapOptionsHorizontalPadding;
        if (text.Length == 0)
        {
            iconX = bounds.X + (bounds.Width - BeatmapOptionsIconSize) / 2f;
        }

        elements.Add(
            MaterialIcon(
                id + "-icon",
                icon,
                new UiRect(
                    iconX,
                    bounds.Y + 14f * Dp,
                    BeatmapOptionsIconSize,
                    BeatmapOptionsIconSize
                ),
                iconColor,
                1f,
                action
            )
        );
        if (text.Length > 0)
        {
            elements.Add(
                TextMiddle(
                    id,
                    text,
                    bounds.X
                        + BeatmapOptionsHorizontalPadding
                        + BeatmapOptionsIconSize
                        + BeatmapOptionsDrawableGap,
                    bounds.Y,
                    bounds.Width
                        - BeatmapOptionsHorizontalPadding * 2f
                        - BeatmapOptionsIconSize
                        - BeatmapOptionsDrawableGap,
                    bounds.Height,
                    BeatmapOptionsTextSize,
                    s_white,
                    UiTextAlignment.Left,
                    action
                )
            );
        }
    }

    private static void AddOptionsDivider(
        List<UiElementSnapshot> elements,
        string id,
        float x,
        float y
    ) =>
        elements.Add(
            Fill(
                id,
                new UiRect(x, y, BeatmapOptionsDividerWidth, BeatmapOptionsRowHeight),
                s_beatmapOptionsDivider,
                1f
            )
        );

    private static float IconOnlyOptionsButtonWidth() =>
        BeatmapOptionsHorizontalPadding * 2f + BeatmapOptionsIconSize;

    private static float TextOptionsButtonWidth(
        string text,
        float endPadding = BeatmapOptionsHorizontalPadding
    ) =>
        BeatmapOptionsHorizontalPadding
        + BeatmapOptionsIconSize
        + BeatmapOptionsDrawableGap
        + EstimateOptionsTextWidth(text)
        + endPadding;

    private static float EstimateOptionsTextWidth(string text) =>
        text.Length * BeatmapOptionsTextSize * BeatmapOptionsTextWidthFactor;

    private static UiRect BeatmapOptionsSearchBounds(VirtualViewport viewport)
    {
        float width = Math.Min(BeatmapOptionsWidth, viewport.VirtualWidth - 120f * Dp);
        return new UiRect(
            (viewport.VirtualWidth - width) / 2f,
            8f * Dp,
            width,
            BeatmapOptionsSearchHeight
        );
    }

    private string SortLabel(SongSelectSortMode mode) =>
        mode switch
        {
            SongSelectSortMode.Artist => _localizer["Sort_Artist"],
            SongSelectSortMode.Creator => _localizer["Sort_Creator"],
            SongSelectSortMode.Date => _localizer["Sort_Date"],
            SongSelectSortMode.Bpm => _localizer["Sort_Bpm"],
            SongSelectSortMode.DroidStars => _localizer["Sort_DroidStars"],
            SongSelectSortMode.StandardStars => _localizer["Sort_StandardStars"],
            SongSelectSortMode.Length => _localizer["Sort_Length"],
            SongSelectSortMode.Title => _localizer["Sort_Title"],
            _ => _localizer["Sort_Title"],
        };
}
