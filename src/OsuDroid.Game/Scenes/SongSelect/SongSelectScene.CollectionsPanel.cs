using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private void AddCollectionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        BeatmapSetInfo? set = SelectedSet;
        BeatmapCollection[] sourceCollections = library.GetCollections(set?.Directory).ToArray();
        BeatmapCollection[] collections = _collectionsFilterMode
            ? new[]
            {
                new BeatmapCollection(
                    _localizer["SongSelect_DefaultFavoriteFolder"],
                    0,
                    collectionFilter is null
                ),
            }
                .Concat(sourceCollections)
                .ToArray()
            : sourceCollections;
        float panelHeight = viewport.VirtualHeight - CollectionsMargin * 2f;
        var panel = new UiRect(
            (viewport.VirtualWidth - CollectionsWidth) / 2f,
            viewport.VirtualHeight - CollectionsMargin - panelHeight,
            CollectionsWidth,
            panelHeight
        );
        elements.Add(
            Fill(
                "songselect-collections-panel",
                panel,
                s_collectionsPanelDark,
                1f,
                UiAction.SongSelectPropertiesPanel,
                14f * Dp
            )
        );
        AddIconRow(
            elements,
            "songselect-collections-new",
            UiMaterialIcon.Plus,
            _localizer["SongSelect_CreateNewFolder"],
            panel.X,
            panel.Y,
            panel.Width,
            UiAction.SongSelectCollectionsNewFolder,
            s_white
        );
        AddDivider(
            elements,
            "songselect-collections-divider-new",
            panel.X,
            panel.Y + PropertiesRowHeight,
            panel.Width
        );

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
            AddCollectionRow(
                elements,
                slot,
                collections[index],
                panel.X + 12f * Dp,
                rowY,
                panel.Width - 24f * Dp,
                _collectionsFilterMode,
                collectionFilter
            );
        }

        if (collections.Length == 0)
        {
            elements.Add(
                TextMiddle(
                    "songselect-collections-empty",
                    _localizer["SongSelect_NoCollections"],
                    panel.X,
                    listY,
                    panel.Width,
                    CollectionRowHeight,
                    16f * Dp,
                    s_propertiesSecondary,
                    UiTextAlignment.Center
                )
            );
        }
    }

    private void AddCollectionRow(
        List<UiElementSnapshot> elements,
        int slot,
        BeatmapCollection collection,
        float x,
        float y,
        float width,
        bool filterMode,
        string? selectedFilter
    )
    {
        UiAction action = filterMode ? CollectionToggleAction(slot) : UiAction.None;
        elements.Add(
            Fill(
                $"songselect-collection-{slot}",
                new UiRect(x, y, width, CollectionRowHeight),
                s_propertiesPanel,
                1f,
                action,
                14f * Dp
            )
        );
        elements.Add(
            TextMiddle(
                $"songselect-collection-{slot}-name",
                collection.Name,
                x + 16f * Dp,
                y,
                width - 180f * Dp,
                CollectionRowHeight,
                15f * Dp,
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
        if (!filterMode || slot != 0)
        {
            elements.Add(
                TextMiddle(
                    $"songselect-collection-{slot}-count",
                    _localizer.Format("SongSelect_CollectionBeatmaps", collection.BeatmapCount),
                    x + 170f * Dp,
                    y,
                    width - 280f * Dp,
                    CollectionRowHeight,
                    12f * Dp,
                    s_propertiesSecondary,
                    UiTextAlignment.Left,
                    action
                )
            );
        }

        if (filterMode)
        {
            bool isDefaultSelected = slot == 0 && selectedFilter is null;
            if (
                isDefaultSelected
                || string.Equals(collection.Name, selectedFilter, StringComparison.Ordinal)
            )
            {
                elements.Add(
                    MaterialIcon(
                        $"songselect-collection-{slot}-selected",
                        UiMaterialIcon.Check,
                        new UiRect(x + width - 40f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp),
                        s_white,
                        1f,
                        action
                    )
                );
            }

            return;
        }

        AddSmallAction(
            elements,
            $"songselect-collection-{slot}-delete",
            UiMaterialIcon.Delete,
            x + width - 112f * Dp,
            y,
            CollectionDeleteAction(slot),
            s_propertiesDanger
        );
        AddSmallAction(
            elements,
            $"songselect-collection-{slot}-toggle",
            collection.ContainsSelectedSet ? UiMaterialIcon.Minus : UiMaterialIcon.Plus,
            x + width - 56f * Dp,
            y,
            CollectionToggleAction(slot),
            s_white
        );
    }

    private static void AddSmallAction(
        List<UiElementSnapshot> elements,
        string id,
        UiMaterialIcon icon,
        float x,
        float y,
        UiAction action,
        UiColor color
    )
    {
        elements.Add(
            Fill(
                id + "-hit",
                new UiRect(x, y, 56f * Dp, CollectionRowHeight),
                s_propertiesPanel,
                1f,
                action
            )
        );
        elements.Add(
            MaterialIcon(
                id,
                icon,
                new UiRect(x + 16f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp),
                color,
                1f,
                action
            )
        );
    }
}
