namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private sealed class SelectionState
    {
        public int SetIndex;
        public int DifficultyIndex;
        public float ScrollY;
        public float CollectionScrollY;
        public float SetExpansion = 1f;
    }

    private sealed class QueryState
    {
        public string SearchQuery = string.Empty;
        public bool FavoriteOnlyFilter;
        public string? CollectionFilter;
        public SongSelectSortMode SortMode = SongSelectSortMode.Title;
    }

    private sealed class BackgroundState
    {
        public string? Path;
        public string? BeatmapKey;
        public float Luminance = 1f;
    }
}
