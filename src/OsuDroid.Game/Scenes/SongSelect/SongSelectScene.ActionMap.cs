using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private static readonly UiAction[] SetActions =
    [
        UiAction.SongSelectSet0,
        UiAction.SongSelectSet1,
        UiAction.SongSelectSet2,
        UiAction.SongSelectSet3,
        UiAction.SongSelectSet4,
        UiAction.SongSelectSet5,
        UiAction.SongSelectSet6,
        UiAction.SongSelectSet7,
    ];

    private static readonly UiAction[] DifficultyActions =
    [
        UiAction.SongSelectDifficulty0,
        UiAction.SongSelectDifficulty1,
        UiAction.SongSelectDifficulty2,
        UiAction.SongSelectDifficulty3,
        UiAction.SongSelectDifficulty4,
        UiAction.SongSelectDifficulty5,
        UiAction.SongSelectDifficulty6,
        UiAction.SongSelectDifficulty7,
        UiAction.SongSelectDifficulty8,
        UiAction.SongSelectDifficulty9,
        UiAction.SongSelectDifficulty10,
        UiAction.SongSelectDifficulty11,
        UiAction.SongSelectDifficulty12,
        UiAction.SongSelectDifficulty13,
        UiAction.SongSelectDifficulty14,
        UiAction.SongSelectDifficulty15,
    ];

    private static readonly UiAction[] CollectionToggleActions =
    [
        UiAction.SongSelectCollectionToggle0,
        UiAction.SongSelectCollectionToggle1,
        UiAction.SongSelectCollectionToggle2,
        UiAction.SongSelectCollectionToggle3,
        UiAction.SongSelectCollectionToggle4,
        UiAction.SongSelectCollectionToggle5,
        UiAction.SongSelectCollectionToggle6,
        UiAction.SongSelectCollectionToggle7,
    ];

    private static readonly UiAction[] CollectionDeleteActions =
    [
        UiAction.SongSelectCollectionDelete0,
        UiAction.SongSelectCollectionDelete1,
        UiAction.SongSelectCollectionDelete2,
        UiAction.SongSelectCollectionDelete3,
        UiAction.SongSelectCollectionDelete4,
        UiAction.SongSelectCollectionDelete5,
        UiAction.SongSelectCollectionDelete6,
        UiAction.SongSelectCollectionDelete7,
    ];

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new("SongSelect", "Song Select", string.Empty, Array.Empty<string>(), 0, false, CreateFrame(viewport));

    public static UiAction SetAction(int visibleSlot) => ActionAt(SetActions, visibleSlot);

    public static int SetIndex(UiAction action) => UiActionGroups.TryGetSongSelectSetIndex(action, out var index) ? index : -1;

    public static UiAction DifficultyAction(int index) => ActionAt(DifficultyActions, index);

    public static int DifficultyIndex(UiAction action) => UiActionGroups.TryGetSongSelectDifficultyIndex(action, out var index) ? index : -1;

    public static UiAction CollectionToggleAction(int index) => ActionAt(CollectionToggleActions, index);

    public static int CollectionToggleIndex(UiAction action) => UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out var index) ? index : -1;

    public static UiAction CollectionDeleteAction(int index) => ActionAt(CollectionDeleteActions, index);

    public static int CollectionDeleteIndex(UiAction action) => UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out var index) ? index : -1;

    private static UiAction ActionAt(UiAction[] actions, int index) => index >= 0 && index < actions.Length ? actions[index] : UiAction.None;
}
