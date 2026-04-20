namespace OsuDroid.Game.UI;

public static class DroidAssets
{
    public const string MenuBackground = "menu-background";
    public const string EmptyAvatar = "emptyavatar";
    public const string BeatmapDownloader = "beatmap_downloader";
    public const string Logo = "logo";
    public const string Play = "play";
    public const string Solo = "solo";
    public const string Options = "options";
    public const string Multi = "multi";
    public const string Exit = "exit";
    public const string Back = "back";
    public const string MusicPrevious = "music_prev";
    public const string MusicPlay = "music_play";
    public const string MusicPause = "music_pause";
    public const string MusicStop = "music_stop";
    public const string MusicNext = "music_next";
    public const string MusicNowPlaying = "music_np";
    public const string SettingsArrowBack = "settings-arrow-back";
    public const string SettingsGeneral = "settings-general";
    public const string SettingsGameplay = "settings-gameplay";
    public const string SettingsGraphics = "settings-graphics";
    public const string SettingsAudio = "settings-audio";
    public const string SettingsLibrary = "settings-library";
    public const string SettingsInput = "settings-input";
    public const string SettingsAdvanced = "settings-advanced";
    public const string SettingsCheck = "settings-check";
    public const string SettingsArrowDropDown = "settings-arrow-drop-down";
    public const string BeatmapDownloaderOsuDirect = "beatmap-downloader-osudirect";
    public const string BeatmapDownloaderCatboy = "beatmap-downloader-catboy";

    public static UiAssetManifest MainMenuManifest { get; } = new(CreateMainMenuEntries());

    private static IEnumerable<UiAssetEntry> CreateMainMenuEntries()
    {
        yield return Texture(MenuBackground, "assets/droid/main-menu/background.png", 1500, 768);
        yield return Texture(EmptyAvatar, "assets/droid/common/empty-avatar.png", 90, 90);
        yield return Texture(BeatmapDownloader, "assets/droid/main-menu/beatmap-downloader-tab.png", 80, 284);
        yield return Texture(Logo, "assets/droid/main-menu/logo.png", 540, 540);
        yield return Texture(Play, "assets/droid/main-menu/play-button.png", 586, 92);
        yield return Texture(Solo, "assets/droid/main-menu/solo-button.png", 586, 92);
        yield return Texture(Options, "assets/droid/main-menu/options-button.png", 586, 92);
        yield return Texture(Multi, "assets/droid/main-menu/multiplayer-button.png", 586, 92);
        yield return Texture(Exit, "assets/droid/main-menu/exit-button.png", 586, 92);
        yield return Texture(Back, "assets/droid/main-menu/back-button.png", 583, 89);
        yield return Texture(MusicPrevious, "assets/droid/main-menu/music-previous.png", 64, 62);
        yield return Texture(MusicPlay, "assets/droid/main-menu/music-play.png", 60, 62);
        yield return Texture(MusicPause, "assets/droid/main-menu/music-pause.png", 66, 66);
        yield return Texture(MusicStop, "assets/droid/main-menu/music-stop.png", 66, 66);
        yield return Texture(MusicNext, "assets/droid/main-menu/music-next.png", 64, 62);
        yield return Texture(MusicNowPlaying, "assets/droid/main-menu/now-playing-panel.png", 1364, 60);
        yield return Texture(SettingsArrowBack, "assets/droid/settings/arrow-back.png", 192, 192);
        yield return Texture(SettingsGeneral, "assets/droid/settings/category-general.png", 192, 192);
        yield return Texture(SettingsGameplay, "assets/droid/settings/category-gameplay.png", 192, 192);
        yield return Texture(SettingsGraphics, "assets/droid/settings/category-graphics.png", 192, 192);
        yield return Texture(SettingsAudio, "assets/droid/settings/category-audio.png", 192, 192);
        yield return Texture(SettingsLibrary, "assets/droid/settings/category-library.png", 192, 192);
        yield return Texture(SettingsInput, "assets/droid/settings/category-input.png", 192, 192);
        yield return Texture(SettingsAdvanced, "assets/droid/settings/category-advanced.png", 192, 192);
        yield return Texture(SettingsCheck, "assets/droid/settings/check.png", 192, 192);
        yield return Texture(SettingsArrowDropDown, "assets/droid/settings/arrow-drop-down.png", 192, 192);
        yield return Texture(BeatmapDownloaderOsuDirect, "assets/droid/beatmap-downloader/osudirect.png", 96, 96);
        yield return Texture(BeatmapDownloaderCatboy, "assets/droid/beatmap-downloader/catboy.png", 96, 96);
    }

    private static UiAssetEntry Texture(string logicalName, string packagePath, float width, float height) =>
        new(logicalName, packagePath, UiAssetKind.Texture, UiAssetProvenance.OsuDroid, new UiSize(width, height));
}
