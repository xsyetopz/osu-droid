using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private void AddModal(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!propertiesOpen && !beatmapOptionsOpen)
            return;

        elements.Add(Fill("songselect-popup-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), ModalShade, 1f, UiAction.SongSelectPropertiesDismiss));

        if (collectionsOpen)
            AddCollectionsPanel(elements, viewport);
        else if (beatmapOptionsOpen)
            AddBeatmapOptionsPanel(elements, viewport);
        else
            AddPropertiesPanel(elements, viewport);

        if (deleteBeatmapConfirmOpen)
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-beatmap",
                "Delete beatmap",
                "Are you sure?",
                UiAction.SongSelectPropertiesDeleteConfirm,
                UiAction.SongSelectPropertiesDeleteCancel);
        else if (collectionPendingDelete is not null)
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-collection",
                "Remove collection",
                "Are you sure?",
                UiAction.SongSelectCollectionDeleteConfirm,
                UiAction.SongSelectCollectionDeleteCancel);
    }

    private void AddPropertiesPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var options = CurrentOptions() ?? new BeatmapOptions(string.Empty);
        var panelHeight = PropertiesRowHeight * 5f;
        var panel = new UiRect((viewport.VirtualWidth - PropertiesWidth) / 2f, (viewport.VirtualHeight - panelHeight) / 2f, PropertiesWidth, panelHeight);
        elements.Add(Fill("songselect-properties-panel", panel, PropertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));

        AddPropertiesRowText(elements, "songselect-properties-title", "Song Properties", panel.X, panel.Y, panel.Width, PropertiesRowHeight, 15f * Dp, White, UiAction.SongSelectPropertiesPanel);
        AddDivider(elements, "songselect-properties-divider-title", panel.X, panel.Y + PropertiesRowHeight, panel.Width);

        var offsetY = panel.Y + PropertiesRowHeight;
        var buttonWidth = 70f * Dp;
        elements.Add(Fill("songselect-properties-offset-minus-hit", new UiRect(panel.X, offsetY, buttonWidth, PropertiesRowHeight), PropertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetMinus));
        elements.Add(MaterialIcon("songselect-properties-offset-minus", UiMaterialIcon.Minus, new UiRect(panel.X + (buttonWidth - 24f * Dp) / 2f, offsetY + 14f * Dp, 24f * Dp, 24f * Dp), White, 1f, UiAction.SongSelectPropertiesOffsetMinus));
        var input = PropertiesOffsetInputBounds(viewport);
        elements.Add(Fill("songselect-properties-offset-input-hit", input, PropertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetInput));
        AddPropertiesRowText(elements, "songselect-properties-offset-label", "Offset", input.X, input.Y + 4f * Dp, input.Width, 16f * Dp, 10f * Dp, PropertiesSecondary, UiAction.SongSelectPropertiesOffsetInput);
        AddPropertiesRowText(elements, "songselect-properties-offset-value", options.Offset.ToString(CultureInfo.InvariantCulture), input.X, input.Y + 18f * Dp, input.Width, 30f * Dp, 18f * Dp, White, UiAction.SongSelectPropertiesOffsetInput);
        elements.Add(Fill("songselect-properties-offset-plus-hit", new UiRect(panel.Right - buttonWidth, offsetY, buttonWidth, PropertiesRowHeight), PropertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetPlus));
        elements.Add(MaterialIcon("songselect-properties-offset-plus", UiMaterialIcon.Plus, new UiRect(panel.Right - buttonWidth + (buttonWidth - 24f * Dp) / 2f, offsetY + 14f * Dp, 24f * Dp, 24f * Dp), White, 1f, UiAction.SongSelectPropertiesOffsetPlus));
        AddDivider(elements, "songselect-properties-divider-offset", panel.X, offsetY + PropertiesRowHeight, panel.Width);

        var favoriteY = offsetY + PropertiesRowHeight;
        AddIconRow(elements, "songselect-properties-favorite", options.IsFavorite ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline, "Add to Favorites", panel.X, favoriteY, panel.Width, UiAction.SongSelectPropertiesFavorite, White);
        AddDivider(elements, "songselect-properties-divider-favorite", panel.X, favoriteY + PropertiesRowHeight, panel.Width);

        var manageY = favoriteY + PropertiesRowHeight;
        AddIconRow(elements, "songselect-properties-manage", UiMaterialIcon.Folder, "Manage Favorites", panel.X, manageY, panel.Width, UiAction.SongSelectPropertiesManageCollections, White);
        AddDivider(elements, "songselect-properties-divider-manage", panel.X, manageY + PropertiesRowHeight, panel.Width);

        AddIconRow(elements, "songselect-properties-delete", UiMaterialIcon.Delete, "Delete beatmap", panel.X, manageY + PropertiesRowHeight, panel.Width, UiAction.SongSelectPropertiesDelete, PropertiesDanger);
    }

    private void AddBeatmapOptionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var search = BeatmapOptionsSearchBounds(viewport);
        elements.Add(Fill("songselect-beatmap-options-search", search, BeatmapOptionsSearchPanel, 1f, UiAction.SongSelectBeatmapOptionsSearch, BeatmapOptionsRadius));
        elements.Add(TextMiddle(
            "songselect-beatmap-options-search-text",
            searchQuery.Length == 0 ? "Search for..." : searchQuery,
            search.X + 16f * Dp,
            search.Y,
            search.Width - 64f * Dp,
            search.Height,
            16f * Dp,
            searchQuery.Length == 0 ? PropertiesSecondary : White,
            UiTextAlignment.Left,
            UiAction.SongSelectBeatmapOptionsSearch));
        elements.Add(MaterialIcon("songselect-beatmap-options-search-icon", UiMaterialIcon.Search, new UiRect(search.Right - 40f * Dp, search.Y + 16f * Dp, 24f * Dp, 24f * Dp), PropertiesSecondary, 1f, UiAction.SongSelectBeatmapOptionsSearch));

        var optionsY = search.Bottom + 12f * Dp;
        var x = search.X;
        var favoriteWidth = 56f * Dp;
        var algorithmWidth = 190f * Dp;
        var sortWidth = 150f * Dp;
        var folderWidth = 210f * Dp;
        var stripWidth = favoriteWidth + algorithmWidth + sortWidth + folderWidth + BeatmapOptionsDividerWidth * 3f;
        elements.Add(Fill("songselect-beatmap-options-strip", new UiRect(search.X, optionsY, stripWidth, BeatmapOptionsRowHeight), PropertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, BeatmapOptionsRadius));

        AddOptionsButton(elements, "songselect-beatmap-options-favorite", new UiRect(x, optionsY, favoriteWidth, BeatmapOptionsRowHeight), favoriteOnlyFilter ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline, string.Empty, UiAction.SongSelectBeatmapOptionsFavorite);
        x += favoriteWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-favorite", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-algorithm", new UiRect(x, optionsY, algorithmWidth, BeatmapOptionsRowHeight), UiMaterialIcon.Star, displayAlgorithm == DifficultyAlgorithm.Standard ? "osu!standard" : "osu!droid", UiAction.SongSelectBeatmapOptionsAlgorithm);
        x += algorithmWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-algorithm", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-sort", new UiRect(x, optionsY, sortWidth, BeatmapOptionsRowHeight), UiMaterialIcon.Sort, SortLabel(sortMode), UiAction.SongSelectBeatmapOptionsSort);
        x += sortWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-sort", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-folder", new UiRect(x, optionsY, folderWidth, BeatmapOptionsRowHeight), UiMaterialIcon.Folder, collectionFilter ?? "Folder", UiAction.SongSelectBeatmapOptionsFolder);
    }

    private void AddCollectionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var set = SelectedSet;
        var sourceCollections = library.GetCollections(set?.Directory).ToArray();
        var collections = collectionsFilterMode
            ? new[] { new BeatmapCollection("All folders", sourceCollections.Sum(collection => collection.BeatmapCount), collectionFilter is null) }.Concat(sourceCollections).ToArray()
            : sourceCollections;
        var panelHeight = viewport.VirtualHeight - CollectionsMargin * 2f;
        var panel = new UiRect((viewport.VirtualWidth - CollectionsWidth) / 2f, viewport.VirtualHeight - CollectionsMargin - panelHeight, CollectionsWidth, panelHeight);
        elements.Add(Fill("songselect-collections-panel", panel, CollectionsPanelDark, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));
        AddIconRow(elements, "songselect-collections-new", UiMaterialIcon.Plus, "New folder", panel.X, panel.Y, panel.Width, UiAction.SongSelectCollectionsNewFolder, White);
        AddDivider(elements, "songselect-collections-divider-new", panel.X, panel.Y + PropertiesRowHeight, panel.Width);

        var listY = panel.Y + PropertiesRowHeight + 12f * Dp;
        var rowGap = 8f * Dp;
        var rowStep = CollectionRowHeight + rowGap;
        var first = Math.Max(0, (int)MathF.Floor(collectionScrollY / rowStep));
        var yOffset = -(collectionScrollY - first * rowStep);
        for (var slot = 0; slot < VisibleCollectionSlots; slot++)
        {
            var index = first + slot;
            if (index >= collections.Length)
                break;

            var rowY = listY + yOffset + slot * rowStep;
            if (rowY > panel.Bottom - rowGap)
                break;

            visibleCollectionIndices[slot] = index;
            AddCollectionRow(elements, slot, collections[index], panel.X + 12f * Dp, rowY, panel.Width - 24f * Dp, collectionsFilterMode, collectionFilter);
        }

        if (collections.Length == 0)
            elements.Add(TextMiddle("songselect-collections-empty", "No collections", panel.X, listY, panel.Width, CollectionRowHeight, 16f * Dp, PropertiesSecondary, UiTextAlignment.Center));
    }

    private static void AddCollectionRow(List<UiElementSnapshot> elements, int slot, BeatmapCollection collection, float x, float y, float width, bool filterMode, string? selectedFilter)
    {
        var action = filterMode ? CollectionToggleAction(slot) : UiAction.None;
        elements.Add(Fill($"songselect-collection-{slot}", new UiRect(x, y, width, CollectionRowHeight), PropertiesPanel, 1f, action, 14f * Dp));
        elements.Add(TextMiddle($"songselect-collection-{slot}-name", collection.Name, x + 16f * Dp, y, width - 180f * Dp, CollectionRowHeight, 15f * Dp, White, UiTextAlignment.Left, action));
        if (!filterMode || slot != 0)
            elements.Add(TextMiddle($"songselect-collection-{slot}-count", $"· {collection.BeatmapCount} beatmaps", x + 170f * Dp, y, width - 280f * Dp, CollectionRowHeight, 12f * Dp, PropertiesSecondary, UiTextAlignment.Left, action));
        if (filterMode)
        {
            if (string.Equals(collection.Name, selectedFilter, StringComparison.Ordinal))
                elements.Add(MaterialIcon($"songselect-collection-{slot}-selected", UiMaterialIcon.Check, new UiRect(x + width - 40f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
            return;
        }

        AddSmallAction(elements, $"songselect-collection-{slot}-delete", UiMaterialIcon.Delete, x + width - 112f * Dp, y, CollectionDeleteAction(slot), PropertiesDanger);
        AddSmallAction(elements, $"songselect-collection-{slot}-toggle", collection.ContainsSelectedSet ? UiMaterialIcon.Minus : UiMaterialIcon.Plus, x + width - 56f * Dp, y, CollectionToggleAction(slot), White);
    }

    private static void AddSmallAction(List<UiElementSnapshot> elements, string id, UiMaterialIcon icon, float x, float y, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, 56f * Dp, CollectionRowHeight), PropertiesPanel, 1f, action));
        elements.Add(MaterialIcon(id, icon, new UiRect(x + 16f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp), color, 1f, action));
    }

    private static void AddConfirmPanel(List<UiElementSnapshot> elements, VirtualViewport viewport, string id, string title, string message, UiAction confirmAction, UiAction cancelAction)
    {
        elements.Add(Fill(id + "-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), new UiColor(0, 0, 0, 96), 1f, cancelAction));
        var width = 300f * Dp;
        var height = 150f * Dp;
        var panel = new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height);
        elements.Add(Fill(id + "-panel", panel, PropertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));
        AddPropertiesRowText(elements, id + "-title", title, panel.X, panel.Y, panel.Width, 44f * Dp, 15f * Dp, White, UiAction.SongSelectPropertiesPanel);
        AddPropertiesRowText(elements, id + "-message", message, panel.X, panel.Y + 44f * Dp, panel.Width, 44f * Dp, 14f * Dp, PropertiesSecondary, UiAction.SongSelectPropertiesPanel);
        AddFullWidthRow(elements, id + "-yes", "Yes", panel.X, panel.Y + 88f * Dp, panel.Width / 2f, confirmAction, PropertiesDanger);
        AddFullWidthRow(elements, id + "-no", "No", panel.X + panel.Width / 2f, panel.Y + 88f * Dp, panel.Width / 2f, cancelAction, White);
    }

    private static void AddFullWidthRow(List<UiElementSnapshot> elements, string id, string text, float x, float y, float width, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, width, PropertiesRowHeight), PropertiesPanel, 1f, action));
        elements.Add(TextMiddle(id, text, x + 16f * Dp, y, width - 32f * Dp, PropertiesRowHeight, 15f * Dp, color, UiTextAlignment.Left, action));
    }

    private static void AddIconRow(List<UiElementSnapshot> elements, string id, UiMaterialIcon icon, string text, float x, float y, float width, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, width, PropertiesRowHeight), PropertiesPanel, 1f, action));
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(x + 24f * Dp, y + 14f * Dp, 24f * Dp, 24f * Dp), color, 1f, action));
        elements.Add(TextMiddle(id, text, x + 58f * Dp, y, width - 74f * Dp, PropertiesRowHeight, 15f * Dp, color, UiTextAlignment.Left, action));
    }

    private static void AddOptionsButton(List<UiElementSnapshot> elements, string id, UiRect bounds, UiMaterialIcon icon, string text, UiAction action)
    {
        elements.Add(Fill(id + "-hit", bounds, PropertiesPanel, 0f, action));
        var iconX = bounds.X + 16f * Dp;
        if (text.Length == 0)
            iconX = bounds.X + (bounds.Width - 24f * Dp) / 2f;
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(iconX, bounds.Y + 14f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
        if (text.Length > 0)
            elements.Add(TextMiddle(id, text, bounds.X + 52f * Dp, bounds.Y, bounds.Width - 68f * Dp, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
    }

    private static void AddOptionsDivider(List<UiElementSnapshot> elements, string id, float x, float y) =>
        elements.Add(Fill(id, new UiRect(x, y, BeatmapOptionsDividerWidth, BeatmapOptionsRowHeight), BeatmapOptionsDivider, 1f));

    private static void AddPropertiesRowText(List<UiElementSnapshot> elements, string id, string text, float x, float y, float width, float height, float size, UiColor color, UiAction action) =>
        elements.Add(TextMiddle(id, text, x, y, width, height, size, color, UiTextAlignment.Center, action));

    private static void AddDivider(List<UiElementSnapshot> elements, string id, float x, float y, float width) =>
        elements.Add(Fill(id, new UiRect(x, y, width, 1f * Dp), PropertiesDivider, 1f));

    private static UiRect BeatmapOptionsSearchBounds(VirtualViewport viewport)
    {
        var width = Math.Min(BeatmapOptionsWidth, viewport.VirtualWidth - 120f * Dp);
        return new UiRect((viewport.VirtualWidth - width) / 2f, 8f * Dp, width, BeatmapOptionsSearchHeight);
    }

    private static string SortLabel(SongSelectSortMode mode) => mode switch
    {
        SongSelectSortMode.Artist => "Artist",
        SongSelectSortMode.Creator => "Creator",
        SongSelectSortMode.Date => "Date",
        SongSelectSortMode.Bpm => "BPM",
        SongSelectSortMode.DroidStars => "Droid ★",
        SongSelectSortMode.StandardStars => "Std ★",
        SongSelectSortMode.Length => "Length",
        _ => "Title",
    };
}
