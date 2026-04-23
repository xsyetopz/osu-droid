using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private static readonly UiAction[] s_setActions =
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

    private static readonly UiAction[] s_difficultyActions =
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

    private static readonly UiAction[] s_collectionToggleActions =
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

    private static readonly UiAction[] s_collectionDeleteActions =
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

    public static UiAction SetAction(int visibleSlot) => ActionAt(s_setActions, visibleSlot);

    public static int SetIndex(UiAction action) => UiActionGroups.TryGetSongSelectSetIndex(action, out int index) ? index : -1;

    public static UiAction DifficultyAction(int index) => ActionAt(s_difficultyActions, index);

    public static int DifficultyIndex(UiAction action) => UiActionGroups.TryGetSongSelectDifficultyIndex(action, out int index) ? index : -1;

    public static UiAction CollectionToggleAction(int index) => ActionAt(s_collectionToggleActions, index);

    public static int CollectionToggleIndex(UiAction action) => UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out int index) ? index : -1;

    public static UiAction CollectionDeleteAction(int index) => ActionAt(s_collectionDeleteActions, index);

    public static int CollectionDeleteIndex(UiAction action) => UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out int index) ? index : -1;

    private static UiAction ActionAt(UiAction[] actions, int index) => index >= 0 && index < actions.Length ? actions[index] : UiAction.None;
}
