using Android.App;
using Android.Content.PM;
using OsuDroid.Game;
using osu.Framework.Android;

namespace OsuDroid.Android;

[Activity(
    Label = "osu!droid",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AndroidGameActivity
{
    protected override osu.Framework.Game CreateGame() =>
        createGame();

    private OsuDroidGame createGame()
    {
        var authState = new StubAuthState();

        return new OsuDroidGame(
            new AndroidAudioService(),
            new StubAccountService(authState),
            new StubSessionService(authState),
            new StubBeatmapLibraryService(),
            new AndroidExternalUriLauncher(this),
            new AndroidPlatformStorage());
    }
}
