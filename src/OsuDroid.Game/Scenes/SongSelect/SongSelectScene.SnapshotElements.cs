using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) =>
        UiElementFactory.Fill(id, bounds, color, alpha, action, radius);

    private static UiElementSnapshot Sprite(string id, string asset, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        UiElementFactory.Sprite(id, asset, bounds, color, alpha, action);

    private static UiElementSnapshot Text(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        UiElementFactory.Text(id, text, new UiRect(x, y, width, height), size, color, action, alignment: alignment);

    private static UiElementSnapshot TextMiddle(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        UiElementFactory.Text(id, text, new UiRect(x, y, width, height), size, color, action, alignment: alignment, verticalAlignment: UiTextVerticalAlignment.Middle);

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action);
}
