using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.Startup;

public sealed class StartupScene
{
    public const double LoadingMilliseconds = DroidUiTimings.StartupLoadingFadeOutMilliseconds;
    public const double WelcomeMilliseconds = DroidUiTimings.StartupWelcomeMilliseconds;

    private const double LoadingFadeOutMilliseconds = DroidUiTimings.StartupLoadingFadeOutMilliseconds;
    private const double WelcomeDelayMilliseconds = DroidUiTimings.StartupWelcomeDelayMilliseconds;
    private const double WelcomeStretchMilliseconds = DroidUiTimings.StartupWelcomeStretchMilliseconds;

    private static readonly UiColor s_fallbackBackground = UiColor.Opaque(0, 0, 0);
    private static readonly UiColor s_white = UiColor.Opaque(255, 255, 255);

    private double _elapsedMilliseconds;
    private bool _welcomeSoundsRequested;

    public bool IsComplete => _elapsedMilliseconds >= WelcomeStartMilliseconds + WelcomeMilliseconds;

    public bool ConsumeWelcomeSoundsRequest()
    {
        if (_welcomeSoundsRequested)
        {
            return false;
        }

        if (_elapsedMilliseconds < WelcomeStartMilliseconds)
        {
            return false;
        }

        _welcomeSoundsRequested = true;
        return true;
    }

    public void Update(TimeSpan elapsed) => _elapsedMilliseconds = Math.Min(_elapsedMilliseconds + elapsed.TotalMilliseconds, WelcomeStartMilliseconds + WelcomeMilliseconds);

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport)
    {
        var elements = new List<UiElementSnapshot>
        {
            new("startup-background", UiElementKind.Fill, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_fallbackBackground, 1f),
        };

        AddLoadingSpinner(elements, viewport);
        AddWelcome(elements, viewport);

        return new GameFrameSnapshot("Startup", "Welcome", "Welcome", Array.Empty<string>(), 0, false, new UiFrameSnapshot(viewport, elements, DroidAssets.StartupManifest));
    }

    private void AddLoadingSpinner(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float alpha = LoadingAlpha;
        if (alpha <= 0f)
        {
            return;
        }

        UiAssetEntry asset = DroidAssets.StartupManifest.Get(DroidAssets.Loading);
        float size = asset.NativeSize.Width * 0.4f;
        float progress = (float)(_elapsedMilliseconds / 2000d);
        elements.Add(new UiElementSnapshot(
            "startup-loading-spinner",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - size) / 2f, (viewport.VirtualHeight - size) / 2f, size, size),
            s_white,
            alpha,
            DroidAssets.Loading,
            RotationDegrees: progress * 360f));
    }

    private void AddWelcome(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (_elapsedMilliseconds < WelcomeStartMilliseconds)
        {
            return;
        }

        double welcomeElapsed = _elapsedMilliseconds - WelcomeStartMilliseconds;
        float fade = (float)Math.Clamp(welcomeElapsed / WelcomeMilliseconds, 0d, 1d);
        float yScale = welcomeElapsed < WelcomeStretchMilliseconds
            ? (float)Math.Clamp(welcomeElapsed / WelcomeStretchMilliseconds, 0d, 1d)
            : 1f + 0.1f * (float)Math.Clamp((welcomeElapsed - WelcomeStretchMilliseconds) / (WelcomeMilliseconds - WelcomeStretchMilliseconds), 0d, 1d);
        UiAssetEntry asset = DroidAssets.StartupManifest.Get(DroidAssets.Welcome);
        float width = asset.NativeSize.Width * (welcomeElapsed < WelcomeStretchMilliseconds ? 1f : yScale);
        float height = asset.NativeSize.Height * yScale;
        elements.Add(new UiElementSnapshot(
            "startup-welcome",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height),
            s_white,
            fade,
            DroidAssets.Welcome));
    }

    private static double WelcomeStartMilliseconds => WelcomeDelayMilliseconds;

    private float LoadingAlpha => 1f - (float)Math.Clamp(_elapsedMilliseconds / LoadingFadeOutMilliseconds, 0d, 1d);
}
