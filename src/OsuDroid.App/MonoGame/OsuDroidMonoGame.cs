#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Maui.ApplicationModel;
using OsuDroid.App.MonoGame.Bootstrap;
using OsuDroid.App.MonoGame.Input;
using OsuDroid.App.MonoGame.Rendering;
using OsuDroid.Game;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;
using System.Diagnostics;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaDisplayOrientation = Microsoft.Xna.Framework.DisplayOrientation;
#if IOS
using UIKit;
#endif

namespace OsuDroid.App.MonoGame;

public sealed class OsuDroidMonoGame : Microsoft.Xna.Framework.Game
{
    private const string RenderDiagnosticsEnvironmentVariable = "OSUDROID_RENDER_DIAGNOSTICS";
    private static readonly TimeSpan WarmupBudget = TimeSpan.FromMilliseconds(4);
    private static readonly TimeSpan DrawLogThreshold = TimeSpan.FromMilliseconds(16);

    private readonly GraphicsDeviceManager graphics;
    private readonly RenderWarmupQueue warmupQueue = new();
    private readonly GameBootstrapper? bootstrapper;
    private OsuDroidGameCore? core;
    private MonoGameTouchRouter? touchRouter;
    private MonoGameUiRenderer? renderer;
    private SpriteBatch? spriteBatch;
    private UiFrameSnapshot? frame;
    private VirtualViewport? warmupViewport;
    private TimeSpan bootstrapElapsed;
    private string? lastRenderBoundsLog;
    private bool hasRenderedFrame;
    private readonly bool showRenderDiagnostics = IsRenderDiagnosticsEnabled();
    private string? platformDisplayLog;

    public OsuDroidMonoGame(OsuDroidGameCore core)
    {
        this.core = core;
        Content.RootDirectory = "Content";
        touchRouter = new MonoGameTouchRouter(core);
        graphics = new GraphicsDeviceManager(this)
        {
            SupportedOrientations =
                XnaDisplayOrientation.LandscapeLeft | XnaDisplayOrientation.LandscapeRight,
            IsFullScreen = true,
            PreferMultiSampling = true,
        };
        IsMouseVisible = false;
    }

    public OsuDroidMonoGame(GameBootstrapper bootstrapper)
    {
        this.bootstrapper = bootstrapper;
        Content.RootDirectory = "Content";
        graphics = new GraphicsDeviceManager(this)
        {
            SupportedOrientations =
                XnaDisplayOrientation.LandscapeLeft | XnaDisplayOrientation.LandscapeRight,
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
        renderer = new MonoGameUiRenderer(GraphicsDevice, Content);
        var preloadStart = PerfDiagnostics.Start();
        renderer.PreloadStatic(DroidAssets.StartupManifest);
        PerfDiagnostics.Log("bootstrap.contentPreload", preloadStart);
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
        bootstrapElapsed += gameTime.ElapsedGameTime;
        frame ??= CreateCurrentFrame();
        if (core is null)
        {
            bootstrapper?.Start();
            if (bootstrapper?.TryConsumeCore(out var loadedCore) == true)
            {
                core = loadedCore;
                touchRouter = new MonoGameTouchRouter(core);
                frame = CreateCurrentFrame();
            }
            else
            {
                frame = CreateBootstrapFrame();
                base.Update(gameTime);
                return;
            }
        }

        var inputStart = PerfDiagnostics.Start();
        touchRouter?.Route(frame);
        PerfDiagnostics.Log("monoGame.routeInput", inputStart);
        core.Update(gameTime.ElapsedGameTime);
        var frameStart = PerfDiagnostics.Start();
        frame = CreateCurrentFrame();
        PerfDiagnostics.Log(
            "monoGame.createCurrentFrame",
            frameStart,
            $"elements={frame.Elements.Count}"
        );
        OpenPendingExternalUrl();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        frame ??= CreateCurrentFrame();
        GraphicsDevice.Clear(XnaColor.Black);

        if (spriteBatch is not null && renderer is not null)
        {
            var drawMetrics = showRenderDiagnostics ? new RenderCacheMetrics() : null;
            var drawStart =
                showRenderDiagnostics || PerfDiagnostics.Enabled ? Stopwatch.GetTimestamp() : 0L;
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.LinearClamp
            );
            renderer.Draw(spriteBatch, frame, drawMetrics);

            if (showRenderDiagnostics)
                renderer.DrawDiagnostics(spriteBatch, CreateRenderDiagnostics());

            spriteBatch.End();
            PerfDiagnostics.Log("monoGame.draw", drawStart, $"elements={frame.Elements.Count}");
            LogDrawMetrics(drawStart, drawMetrics);
            RunWarmup(frame);
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
            GraphicsDevice.Viewport = new Viewport(
                0,
                0,
                backBufferSize.Width,
                backBufferSize.Height
            );
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
            $"iosNative={nativeBounds.Width:0}x{nativeBounds.Height:0} "
            + $"iosLogical={logicalBounds.Width:0.###}x{logicalBounds.Height:0.###} "
            + $"iosNativeScale={screen.NativeScale:0.###}";

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
            GraphicsDevice.Viewport.Height
        );
        if (core is null)
            return BootstrapLoadingScene
                .CreateSnapshot(
                    viewport,
                    bootstrapper?.Progress ?? new BootstrapLoadingProgress(0, "Loading skin..."),
                    bootstrapElapsed
                )
                .UiFrame;

        return core.CreateFrame(viewport).UiFrame;
    }

