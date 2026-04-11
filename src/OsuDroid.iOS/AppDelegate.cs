using Foundation;
using OsuDroid.Game;
using OsuDroid.Game.Services.Stubs;
using OsuDroid.iOS.Platform.Audio;
using OsuDroid.iOS.Platform.External;
using OsuDroid.iOS.Platform.Storage;
using osu.Framework.iOS;
using UIKit;

namespace OsuDroid.iOS;

[Register("AppDelegate")]
public class AppDelegate : GameApplicationDelegate, IUIApplicationDelegate
{
    protected override osu.Framework.Game CreateGame() =>
        createGame();

    private static OsuDroidGame createGame()
    {
        var authState = new StubAuthState();

        return new OsuDroidGame(
            new IOSAudioService(),
            new StubAccountService(authState),
            new StubSessionService(authState),
            new StubBeatmapLibraryService(),
            new IOSExternalUriLauncher(),
            new IOSPlatformStorage());
    }
}
