#if ANDROID || IOS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using MauiColor = Microsoft.Maui.Graphics.Color;
using XnaGameRunBehavior = Microsoft.Xna.Framework.GameRunBehavior;
using OsuDroid.App.MonoGame.Bootstrap;
using OsuDroid.App.Platform;
using OsuDroid.App.MonoGame;
using OsuDroid.App.Platform.Audio;
using OsuDroid.App.Platform.Input;
using OsuDroid.Game;
using OsuDroid.Game.Runtime;

namespace OsuDroid.App;

public sealed class MainPage : ContentPage
{
    private readonly IPlatformPaths platformPaths;
    private readonly PlatformTextInputService textInputService = new();
    private readonly PlatformBeatmapPreviewPlayer previewPlayer = new();
    private readonly PlatformMenuSfxPlayer menuSfxPlayer = new(Path.Combine(AppContext.BaseDirectory, "assets", "droid", "sfx"));
    private OsuDroidMonoGame? monoGame;

    public MainPage(IServiceProvider services)
    {
        platformPaths = services.GetRequiredService<IPlatformPaths>();
        BackgroundColor = MauiColor.FromArgb("#000000");
        Content = new Grid();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void AttachPlatformServices(OsuDroidGameCore game)
    {
        var audioStart = PerfDiagnostics.Start();
        _ = BassAudioEngine.EnsureReady();
        PerfDiagnostics.Log("bootstrap.bassInit", audioStart);
        menuSfxPlayer.Preload("welcome", "welcome_piano", "seeya", "menuclick", "menuhit", "menuback", "click-short", "click-short-confirm", "check-on", "check-off");
        textInputService.Attach();
        game.AttachPlatformServices(textInputService, previewPlayer, menuSfxPlayer);
    }

    private void OnLoaded(object? sender, EventArgs args)
    {
        if (monoGame is not null)
            return;

        var bootstrapper = new GameBootstrapper(
            () => OsuDroidGameCore.Create(
                platformPaths.Roots,
#if DEBUG
                "debug",
#else
                "release",
#endif
                AppInfo.Current.VersionString,
                showStartupScene: true),
            AttachPlatformServices);

        monoGame = new OsuDroidMonoGame(bootstrapper);
        monoGame.Run(XnaGameRunBehavior.Asynchronous);
    }

    private void OnUnloaded(object? sender, EventArgs args)
    {
        monoGame?.Dispose();
        monoGame = null;
        textInputService.Detach();
        previewPlayer.StopPreview();
        menuSfxPlayer.Dispose();
    }
}
#endif
