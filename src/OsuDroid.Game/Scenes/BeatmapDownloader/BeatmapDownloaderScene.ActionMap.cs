using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    public static int DownloadIndex(UiAction action) => action switch
    {
        UiAction.DownloaderDownloadFirst or UiAction.DownloaderDownloadFirstNoVideo or UiAction.DownloaderDownload0 or UiAction.DownloaderDownloadNoVideo0 => 0,
        UiAction.DownloaderDownload1 or UiAction.DownloaderDownloadNoVideo1 => 1,
        UiAction.DownloaderDownload2 or UiAction.DownloaderDownloadNoVideo2 => 2,
        UiAction.DownloaderDownload3 or UiAction.DownloaderDownloadNoVideo3 => 3,
        UiAction.DownloaderDownload4 or UiAction.DownloaderDownloadNoVideo4 => 4,
        UiAction.DownloaderDownload5 or UiAction.DownloaderDownloadNoVideo5 => 5,
        UiAction.DownloaderDownload6 or UiAction.DownloaderDownloadNoVideo6 => 6,
        UiAction.DownloaderDownload7 or UiAction.DownloaderDownloadNoVideo7 => 7,
        _ => -1,
    };

    public static bool IsNoVideoAction(UiAction action) => action is
        UiAction.DownloaderDownloadFirstNoVideo or UiAction.DownloaderDownloadNoVideo0 or UiAction.DownloaderDownloadNoVideo1 or UiAction.DownloaderDownloadNoVideo2 or UiAction.DownloaderDownloadNoVideo3 or
        UiAction.DownloaderDownloadNoVideo4 or UiAction.DownloaderDownloadNoVideo5 or UiAction.DownloaderDownloadNoVideo6 or UiAction.DownloaderDownloadNoVideo7;

    public static int CardIndex(UiAction action) => action switch
    {
        UiAction.DownloaderCard0 => 0,
        UiAction.DownloaderCard1 => 1,
        UiAction.DownloaderCard2 => 2,
        UiAction.DownloaderCard3 => 3,
        UiAction.DownloaderCard4 => 4,
        UiAction.DownloaderCard5 => 5,
        UiAction.DownloaderCard6 => 6,
        UiAction.DownloaderCard7 => 7,
        _ => -1,
    };

    public static int PreviewIndex(UiAction action) => action switch
    {
        UiAction.DownloaderPreview0 => 0,
        UiAction.DownloaderPreview1 => 1,
        UiAction.DownloaderPreview2 => 2,
        UiAction.DownloaderPreview3 => 3,
        UiAction.DownloaderPreview4 => 4,
        UiAction.DownloaderPreview5 => 5,
        UiAction.DownloaderPreview6 => 6,
        UiAction.DownloaderPreview7 => 7,
        _ => -1,
    };

    public static int DifficultyIndex(UiAction action) => action switch
    {
        UiAction.DownloaderDetailsDifficulty0 => 0,
        UiAction.DownloaderDetailsDifficulty1 => 1,
        UiAction.DownloaderDetailsDifficulty2 => 2,
        UiAction.DownloaderDetailsDifficulty3 => 3,
        UiAction.DownloaderDetailsDifficulty4 => 4,
        UiAction.DownloaderDetailsDifficulty5 => 5,
        UiAction.DownloaderDetailsDifficulty6 => 6,
        UiAction.DownloaderDetailsDifficulty7 => 7,
        UiAction.DownloaderDetailsDifficulty8 => 8,
        UiAction.DownloaderDetailsDifficulty9 => 9,
        UiAction.DownloaderDetailsDifficulty10 => 10,
        UiAction.DownloaderDetailsDifficulty11 => 11,
        UiAction.DownloaderDetailsDifficulty12 => 12,
        UiAction.DownloaderDetailsDifficulty13 => 13,
        UiAction.DownloaderDetailsDifficulty14 => 14,
        UiAction.DownloaderDetailsDifficulty15 => 15,
        _ => -1,
    };

    private static UiAction CardAction(int index) => index switch
    {
        0 => UiAction.DownloaderCard0,
        1 => UiAction.DownloaderCard1,
        2 => UiAction.DownloaderCard2,
        3 => UiAction.DownloaderCard3,
        4 => UiAction.DownloaderCard4,
        5 => UiAction.DownloaderCard5,
        6 => UiAction.DownloaderCard6,
        7 => UiAction.DownloaderCard7,
        _ => UiAction.None,
    };

    private static UiAction PreviewAction(int index) => index switch
    {
        0 => UiAction.DownloaderPreview0,
        1 => UiAction.DownloaderPreview1,
        2 => UiAction.DownloaderPreview2,
        3 => UiAction.DownloaderPreview3,
        4 => UiAction.DownloaderPreview4,
        5 => UiAction.DownloaderPreview5,
        6 => UiAction.DownloaderPreview6,
        7 => UiAction.DownloaderPreview7,
        _ => UiAction.None,
    };

    private static UiAction DownloadAction(int index) => index switch
    {
        0 => UiAction.DownloaderDownload0,
        1 => UiAction.DownloaderDownload1,
        2 => UiAction.DownloaderDownload2,
        3 => UiAction.DownloaderDownload3,
        4 => UiAction.DownloaderDownload4,
        5 => UiAction.DownloaderDownload5,
        6 => UiAction.DownloaderDownload6,
        7 => UiAction.DownloaderDownload7,
        _ => UiAction.None,
    };

    private static UiAction NoVideoAction(int index) => index switch
    {
        0 => UiAction.DownloaderDownloadNoVideo0,
        1 => UiAction.DownloaderDownloadNoVideo1,
        2 => UiAction.DownloaderDownloadNoVideo2,
        3 => UiAction.DownloaderDownloadNoVideo3,
        4 => UiAction.DownloaderDownloadNoVideo4,
        5 => UiAction.DownloaderDownloadNoVideo5,
        6 => UiAction.DownloaderDownloadNoVideo6,
        7 => UiAction.DownloaderDownloadNoVideo7,
        _ => UiAction.None,
    };

    private static UiAction DifficultyAction(int index) => index switch
    {
        0 => UiAction.DownloaderDetailsDifficulty0,
        1 => UiAction.DownloaderDetailsDifficulty1,
        2 => UiAction.DownloaderDetailsDifficulty2,
        3 => UiAction.DownloaderDetailsDifficulty3,
        4 => UiAction.DownloaderDetailsDifficulty4,
        5 => UiAction.DownloaderDetailsDifficulty5,
        6 => UiAction.DownloaderDetailsDifficulty6,
        7 => UiAction.DownloaderDetailsDifficulty7,
        8 => UiAction.DownloaderDetailsDifficulty8,
        9 => UiAction.DownloaderDetailsDifficulty9,
        10 => UiAction.DownloaderDetailsDifficulty10,
        11 => UiAction.DownloaderDetailsDifficulty11,
        12 => UiAction.DownloaderDetailsDifficulty12,
        13 => UiAction.DownloaderDetailsDifficulty13,
        14 => UiAction.DownloaderDetailsDifficulty14,
        15 => UiAction.DownloaderDetailsDifficulty15,
        _ => UiAction.None,
    };
}
