#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using OsuDroid.App.MonoGame.Rendering;
using OsuDroid.Game;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaDisplayOrientation = Microsoft.Xna.Framework.DisplayOrientation;
#if IOS
using UIKit;
#endif

namespace OsuDroid.App.MonoGame;

public sealed class OsuDroidMonoGame : Microsoft.Xna.Framework.Game
{
    private const string RenderDiagnosticsEnvironmentVariable = "OSUDROID_RENDER_DIAGNOSTICS";

    private readonly OsuDroidGameCore core;
    private readonly GraphicsDeviceManager graphics;
    private MonoGameUiRenderer? renderer;
    private SpriteBatch? spriteBatch;
    private UiFrameSnapshot? frame;
    private string? lastRenderBoundsLog;
    private readonly bool showRenderDiagnostics = IsRenderDiagnosticsEnabled();
    private string? platformDisplayLog;

    public OsuDroidMonoGame(OsuDroidGameCore core)
    {
        this.core = core;
        graphics = new GraphicsDeviceManager(this)
        {
            SupportedOrientations = XnaDisplayOrientation.LandscapeLeft | XnaDisplayOrientation.LandscapeRight,
            IsFullScreen = true,
            PreferMultiSampling = true,
        };
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        ApplyFullScreenBackBuffer();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        renderer = new MonoGameUiRenderer(GraphicsDevice);
        LogRenderBoundsIfChanged("LoadContent");
    }

    protected override void UnloadContent()
    {
        renderer?.Dispose();
        spriteBatch?.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        LogRenderBoundsIfChanged("Update");
        core.Update(gameTime.ElapsedGameTime);
        frame = CreateCurrentFrame();
        RouteTouch(frame);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        frame ??= CreateCurrentFrame();
        GraphicsDevice.Clear(new XnaColor(70, 129, 252));

        if (spriteBatch is not null && renderer is not null)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            renderer.Draw(spriteBatch, frame);

            if (showRenderDiagnostics)
                renderer.DrawDiagnostics(spriteBatch, CreateRenderDiagnostics());

            spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    private void ApplyFullScreenBackBuffer()
    {
        var backBufferSize = GetPreferredBackBufferSize();

        graphics.PreferredBackBufferWidth = backBufferSize.Width;
        graphics.PreferredBackBufferHeight = backBufferSize.Height;
        graphics.IsFullScreen = true;
        graphics.ApplyChanges();

        if (GraphicsDevice is not null)
            GraphicsDevice.Viewport = new Viewport(0, 0, backBufferSize.Width, backBufferSize.Height);
    }

    private BackBufferSize GetPreferredBackBufferSize()
    {
#if IOS
        var screen = UIScreen.MainScreen;
        var nativeBounds = screen.NativeBounds;
        var logicalBounds = screen.Bounds;
        var width = (int)Math.Round(Math.Max(nativeBounds.Width, nativeBounds.Height));
        var height = (int)Math.Round(Math.Min(nativeBounds.Width, nativeBounds.Height));

        platformDisplayLog =
            $"iosNative={nativeBounds.Width:0}x{nativeBounds.Height:0} " +
            $"iosLogical={logicalBounds.Width:0.###}x{logicalBounds.Height:0.###} " +
            $"iosNativeScale={screen.NativeScale:0.###}";

        return new BackBufferSize(width, height);
#else
        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        var width = Math.Max(displayMode.Width, displayMode.Height);
        var height = Math.Min(displayMode.Width, displayMode.Height);

        platformDisplayLog = $"displayMode={displayMode.Width}x{displayMode.Height}";
        return new BackBufferSize(width, height);
#endif
    }

    private UiFrameSnapshot CreateCurrentFrame()
    {
        var viewport = VirtualViewport.FromSurface(
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height);
        return core.CreateFrame(viewport).UiFrame;
    }

    private RenderBoundsDiagnostics CreateRenderDiagnostics()
    {
        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        return new RenderBoundsDiagnostics(
            GraphicsDevice.Viewport.Bounds,
            Window.ClientBounds,
            displayMode.Width,
            displayMode.Height,
            graphics.PreferredBackBufferWidth,
            graphics.PreferredBackBufferHeight);
    }

    private void LogRenderBoundsIfChanged(string phase)
    {
        if (!showRenderDiagnostics || GraphicsDevice is null)
            return;

        var diagnostics = CreateRenderDiagnostics();
        var platformDisplay = platformDisplayLog ?? $"display={diagnostics.DisplayWidth}x{diagnostics.DisplayHeight}";
        var message =
            $"osu!droid render-bounds phase={phase} " +
            $"viewport={diagnostics.ViewportBounds.Width}x{diagnostics.ViewportBounds.Height}+{diagnostics.ViewportBounds.X},{diagnostics.ViewportBounds.Y} " +
            $"client={diagnostics.ClientBounds.Width}x{diagnostics.ClientBounds.Height}+{diagnostics.ClientBounds.X},{diagnostics.ClientBounds.Y} " +
            $"{platformDisplay} " +
            $"backbuffer={diagnostics.PreferredBackBufferWidth}x{diagnostics.PreferredBackBufferHeight}";

        if (message == lastRenderBoundsLog)
            return;

        lastRenderBoundsLog = message;
        Console.WriteLine(message);
    }

    private static bool IsRenderDiagnosticsEnabled()
    {
#if DEBUG
        var enabled = Environment.GetEnvironmentVariable(RenderDiagnosticsEnvironmentVariable);
        return string.Equals(enabled, "1", StringComparison.Ordinal) ||
               string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(enabled, "yes", StringComparison.OrdinalIgnoreCase);
#else
        return false;
#endif
    }

    private readonly record struct BackBufferSize(int Width, int Height);

    private void RouteTouch(UiFrameSnapshot currentFrame)
    {
        foreach (var touch in TouchPanel.GetState())
        {
            if (touch.State != TouchLocationState.Released)
                continue;

            var virtualPoint = currentFrame.Viewport.ToVirtual(touch.Position.X, touch.Position.Y);
            var element = currentFrame.HitTest(virtualPoint);
            if (element is null || element.Action == UiAction.None)
                continue;

            if (element.Action == UiAction.MainMenuCookie)
                core.TapMainMenuCookie();
            else
                core.TapMainMenu(UiActionRouter.ToMainMenuSlot(element.Action));

            break;
        }
    }
}
#endif
