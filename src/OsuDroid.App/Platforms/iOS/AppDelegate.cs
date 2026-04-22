using Foundation;
using OsuDroid.App.MonoGame;
using OsuDroid.App.MonoGame.Bootstrap;
using OsuDroid.App.Platform.Audio;
using OsuDroid.App.Platform.Input;
using OsuDroid.Game;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using UIKit;

namespace OsuDroid.App;

[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    private OsuDroidMonoGame? game;
    private PlatformTextInputService? textInputService;
    private PlatformBeatmapPreviewPlayer? previewPlayer;
    private PlatformMenuSfxPlayer? menuSfxPlayer;

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        application.IdleTimerDisabled = true;
        textInputService = new PlatformTextInputService();
        previewPlayer = new PlatformBeatmapPreviewPlayer();
        menuSfxPlayer = new PlatformMenuSfxPlayer(Path.Combine(NSBundle.MainBundle.ResourcePath!, "assets", "droid", "sfx"));

        var bootstrapper = new GameBootstrapper(
            () => OsuDroidGameCore.Create(GetPathRoots(), BuildType, DisplayVersion, showStartupScene: true),
            AttachPlatformServices);

        game = new OsuDroidMonoGame(bootstrapper);
        game.Run();
        return true;
    }

    private void AttachPlatformServices(OsuDroidGameCore core)
    {
        var audioStart = PerfDiagnostics.Start();
        _ = BassAudioEngine.EnsureReady();
        PerfDiagnostics.Log("bootstrap.bassInit", audioStart);
        menuSfxPlayer?.Preload("welcome", "welcome_piano", "seeya", "menuclick", "menuhit", "menuback", "click-short", "click-short-confirm", "check-on", "check-off");
        textInputService?.Attach();
        core.AttachPlatformServices(textInputService, previewPlayer, menuSfxPlayer);
    }

    public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow? forWindow) =>
        UIInterfaceOrientationMask.Landscape;

    public override void WillTerminate(UIApplication application)
    {
        game?.Dispose();
        game = null;
        textInputService?.Detach();
        textInputService = null;
        previewPlayer?.StopPreview();
        previewPlayer = null;
        menuSfxPlayer?.Dispose();
        menuSfxPlayer = null;
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
        return new DroidPathRoots(Path.Combine(libraryPath, "osu-droid"), cachePath);
    }
}
