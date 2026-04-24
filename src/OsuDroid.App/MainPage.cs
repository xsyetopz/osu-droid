#if ANDROID || IOS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using MauiColor = Microsoft.Maui.Graphics.Color;
using XnaGameRunBehavior = Microsoft.Xna.Framework.GameRunBehavior;
using OsuDroid.App.MonoGame.Bootstrap;
using OsuDroid.App.Platform;
using OsuDroid.App.MonoGame;
using OsuDroid.Game;

namespace OsuDroid.App;

public sealed class MainPage : ContentPage
{
    private readonly IPlatformPaths platformPaths;
    private PlatformRuntimeServices? runtimeServices;
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
        runtimeServices?.AttachTo(game);
    }

    private void OnLoaded(object? sender, EventArgs args)
    {
        if (monoGame is not null)
            return;

        CrashLogInstaller.Install(platformPaths.Roots);
        runtimeServices = new PlatformRuntimeServices(Path.Combine(AppContext.BaseDirectory, "assets", "droid", "sfx"));
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
        runtimeServices?.Dispose();
        runtimeServices = null;
    }
}
#endif
