using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.UI.Actions;

namespace OsuDroid.Game.Tests;

public sealed partial class ModSelectSceneTests
{
    private static void OpenSoloRoute(OsuDroidGameCore core)
    {
        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuPrimaryButton);
        core.HandleUiAction(UiAction.MainMenuPrimaryButton);
    }

    private static BeatmapInfo CreateBeatmap() =>
        new(
            "Insane.osu",
            "1 capsule - JUMPER",
            "md5",
            null,
            "audio.mp3",
            null,
            null,
            1,
            "JUMPER",
            string.Empty,
            "capsule",
            string.Empty,
            "Mafiamaster",
            "Insane",
            string.Empty,
            string.Empty,
            0,
            7,
            7,
            5,
            7,
            3.96f,
            4.7f,
            130,
            130,
            130,
            238000,
            0,
            258,
            221,
            1,
            766,
            false
        );
}