    private UiFrameSnapshot CreateBootstrapFrame()
    {
        var viewport = VirtualViewport.FromSurface(
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height
        );
        return BootstrapLoadingScene
            .CreateSnapshot(
                viewport,
                bootstrapper?.Progress ?? new BootstrapLoadingProgress(0, "Loading skin..."),
                bootstrapElapsed
            )
            .UiFrame;
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
            graphics.PreferredBackBufferHeight
        );
    }

    private void LogRenderBoundsIfChanged(string phase)
    {
        if (!showRenderDiagnostics || GraphicsDevice is null)
            return;

        var diagnostics = CreateRenderDiagnostics();
        var platformDisplay =
            platformDisplayLog ?? $"display={diagnostics.DisplayWidth}x{diagnostics.DisplayHeight}";
        var message =
            $"osu!droid render-bounds phase={phase} "
            + $"viewport={diagnostics.ViewportBounds.Width}x{diagnostics.ViewportBounds.Height}+{diagnostics.ViewportBounds.X},{diagnostics.ViewportBounds.Y} "
            + $"client={diagnostics.ClientBounds.Width}x{diagnostics.ClientBounds.Height}+{diagnostics.ClientBounds.X},{diagnostics.ClientBounds.Y} "
            + $"{platformDisplay} "
            + $"backbuffer={diagnostics.PreferredBackBufferWidth}x{diagnostics.PreferredBackBufferHeight}";

        if (message == lastRenderBoundsLog)
            return;

        lastRenderBoundsLog = message;
        Console.WriteLine(message);
    }

    private void RunWarmup(UiFrameSnapshot activeFrame)
    {
        if (renderer is null || core is null || touchRouter?.IsPointerActive == true)
            return;

        if (!hasRenderedFrame)
        {
            hasRenderedFrame = true;
            return;
        }

        var metrics = new RenderCacheMetrics();
        var start = Stopwatch.GetTimestamp();
        var deadline = DateTime.UtcNow + WarmupBudget;
        _ = renderer.WarmUp(activeFrame, 0, deadline, metrics);

        if (showRenderDiagnostics && (metrics.WarmupElements > 0 || metrics.HasCacheMisses))
        {
            var elapsed = Stopwatch.GetElapsedTime(start);
            Console.WriteLine(
                $"osu!droid render-cache phase=active-warmup elapsedMs={elapsed.TotalMilliseconds:0.###} {metrics}"
            );
        }

        var viewport = activeFrame.Viewport;
        if (warmupViewport != viewport)
        {
            warmupViewport = viewport;
            warmupQueue.Reset(core.CreateWarmupFrames(viewport));
        }

        if (warmupQueue.IsComplete)
            return;

        metrics = new RenderCacheMetrics();
        start = Stopwatch.GetTimestamp();
        warmupQueue.Run(renderer, WarmupBudget, metrics);

        if (showRenderDiagnostics && (metrics.WarmupElements > 0 || metrics.HasCacheMisses))
        {
            var elapsed = Stopwatch.GetElapsedTime(start);
            Console.WriteLine(
                $"osu!droid render-cache phase=warmup elapsedMs={elapsed.TotalMilliseconds:0.###} {metrics}"
            );
        }
    }

    private void LogDrawMetrics(long drawStart, RenderCacheMetrics? metrics)
    {
        if (!showRenderDiagnostics || metrics is null)
            return;

        var elapsed = Stopwatch.GetElapsedTime(drawStart);
        if (elapsed < DrawLogThreshold && !metrics.HasCacheMisses)
            return;

        Console.WriteLine(
            $"osu!droid render-cache phase=draw elapsedMs={elapsed.TotalMilliseconds:0.###} {metrics}"
        );
    }

    private void OpenPendingExternalUrl()
    {
        if (core is null)
            return;

        var pendingUrl = core.ConsumePendingExternalUrl();
        if (pendingUrl is null)
            return;

        _ = Launcher.OpenAsync(pendingUrl);
    }

    private static bool IsRenderDiagnosticsEnabled()
    {
#if DEBUG
        var enabled = Environment.GetEnvironmentVariable(RenderDiagnosticsEnvironmentVariable);
        return string.Equals(enabled, "1", StringComparison.Ordinal)
            || string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(enabled, "yes", StringComparison.OrdinalIgnoreCase);
#else
        return false;
#endif
    }

    private readonly record struct BackBufferSize(int Width, int Height);
}
#endif
