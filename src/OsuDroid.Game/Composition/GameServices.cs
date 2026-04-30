using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game.Composition;

public sealed record GameServices(
    DroidDatabase Database,
    DroidGamePathLayout Paths,
    string BuildType,
    string DisplayVersion = "1.0",
    IMenuMusicController? MusicController = null,
    MenuNowPlayingState? NowPlaying = null,
    IBeatmapLibrary? BeatmapLibrary = null,
    IBeatmapImportService? BeatmapImportService = null,
    IBeatmapProcessingService? BeatmapProcessingService = null,
    IBeatmapDownloadService? BeatmapDownloadService = null,
    IBeatmapMirrorClient? BeatmapMirrorClient = null,
    ITextInputService? TextInputService = null,
    IBeatmapPreviewPlayer? BeatmapPreviewPlayer = null,
    IBeatmapDifficultyService? BeatmapDifficultyService = null,
    IGameSettingsStore? SettingsStore = null,
    IMenuSfxPlayer? MenuSfxPlayer = null,
    IOnlineLoginClient? OnlineLoginClient = null,
    OnlineProfileSnapshot? OnlineProfile = null,
    bool ShowStartupScene = false
)
{
    public string CorePath => Paths.CoreRoot;
}
