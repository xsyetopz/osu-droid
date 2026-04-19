#if ANDROID || IOS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using MauiColor = Microsoft.Maui.Graphics.Color;
using XnaGameRunBehavior = Microsoft.Xna.Framework.GameRunBehavior;
using OsuDroid.App.MonoGame;
using OsuDroid.Game;

namespace OsuDroid.App;

public sealed class MainPage : ContentPage
{
    private readonly OsuDroidGameCore game;
    private OsuDroidMonoGame? monoGame;

    public MainPage(IServiceProvider services)
    {
        game = services.GetRequiredService<OsuDroidGameCore>();
        BackgroundColor = MauiColor.FromArgb("#4681fc");
        Content = new Grid();
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
    }
}
#endif
