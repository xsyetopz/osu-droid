namespace OsuDroid.Game.UI;

public enum UiAssetProvenance
{
    LegacyOsuDroid,
    OfficialOsu,
    PortLocal,
}

public enum UiAssetKind
{
    Texture,
    Sound,
    Font,
}

public sealed record UiAssetEntry(
    string LogicalName,
    string PackagePath,
    UiAssetKind Kind,
    UiAssetProvenance Provenance,
    UiSize NativeSize);

public sealed class UiAssetManifest
{
    private readonly Dictionary<string, UiAssetEntry> entriesByName;

    public UiAssetManifest(IEnumerable<UiAssetEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        entriesByName = entries.ToDictionary(static entry => entry.LogicalName, StringComparer.Ordinal);
    }

    public IReadOnlyCollection<UiAssetEntry> Entries => entriesByName.Values;

    public UiAssetEntry Get(string logicalName) => entriesByName[logicalName];

    public bool Contains(string logicalName) => entriesByName.ContainsKey(logicalName);
}

public static class LegacyUiAssets
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

    public static UiAssetManifest MainMenuManifest { get; } = new(CreateMainMenuEntries());

    private static IEnumerable<UiAssetEntry> CreateMainMenuEntries()
    {
        yield return Texture(MenuBackground, "legacy/gfx/menu-background.png", 1500, 768);
        yield return Texture(EmptyAvatar, "legacy/gfx/emptyavatar.png", 90, 90);
        yield return Texture(BeatmapDownloader, "legacy/beatmap_downloader.png", 80, 284);
        yield return Texture(Logo, "legacy/logo.png", 540, 540);
        yield return Texture(Play, "legacy/play.png", 586, 92);
        yield return Texture(Solo, "legacy/solo.png", 586, 92);
        yield return Texture(Options, "legacy/options.png", 586, 92);
        yield return Texture(Multi, "legacy/multi.png", 586, 92);
        yield return Texture(Exit, "legacy/exit.png", 586, 92);
        yield return Texture(Back, "legacy/back.png", 583, 89);
        yield return Texture(MusicPrevious, "legacy/music_prev.png", 64, 62);
        yield return Texture(MusicPlay, "legacy/music_play.png", 60, 62);
        yield return Texture(MusicPause, "legacy/music_pause.png", 66, 66);
        yield return Texture(MusicStop, "legacy/music_stop.png", 66, 66);
        yield return Texture(MusicNext, "legacy/music_next.png", 64, 62);
        yield return Texture(MusicNowPlaying, "legacy/music_np.png", 1364, 60);
    }

    private static UiAssetEntry Texture(string logicalName, string packagePath, float width, float height) =>
        new(logicalName, packagePath, UiAssetKind.Texture, UiAssetProvenance.LegacyOsuDroid, new UiSize(width, height));
}
