using Foundation;
using OsuDroid.App.MonoGame;
using OsuDroid.App.Platform.Audio;
using OsuDroid.App.Platform.Input;
using OsuDroid.Game;
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
        var core = OsuDroidGameCore.Create(GetPathRoots(), BuildType);
        textInputService = new PlatformTextInputService();
        previewPlayer = new PlatformBeatmapPreviewPlayer();
        menuSfxPlayer = new PlatformMenuSfxPlayer(Path.Combine(NSBundle.MainBundle.ResourcePath!, "assets", "droid", "sfx"));
        textInputService.Attach();
        core.AttachPlatformServices(textInputService, previewPlayer, menuSfxPlayer);

        game = new OsuDroidMonoGame(core);
        game.Run();
        return true;
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

    private static DroidPathRoots GetPathRoots()
    {
        var libraryPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0];
        var cachePath = NSSearchPath.GetDirectories(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User)[0];
        return new DroidPathRoots(Path.Combine(libraryPath, "osu-droid"), cachePath);
    }
}
