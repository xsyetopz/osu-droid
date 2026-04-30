using System.Globalization;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private void AddPropertiesPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        BeatmapOptions options = CurrentOptions() ?? new BeatmapOptions(string.Empty);
        float panelHeight = PropertiesRowHeight * 5f;
        var panel = new UiRect(
            (viewport.VirtualWidth - PropertiesWidth) / 2f,
            (viewport.VirtualHeight - panelHeight) / 2f,
            PropertiesWidth,
            panelHeight
        );
        elements.Add(
            Fill(
                "songselect-properties-panel",
                panel,
                s_propertiesPanel,
                1f,
                UiAction.SongSelectPropertiesPanel,
                14f * Dp
            )
        );

        AddPropertiesRowText(
            elements,
            "songselect-properties-title",
            _localizer["SongSelect_PropertiesTitle"],
            panel.X,
            panel.Y,
            panel.Width,
            PropertiesRowHeight,
            15f * Dp,
            s_white,
            UiAction.SongSelectPropertiesPanel
        );
        AddDivider(
            elements,
            "songselect-properties-divider-title",
            panel.X,
            panel.Y + PropertiesRowHeight,
            panel.Width
        );

        float offsetY = panel.Y + PropertiesRowHeight;
        float buttonWidth = 70f * Dp;
        elements.Add(
            Fill(
                "songselect-properties-offset-minus-hit",
                new UiRect(panel.X, offsetY, buttonWidth, PropertiesRowHeight),
                s_propertiesPanel,
                1f,
                UiAction.SongSelectPropertiesOffsetMinus
            )
        );
        elements.Add(
            MaterialIcon(
                "songselect-properties-offset-minus",
                UiMaterialIcon.Minus,
                new UiRect(
                    panel.X + (buttonWidth - 24f * Dp) / 2f,
                    offsetY + 14f * Dp,
                    24f * Dp,
                    24f * Dp
                ),
                s_white,
                1f,
                UiAction.SongSelectPropertiesOffsetMinus
            )
        );
        UiRect input = PropertiesOffsetInputBounds(viewport);
        elements.Add(
            Fill(
                "songselect-properties-offset-input-hit",
                input,
                s_propertiesPanel,
                1f,
                UiAction.SongSelectPropertiesOffsetInput
            )
        );
        AddPropertiesRowText(
            elements,
            "songselect-properties-offset-label",
            _localizer["SongSelect_Offset"],
            input.X,
            input.Y + 4f * Dp,
            input.Width,
            16f * Dp,
            10f * Dp,
            s_propertiesSecondary,
            UiAction.SongSelectPropertiesOffsetInput
        );
        AddPropertiesRowText(
            elements,
            "songselect-properties-offset-value",
            options.Offset.ToString(CultureInfo.InvariantCulture),
            input.X,
            input.Y + 18f * Dp,
            input.Width,
            30f * Dp,
            18f * Dp,
            s_white,
            UiAction.SongSelectPropertiesOffsetInput
        );
        elements.Add(
            Fill(
                "songselect-properties-offset-plus-hit",
                new UiRect(panel.Right - buttonWidth, offsetY, buttonWidth, PropertiesRowHeight),
                s_propertiesPanel,
                1f,
                UiAction.SongSelectPropertiesOffsetPlus
            )
        );
        elements.Add(
            MaterialIcon(
                "songselect-properties-offset-plus",
                UiMaterialIcon.Plus,
                new UiRect(
                    panel.Right - buttonWidth + (buttonWidth - 24f * Dp) / 2f,
                    offsetY + 14f * Dp,
                    24f * Dp,
                    24f * Dp
                ),
                s_white,
                1f,
                UiAction.SongSelectPropertiesOffsetPlus
            )
        );
        AddDivider(
            elements,
            "songselect-properties-divider-offset",
            panel.X,
            offsetY + PropertiesRowHeight,
            panel.Width
        );

        float favoriteY = offsetY + PropertiesRowHeight;
        AddIconRow(
            elements,
            "songselect-properties-favorite",
            options.IsFavorite ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline,
            _localizer["SongSelect_AddToFavorites"],
            panel.X,
            favoriteY,
            panel.Width,
            UiAction.SongSelectPropertiesFavorite,
            s_white
        );
        AddDivider(
            elements,
            "songselect-properties-divider-favorite",
            panel.X,
            favoriteY + PropertiesRowHeight,
            panel.Width
        );

        float manageY = favoriteY + PropertiesRowHeight;
        AddIconRow(
            elements,
            "songselect-properties-manage",
            UiMaterialIcon.Folder,
            _localizer["SongSelect_ManageFavorites"],
            panel.X,
            manageY,
            panel.Width,
            UiAction.SongSelectPropertiesManageCollections,
            s_white
        );
        AddDivider(
            elements,
            "songselect-properties-divider-manage",
            panel.X,
            manageY + PropertiesRowHeight,
            panel.Width
        );

        AddIconRow(
            elements,
            "songselect-properties-delete",
            UiMaterialIcon.Delete,
            _localizer["SongSelect_DeleteBeatmapTitle"],
            panel.X,
            manageY + PropertiesRowHeight,
            panel.Width,
            UiAction.SongSelectPropertiesDelete,
            s_propertiesDanger
        );
    }

    private static void AddPropertiesRowText(
        List<UiElementSnapshot> elements,
        string id,
        string text,
        float x,
        float y,
        float width,
        float height,
        float size,
        UiColor color,
        UiAction action
    ) =>
        elements.Add(
            TextMiddle(id, text, x, y, width, height, size, color, UiTextAlignment.Center, action)
        );

    private static void AddDivider(
        List<UiElementSnapshot> elements,
        string id,
        float x,
        float y,
        float width
    ) => elements.Add(Fill(id, new UiRect(x, y, width, 1f * Dp), s_propertiesDivider, 1f));
}
