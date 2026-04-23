#if ANDROID || IOS
using OsuDroid.App.Platform.Audio;
using OsuDroid.App.Platform.Input;
using OsuDroid.Game;
using OsuDroid.Game.Runtime;

namespace OsuDroid.App.Platform;

public sealed class PlatformRuntimeServices(string sfxAssetsRoot) : IDisposable
{
    private static readonly string[] preloadedMenuSfxKeys =
    [
        "welcome",
        "welcome_piano",
        "seeya",
        "menuclick",
        "menuhit",
        "menuback",
        "click-short",
        "click-short-confirm",
        "check-on",
        "check-off",
    ];

    private readonly object gate = new();
    private readonly PlatformTextInputService textInputService = new();
    private readonly PlatformBeatmapPreviewPlayer previewPlayer = new();
    private readonly PlatformMenuSfxPlayer menuSfxPlayer = new(sfxAssetsRoot);
    private bool disposed;

    public void AttachTo(OsuDroidGameCore core)
    {
        lock (gate)
        {
            if (disposed)
                return;

            var audioStart = PerfDiagnostics.Start();
            _ = BassAudioEngine.EnsureReady();
            PerfDiagnostics.Log("bootstrap.bassInit", audioStart);
            menuSfxPlayer.Preload(preloadedMenuSfxKeys);
            textInputService.Attach();
            core.AttachPlatformServices(textInputService, previewPlayer, menuSfxPlayer);
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed)
                return;

            disposed = true;
            textInputService.Detach();
            previewPlayer.Dispose();
            menuSfxPlayer.Dispose();
        }
    }
}
#endif
