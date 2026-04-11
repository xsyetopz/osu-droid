using Foundation;
using OsuDroid.Game;
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

    private static OsuDroidGame createGame() => new(new IOSExternalUriLauncher(), new IOSPlatformStorage());
}
