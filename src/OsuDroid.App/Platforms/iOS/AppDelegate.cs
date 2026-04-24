using Foundation;
using OsuDroid.App.MonoGame;
using OsuDroid.App.MonoGame.Bootstrap;
using OsuDroid.App.Platform;
using OsuDroid.Game;
using OsuDroid.Game.Runtime.Paths;
using UIKit;

namespace OsuDroid.App;

[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    private OsuDroidMonoGame? game;
    private PlatformRuntimeServices? runtimeServices;

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        application.IdleTimerDisabled = true;
        DroidPathRoots pathRoots = GetPathRoots();
        CrashLogInstaller.Install(pathRoots);
        runtimeServices = new PlatformRuntimeServices(Path.Combine(NSBundle.MainBundle.ResourcePath!, "assets", "droid", "sfx"));

        var bootstrapper = new GameBootstrapper(
            () => OsuDroidGameCore.Create(pathRoots, BuildType, DisplayVersion, showStartupScene: true),
            runtimeServices.AttachTo);

        game = new OsuDroidMonoGame(bootstrapper);
        game.Run();
        return true;
    }

    public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow? forWindow) =>
        UIInterfaceOrientationMask.Landscape;

    public override void WillTerminate(UIApplication application)
    {
        game?.Dispose();
        game = null;
        runtimeServices?.Dispose();
        runtimeServices = null;
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

    private static string DisplayVersion =>
        NSBundle.MainBundle.InfoDictionary?["CFBundleShortVersionString"]?.ToString() ?? "1.0";

    private static DroidPathRoots GetPathRoots()
    {
        var libraryPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0];
        var cachePath = NSSearchPath.GetDirectories(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User)[0];
        return DroidPathRoots.FromAppDataDirectory(libraryPath, cachePath);
    }
}
