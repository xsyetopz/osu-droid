#if ANDROID || IOS
using Microsoft.Extensions.DependencyInjection;
using MauiColor = Microsoft.Maui.Graphics.Color;
using XnaGameRunBehavior = Microsoft.Xna.Framework.GameRunBehavior;
using OsuDroid.App.MonoGame;
using OsuDroid.App.Platform.Audio;
using OsuDroid.App.Platform.Input;
using OsuDroid.Game;

namespace OsuDroid.App;

public sealed class MainPage : ContentPage
{
    private readonly OsuDroidGameCore game;
    private readonly PlatformTextInputService textInputService = new();
    private readonly PlatformBeatmapPreviewPlayer previewPlayer = new();
    private readonly PlatformMenuSfxPlayer menuSfxPlayer = new(Path.Combine(AppContext.BaseDirectory, "assets", "droid", "sfx"));
    private OsuDroidMonoGame? monoGame;

    public MainPage(IServiceProvider services)
    {
        game = services.GetRequiredService<OsuDroidGameCore>();
        BackgroundColor = MauiColor.FromArgb("#4681fc");
        Content = new Grid();
        textInputService.Attach();
        game.AttachPlatformServices(textInputService, previewPlayer, menuSfxPlayer);
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs args)
    {
        if (monoGame is not null)
            return;

        monoGame = new OsuDroidMonoGame(game);
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
