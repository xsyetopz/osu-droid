using System.Globalization;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
#pragma warning disable IDE0072 // Sort mode defaults to Title for unknown values.
    private void AddModal(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!_propertiesOpen && !_beatmapOptionsOpen)
        {
            return;
        }

        elements.Add(Fill("songselect-popup-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_modalShade, 1f, UiAction.SongSelectPropertiesDismiss));

        if (_collectionsOpen)
        {
            AddCollectionsPanel(elements, viewport);
        }
        else if (_beatmapOptionsOpen)
        {
            AddBeatmapOptionsPanel(elements, viewport);
        }
        else
        {
            AddPropertiesPanel(elements, viewport);
        }

        if (_deleteBeatmapConfirmOpen)
        {
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-beatmap",
                _localizer["SongSelect_DeleteBeatmapTitle"],
                _localizer["SongSelect_DeleteBeatmapMessage"],
                UiAction.SongSelectPropertiesDeleteConfirm,
                UiAction.SongSelectPropertiesDeleteCancel);
        }
        else if (_collectionPendingDelete is not null)
        {
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-collection",
                _localizer["SongSelect_RemoveCollectionTitle"],
                _localizer["SongSelect_DeleteBeatmapMessage"],
                UiAction.SongSelectCollectionDeleteConfirm,
                UiAction.SongSelectCollectionDeleteCancel);
        }
    }

    private void AddPropertiesPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        BeatmapOptions options = CurrentOptions() ?? new BeatmapOptions(string.Empty);
        float panelHeight = PropertiesRowHeight * 5f;
        var panel = new UiRect((viewport.VirtualWidth - PropertiesWidth) / 2f, (viewport.VirtualHeight - panelHeight) / 2f, PropertiesWidth, panelHeight);
        elements.Add(Fill("songselect-properties-panel", panel, s_propertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));

        AddPropertiesRowText(elements, "songselect-properties-title", _localizer["SongSelect_PropertiesTitle"], panel.X, panel.Y, panel.Width, PropertiesRowHeight, 15f * Dp, s_white, UiAction.SongSelectPropertiesPanel);
        AddDivider(elements, "songselect-properties-divider-title", panel.X, panel.Y + PropertiesRowHeight, panel.Width);

        float offsetY = panel.Y + PropertiesRowHeight;
        float buttonWidth = 70f * Dp;
        elements.Add(Fill("songselect-properties-offset-minus-hit", new UiRect(panel.X, offsetY, buttonWidth, PropertiesRowHeight), s_propertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetMinus));
        elements.Add(MaterialIcon("songselect-properties-offset-minus", UiMaterialIcon.Minus, new UiRect(panel.X + (buttonWidth - 24f * Dp) / 2f, offsetY + 14f * Dp, 24f * Dp, 24f * Dp), s_white, 1f, UiAction.SongSelectPropertiesOffsetMinus));
        UiRect input = PropertiesOffsetInputBounds(viewport);
        elements.Add(Fill("songselect-properties-offset-input-hit", input, s_propertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetInput));
        AddPropertiesRowText(elements, "songselect-properties-offset-label", _localizer["SongSelect_Offset"], input.X, input.Y + 4f * Dp, input.Width, 16f * Dp, 10f * Dp, s_propertiesSecondary, UiAction.SongSelectPropertiesOffsetInput);
        AddPropertiesRowText(elements, "songselect-properties-offset-value", options.Offset.ToString(CultureInfo.InvariantCulture), input.X, input.Y + 18f * Dp, input.Width, 30f * Dp, 18f * Dp, s_white, UiAction.SongSelectPropertiesOffsetInput);
        elements.Add(Fill("songselect-properties-offset-plus-hit", new UiRect(panel.Right - buttonWidth, offsetY, buttonWidth, PropertiesRowHeight), s_propertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetPlus));
        elements.Add(MaterialIcon("songselect-properties-offset-plus", UiMaterialIcon.Plus, new UiRect(panel.Right - buttonWidth + (buttonWidth - 24f * Dp) / 2f, offsetY + 14f * Dp, 24f * Dp, 24f * Dp), s_white, 1f, UiAction.SongSelectPropertiesOffsetPlus));
        AddDivider(elements, "songselect-properties-divider-offset", panel.X, offsetY + PropertiesRowHeight, panel.Width);

        float favoriteY = offsetY + PropertiesRowHeight;
        AddIconRow(elements, "songselect-properties-favorite", options.IsFavorite ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline, _localizer["SongSelect_AddToFavorites"], panel.X, favoriteY, panel.Width, UiAction.SongSelectPropertiesFavorite, s_white);
        AddDivider(elements, "songselect-properties-divider-favorite", panel.X, favoriteY + PropertiesRowHeight, panel.Width);

        float manageY = favoriteY + PropertiesRowHeight;
        AddIconRow(elements, "songselect-properties-manage", UiMaterialIcon.Folder, _localizer["SongSelect_ManageFavorites"], panel.X, manageY, panel.Width, UiAction.SongSelectPropertiesManageCollections, s_white);
        AddDivider(elements, "songselect-properties-divider-manage", panel.X, manageY + PropertiesRowHeight, panel.Width);

        AddIconRow(elements, "songselect-properties-delete", UiMaterialIcon.Delete, _localizer["SongSelect_DeleteBeatmapTitle"], panel.X, manageY + PropertiesRowHeight, panel.Width, UiAction.SongSelectPropertiesDelete, s_propertiesDanger);
    }

    private void AddBeatmapOptionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiRect search = BeatmapOptionsSearchBounds(viewport);
        elements.Add(Fill("songselect-beatmap-options-search", search, s_beatmapOptionsSearchPanel, 1f, UiAction.SongSelectBeatmapOptionsSearch, BeatmapOptionsRadius));
        elements.Add(TextMiddle(
            "songselect-beatmap-options-search-text",
            searchQuery.Length == 0 ? _localizer["SongSelect_SearchPlaceholder"] : searchQuery,
            search.X + 16f * Dp,
            search.Y,
            search.Width - 64f * Dp,
            search.Height,
            16f * Dp,
            searchQuery.Length == 0 ? s_propertiesSecondary : s_white,
            UiTextAlignment.Left,
            UiAction.SongSelectBeatmapOptionsSearch));
        elements.Add(MaterialIcon("songselect-beatmap-options-search-icon", UiMaterialIcon.Search, new UiRect(search.Right - 40f * Dp, search.Y + 16f * Dp, 24f * Dp, 24f * Dp), s_propertiesSecondary, 1f, UiAction.SongSelectBeatmapOptionsSearch));

        float optionsY = search.Bottom + 12f * Dp;
        string algorithmText = _displayAlgorithm == DifficultyAlgorithm.Standard ? "osu!standard" : "osu!droid";
        string sortText = SortLabel(sortMode);
        string folderText = collectionFilter ?? _localizer["SongSelect_DefaultFavoriteFolder"];
        float favoriteWidth = IconOnlyOptionsButtonWidth();
        float algorithmWidth = TextOptionsButtonWidth(algorithmText);
        float sortWidth = TextOptionsButtonWidth(sortText);
        float folderWidth = TextOptionsButtonWidth(folderText, BeatmapOptionsFolderEndPadding);
        float stripWidth = favoriteWidth + algorithmWidth + sortWidth + folderWidth + BeatmapOptionsDividerWidth * 3f;
        float x = search.X;
        elements.Add(Fill("songselect-beatmap-options-strip", new UiRect(search.X, optionsY, stripWidth, BeatmapOptionsRowHeight), s_propertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, BeatmapOptionsRadius));

        AddOptionsButton(elements, "songselect-beatmap-options-favorite", new UiRect(x, optionsY, favoriteWidth, BeatmapOptionsRowHeight), favoriteOnlyFilter ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline, string.Empty, UiAction.SongSelectBeatmapOptionsFavorite, favoriteOnlyFilter ? s_beatmapOptionsAccent : s_beatmapOptionsInactiveCheckbox);
        x += favoriteWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-favorite", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-algorithm", new UiRect(x, optionsY, algorithmWidth, BeatmapOptionsRowHeight), UiMaterialIcon.StarOutline, algorithmText, UiAction.SongSelectBeatmapOptionsAlgorithm, s_white);
        x += algorithmWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-algorithm", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-sort", new UiRect(x, optionsY, sortWidth, BeatmapOptionsRowHeight), UiMaterialIcon.Sort, sortText, UiAction.SongSelectBeatmapOptionsSort, s_white);
        x += sortWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-sort", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-folder", new UiRect(x, optionsY, folderWidth, BeatmapOptionsRowHeight), UiMaterialIcon.FolderOutline, folderText, UiAction.SongSelectBeatmapOptionsFolder, s_white);
    }

    private void AddCollectionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        BeatmapSetInfo? set = SelectedSet;
        BeatmapCollection[] sourceCollections = library.GetCollections(set?.Directory).ToArray();
        BeatmapCollection[] collections = _collectionsFilterMode
            ? new[] { new BeatmapCollection(_localizer["SongSelect_DefaultFavoriteFolder"], 0, collectionFilter is null) }.Concat(sourceCollections).ToArray()
            : sourceCollections;
        float panelHeight = viewport.VirtualHeight - CollectionsMargin * 2f;
        var panel = new UiRect((viewport.VirtualWidth - CollectionsWidth) / 2f, viewport.VirtualHeight - CollectionsMargin - panelHeight, CollectionsWidth, panelHeight);
        elements.Add(Fill("songselect-collections-panel", panel, s_collectionsPanelDark, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));
        AddIconRow(elements, "songselect-collections-new", UiMaterialIcon.Plus, _localizer["SongSelect_CreateNewFolder"], panel.X, panel.Y, panel.Width, UiAction.SongSelectCollectionsNewFolder, s_white);
        AddDivider(elements, "songselect-collections-divider-new", panel.X, panel.Y + PropertiesRowHeight, panel.Width);

        float listY = panel.Y + PropertiesRowHeight + 12f * Dp;
        float rowGap = 8f * Dp;
        float rowStep = CollectionRowHeight + rowGap;
        int first = Math.Max(0, (int)MathF.Floor(collectionScrollY / rowStep));
        float yOffset = -(collectionScrollY - first * rowStep);
        for (int slot = 0; slot < VisibleCollectionSlots; slot++)
        {
            int index = first + slot;
            if (index >= collections.Length)
            {
                break;
            }

            float rowY = listY + yOffset + slot * rowStep;
            if (rowY > panel.Bottom - rowGap)
            {
                break;
            }

            _visibleCollectionIndices[slot] = index;
            AddCollectionRow(elements, slot, collections[index], panel.X + 12f * Dp, rowY, panel.Width - 24f * Dp, _collectionsFilterMode, collectionFilter);
        }

        if (collections.Length == 0)
        {
            elements.Add(TextMiddle("songselect-collections-empty", _localizer["SongSelect_NoCollections"], panel.X, listY, panel.Width, CollectionRowHeight, 16f * Dp, s_propertiesSecondary, UiTextAlignment.Center));
        }
    }

    private void AddCollectionRow(List<UiElementSnapshot> elements, int slot, BeatmapCollection collection, float x, float y, float width, bool filterMode, string? selectedFilter)
    {
        UiAction action = filterMode ? CollectionToggleAction(slot) : UiAction.None;
        elements.Add(Fill($"songselect-collection-{slot}", new UiRect(x, y, width, CollectionRowHeight), s_propertiesPanel, 1f, action, 14f * Dp));
        elements.Add(TextMiddle($"songselect-collection-{slot}-name", collection.Name, x + 16f * Dp, y, width - 180f * Dp, CollectionRowHeight, 15f * Dp, s_white, UiTextAlignment.Left, action));
        if (!filterMode || slot != 0)
        {
            elements.Add(TextMiddle($"songselect-collection-{slot}-count", _localizer.Format("SongSelect_CollectionBeatmaps", collection.BeatmapCount), x + 170f * Dp, y, width - 280f * Dp, CollectionRowHeight, 12f * Dp, s_propertiesSecondary, UiTextAlignment.Left, action));
        }

        if (filterMode)
        {
            bool isDefaultSelected = slot == 0 && selectedFilter is null;
            if (isDefaultSelected || string.Equals(collection.Name, selectedFilter, StringComparison.Ordinal))
            {
                elements.Add(MaterialIcon($"songselect-collection-{slot}-selected", UiMaterialIcon.Check, new UiRect(x + width - 40f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp), s_white, 1f, action));
            }

            return;
        }

        AddSmallAction(elements, $"songselect-collection-{slot}-delete", UiMaterialIcon.Delete, x + width - 112f * Dp, y, CollectionDeleteAction(slot), s_propertiesDanger);
        AddSmallAction(elements, $"songselect-collection-{slot}-toggle", collection.ContainsSelectedSet ? UiMaterialIcon.Minus : UiMaterialIcon.Plus, x + width - 56f * Dp, y, CollectionToggleAction(slot), s_white);
    }

    private static void AddSmallAction(List<UiElementSnapshot> elements, string id, UiMaterialIcon icon, float x, float y, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, 56f * Dp, CollectionRowHeight), s_propertiesPanel, 1f, action));
        elements.Add(MaterialIcon(id, icon, new UiRect(x + 16f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp), color, 1f, action));
    }

    private void AddConfirmPanel(List<UiElementSnapshot> elements, VirtualViewport viewport, string id, string title, string message, UiAction confirmAction, UiAction cancelAction)
    {
        elements.Add(Fill(id + "-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), DroidUiColors.ModalShadeLight, 1f, cancelAction));
        float width = 300f * Dp;
        float height = 150f * Dp;
        var panel = new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height);
        elements.Add(Fill(id + "-panel", panel, s_propertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));
        AddPropertiesRowText(elements, id + "-title", title, panel.X, panel.Y, panel.Width, 44f * Dp, 15f * Dp, s_white, UiAction.SongSelectPropertiesPanel);
        AddPropertiesRowText(elements, id + "-message", message, panel.X, panel.Y + 44f * Dp, panel.Width, 44f * Dp, 14f * Dp, s_propertiesSecondary, UiAction.SongSelectPropertiesPanel);
        AddFullWidthRow(elements, id + "-yes", _localizer["Common_Yes"], panel.X, panel.Y + 88f * Dp, panel.Width / 2f, confirmAction, s_propertiesDanger);
        AddFullWidthRow(elements, id + "-no", _localizer["Common_No"], panel.X + panel.Width / 2f, panel.Y + 88f * Dp, panel.Width / 2f, cancelAction, s_white);
    }

    private static void AddFullWidthRow(List<UiElementSnapshot> elements, string id, string text, float x, float y, float width, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, width, PropertiesRowHeight), s_propertiesPanel, 1f, action));
        elements.Add(TextMiddle(id, text, x + 16f * Dp, y, width - 32f * Dp, PropertiesRowHeight, 15f * Dp, color, UiTextAlignment.Left, action));
    }

    private static void AddIconRow(List<UiElementSnapshot> elements, string id, UiMaterialIcon icon, string text, float x, float y, float width, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, width, PropertiesRowHeight), s_propertiesPanel, 1f, action));
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(x + 24f * Dp, y + 14f * Dp, 24f * Dp, 24f * Dp), color, 1f, action));
        elements.Add(TextMiddle(id, text, x + 58f * Dp, y, width - 74f * Dp, PropertiesRowHeight, 15f * Dp, color, UiTextAlignment.Left, action));
    }

    private static void AddOptionsButton(List<UiElementSnapshot> elements, string id, UiRect bounds, UiMaterialIcon icon, string text, UiAction action, UiColor iconColor)
    {
        elements.Add(Fill(id + "-hit", bounds, s_propertiesPanel, 0f, action));
        float iconX = bounds.X + BeatmapOptionsHorizontalPadding;
        if (text.Length == 0)
        {
            iconX = bounds.X + (bounds.Width - BeatmapOptionsIconSize) / 2f;
        }

        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(iconX, bounds.Y + 14f * Dp, BeatmapOptionsIconSize, BeatmapOptionsIconSize), iconColor, 1f, action));
        if (text.Length > 0)
        {
            elements.Add(TextMiddle(id, text, bounds.X + BeatmapOptionsHorizontalPadding + BeatmapOptionsIconSize + BeatmapOptionsDrawableGap, bounds.Y, bounds.Width - BeatmapOptionsHorizontalPadding * 2f - BeatmapOptionsIconSize - BeatmapOptionsDrawableGap, bounds.Height, BeatmapOptionsTextSize, s_white, UiTextAlignment.Left, action));
        }
    }

    private static void AddOptionsDivider(List<UiElementSnapshot> elements, string id, float x, float y) =>
        elements.Add(Fill(id, new UiRect(x, y, BeatmapOptionsDividerWidth, BeatmapOptionsRowHeight), s_beatmapOptionsDivider, 1f));

    private static float IconOnlyOptionsButtonWidth() => BeatmapOptionsHorizontalPadding * 2f + BeatmapOptionsIconSize;

    private static float TextOptionsButtonWidth(string text, float endPadding = BeatmapOptionsHorizontalPadding) =>
        BeatmapOptionsHorizontalPadding + BeatmapOptionsIconSize + BeatmapOptionsDrawableGap + EstimateOptionsTextWidth(text) + endPadding;

    private static float EstimateOptionsTextWidth(string text) => text.Length * BeatmapOptionsTextSize * BeatmapOptionsTextWidthFactor;

    private static void AddPropertiesRowText(List<UiElementSnapshot> elements, string id, string text, float x, float y, float width, float height, float size, UiColor color, UiAction action) =>
        elements.Add(TextMiddle(id, text, x, y, width, height, size, color, UiTextAlignment.Center, action));

    private static void AddDivider(List<UiElementSnapshot> elements, string id, float x, float y, float width) =>
        elements.Add(Fill(id, new UiRect(x, y, width, 1f * Dp), s_propertiesDivider, 1f));

    private static UiRect BeatmapOptionsSearchBounds(VirtualViewport viewport)
    {
        float width = Math.Min(BeatmapOptionsWidth, viewport.VirtualWidth - 120f * Dp);
        return new UiRect((viewport.VirtualWidth - width) / 2f, 8f * Dp, width, BeatmapOptionsSearchHeight);
    }

    private string SortLabel(SongSelectSortMode mode) => mode switch
    {
        SongSelectSortMode.Artist => _localizer["Sort_Artist"],
        SongSelectSortMode.Creator => _localizer["Sort_Creator"],
        SongSelectSortMode.Date => _localizer["Sort_Date"],
        SongSelectSortMode.Bpm => _localizer["Sort_Bpm"],
        SongSelectSortMode.DroidStars => _localizer["Sort_DroidStars"],
        SongSelectSortMode.StandardStars => _localizer["Sort_StandardStars"],
        SongSelectSortMode.Length => _localizer["Sort_Length"],
        _ => _localizer["Sort_Title"],
    };
}
