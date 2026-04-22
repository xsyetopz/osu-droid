using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new("SongSelect", "Song Select", string.Empty, Array.Empty<string>(), 0, false, CreateFrame(viewport));

    public static int SetIndex(UiAction action) => action switch
    {
        UiAction.SongSelectSet0 or UiAction.SongSelectFirstSet => 0,
        UiAction.SongSelectSet1 => 1,
        UiAction.SongSelectSet2 => 2,
        UiAction.SongSelectSet3 => 3,
        UiAction.SongSelectSet4 => 4,
        UiAction.SongSelectSet5 => 5,
        UiAction.SongSelectSet6 => 6,
        UiAction.SongSelectSet7 => 7,
        _ => -1,
    };

    public static int DifficultyIndex(UiAction action) => action switch
    {
        UiAction.SongSelectDifficulty0 => 0,
        UiAction.SongSelectDifficulty1 => 1,
        UiAction.SongSelectDifficulty2 => 2,
        UiAction.SongSelectDifficulty3 => 3,
        UiAction.SongSelectDifficulty4 => 4,
        UiAction.SongSelectDifficulty5 => 5,
        UiAction.SongSelectDifficulty6 => 6,
        UiAction.SongSelectDifficulty7 => 7,
        UiAction.SongSelectDifficulty8 => 8,
        UiAction.SongSelectDifficulty9 => 9,
        UiAction.SongSelectDifficulty10 => 10,
        UiAction.SongSelectDifficulty11 => 11,
        UiAction.SongSelectDifficulty12 => 12,
        UiAction.SongSelectDifficulty13 => 13,
        UiAction.SongSelectDifficulty14 => 14,
        UiAction.SongSelectDifficulty15 => 15,
        _ => -1,
    };
}
