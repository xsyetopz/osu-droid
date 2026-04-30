using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private static void AddFullWidthRow(
        List<UiElementSnapshot> elements,
        string id,
        string text,
        float x,
        float y,
        float width,
        UiAction action,
        UiColor color
    )
    {
        elements.Add(
            Fill(
                id + "-hit",
                new UiRect(x, y, width, PropertiesRowHeight),
                s_propertiesPanel,
                1f,
                action
            )
        );
        elements.Add(
            TextMiddle(
                id,
                text,
                x + 16f * Dp,
                y,
                width - 32f * Dp,
                PropertiesRowHeight,
                15f * Dp,
                color,
                UiTextAlignment.Left,
                action
            )
        );
    }

    private static void AddIconRow(
        List<UiElementSnapshot> elements,
        string id,
        UiMaterialIcon icon,
        string text,
        float x,
        float y,
        float width,
        UiAction action,
        UiColor color
    )
    {
        elements.Add(
            Fill(
                id + "-hit",
                new UiRect(x, y, width, PropertiesRowHeight),
                s_propertiesPanel,
                1f,
                action
            )
        );
        elements.Add(
            MaterialIcon(
                id + "-icon",
                icon,
                new UiRect(x + 24f * Dp, y + 14f * Dp, 24f * Dp, 24f * Dp),
                color,
                1f,
                action
            )
        );
        elements.Add(
            TextMiddle(
                id,
                text,
                x + 58f * Dp,
                y,
                width - 74f * Dp,
                PropertiesRowHeight,
                15f * Dp,
                color,
                UiTextAlignment.Left,
                action
            )
        );
    }
}
