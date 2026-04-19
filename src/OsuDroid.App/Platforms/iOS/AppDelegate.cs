using Foundation;
using OsuDroid.App.MonoGame;
using OsuDroid.Game;
using UIKit;

namespace OsuDroid.App;

[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    private OsuDroidMonoGame? game;

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        application.IdleTimerDisabled = true;
        game = new OsuDroidMonoGame(OsuDroidGameCore.Create(GetCorePath(), BuildType));
        game.Run();
        return true;
    }

    public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow? forWindow) =>
        UIInterfaceOrientationMask.Landscape;

    public override void WillTerminate(UIApplication application)
    {
        game?.Dispose();
        game = null;
        base.WillTerminate(application);
    }

    private static string BuildType
    {
        get
        {
#if DEBUG
            return "debug";
#else
            return "release";
#endif
        }
    }

    private static string GetCorePath()
    {
        var libraryPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0];
        var corePath = Path.Combine(libraryPath, "osu-droid");
        Directory.CreateDirectory(corePath);
        return corePath;
    }
}
