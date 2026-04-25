using OsuDroid.Game.UI.Elements;
namespace OsuDroid.Game.UI.Frames;

public sealed record GameFrameSnapshot(
    string Scene,
    string Title,
    string Subtitle,
    IReadOnlyList<string> MenuEntries,
    int SelectedIndex,
    bool IsSecondMenu,
    UiFrameSnapshot UiFrame);
