using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private static UiAction SetAction(int visibleSlot) => visibleSlot switch
    {
        0 => UiAction.SongSelectSet0,
        1 => UiAction.SongSelectSet1,
        2 => UiAction.SongSelectSet2,
        3 => UiAction.SongSelectSet3,
        4 => UiAction.SongSelectSet4,
        5 => UiAction.SongSelectSet5,
        6 => UiAction.SongSelectSet6,
        7 => UiAction.SongSelectSet7,
        _ => UiAction.None,
    };

    private static UiAction DifficultyAction(int index) => index switch
    {
        0 => UiAction.SongSelectDifficulty0,
        1 => UiAction.SongSelectDifficulty1,
        2 => UiAction.SongSelectDifficulty2,
        3 => UiAction.SongSelectDifficulty3,
        4 => UiAction.SongSelectDifficulty4,
        5 => UiAction.SongSelectDifficulty5,
        6 => UiAction.SongSelectDifficulty6,
        7 => UiAction.SongSelectDifficulty7,
        8 => UiAction.SongSelectDifficulty8,
        9 => UiAction.SongSelectDifficulty9,
        10 => UiAction.SongSelectDifficulty10,
        11 => UiAction.SongSelectDifficulty11,
        12 => UiAction.SongSelectDifficulty12,
        13 => UiAction.SongSelectDifficulty13,
        14 => UiAction.SongSelectDifficulty14,
        15 => UiAction.SongSelectDifficulty15,
        _ => UiAction.None,
    };

    private static UiAction UiActionForCollectionToggle(int index) => index switch
    {
        0 => UiAction.SongSelectCollectionToggle0,
        1 => UiAction.SongSelectCollectionToggle1,
        2 => UiAction.SongSelectCollectionToggle2,
        3 => UiAction.SongSelectCollectionToggle3,
        4 => UiAction.SongSelectCollectionToggle4,
        5 => UiAction.SongSelectCollectionToggle5,
        6 => UiAction.SongSelectCollectionToggle6,
        7 => UiAction.SongSelectCollectionToggle7,
        _ => UiAction.None,
    };

    private static UiAction UiActionForCollectionDelete(int index) => index switch
    {
        0 => UiAction.SongSelectCollectionDelete0,
        1 => UiAction.SongSelectCollectionDelete1,
        2 => UiAction.SongSelectCollectionDelete2,
        3 => UiAction.SongSelectCollectionDelete3,
        4 => UiAction.SongSelectCollectionDelete4,
        5 => UiAction.SongSelectCollectionDelete5,
        6 => UiAction.SongSelectCollectionDelete6,
        7 => UiAction.SongSelectCollectionDelete7,
        _ => UiAction.None,
    };

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) =>
        new(id, UiElementKind.Fill, bounds, color, alpha, Action: action, CornerRadius: radius);

    private static UiElementSnapshot Sprite(string id, string asset, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        new(id, UiElementKind.Sprite, bounds, color, alpha, asset, action);

    private static UiElementSnapshot Text(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment));

    private static UiElementSnapshot TextMiddle(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment, VerticalAlignment: UiTextVerticalAlignment.Middle));

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        new(
            id,
            UiElementKind.MaterialIcon,
            bounds,
            color,
            alpha,
            Action: action,
            MaterialIcon: icon);
}
