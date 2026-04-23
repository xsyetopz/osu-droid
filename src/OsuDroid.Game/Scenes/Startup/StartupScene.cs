using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed class StartupScene
{
    public const double LoadingMilliseconds = DroidUiTimings.StartupLoadingFadeOutMilliseconds;
    public const double WelcomeMilliseconds = DroidUiTimings.StartupWelcomeMilliseconds;

    private const double LoadingFadeOutMilliseconds = DroidUiTimings.StartupLoadingFadeOutMilliseconds;
    private const double WelcomeDelayMilliseconds = DroidUiTimings.StartupWelcomeDelayMilliseconds;
    private const double WelcomeStretchMilliseconds = DroidUiTimings.StartupWelcomeStretchMilliseconds;

    private static readonly UiColor fallbackBackground = UiColor.Opaque(0, 0, 0);
    private static readonly UiColor white = UiColor.Opaque(255, 255, 255);

    private double elapsedMilliseconds;
    private bool welcomeSoundsRequested;

    public bool IsComplete => elapsedMilliseconds >= WelcomeStartMilliseconds + WelcomeMilliseconds;

    public bool ConsumeWelcomeSoundsRequest()
    {
        if (welcomeSoundsRequested)
            return false;

        if (elapsedMilliseconds < WelcomeStartMilliseconds)
            return false;

        welcomeSoundsRequested = true;
        return true;
    }

    public void Update(TimeSpan elapsed)
    {
        elapsedMilliseconds = Math.Min(elapsedMilliseconds + elapsed.TotalMilliseconds, WelcomeStartMilliseconds + WelcomeMilliseconds);
    }

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport)
    {
        var elements = new List<UiElementSnapshot>
        {
            new("startup-background", UiElementKind.Fill, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), fallbackBackground, 1f),
        };

        AddLoadingSpinner(elements, viewport);
        AddWelcome(elements, viewport);

        return new GameFrameSnapshot("Startup", "Welcome", "Welcome", Array.Empty<string>(), 0, false, new UiFrameSnapshot(viewport, elements, DroidAssets.StartupManifest));
    }

    private void AddLoadingSpinner(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var alpha = LoadingAlpha;
        if (alpha <= 0f)
            return;

        var asset = DroidAssets.StartupManifest.Get(DroidAssets.Loading);
        var size = asset.NativeSize.Width * 0.4f;
        var progress = (float)(elapsedMilliseconds / 2000d);
        elements.Add(new UiElementSnapshot(
            "startup-loading-spinner",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - size) / 2f, (viewport.VirtualHeight - size) / 2f, size, size),
            white,
            alpha,
            DroidAssets.Loading,
            RotationDegrees: progress * 360f));
    }

    private void AddWelcome(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (elapsedMilliseconds < WelcomeStartMilliseconds)
            return;

        var welcomeElapsed = elapsedMilliseconds - WelcomeStartMilliseconds;
        var fade = (float)Math.Clamp(welcomeElapsed / WelcomeMilliseconds, 0d, 1d);
        var yScale = welcomeElapsed < WelcomeStretchMilliseconds
            ? (float)Math.Clamp(welcomeElapsed / WelcomeStretchMilliseconds, 0d, 1d)
            : 1f + 0.1f * (float)Math.Clamp((welcomeElapsed - WelcomeStretchMilliseconds) / (WelcomeMilliseconds - WelcomeStretchMilliseconds), 0d, 1d);
        var asset = DroidAssets.StartupManifest.Get(DroidAssets.Welcome);
        var width = asset.NativeSize.Width * (welcomeElapsed < WelcomeStretchMilliseconds ? 1f : yScale);
        var height = asset.NativeSize.Height * yScale;
        elements.Add(new UiElementSnapshot(
            "startup-welcome",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height),
            white,
            fade,
            DroidAssets.Welcome));
    }

    private static double WelcomeStartMilliseconds => WelcomeDelayMilliseconds;

    private float LoadingAlpha => 1f - (float)Math.Clamp(elapsedMilliseconds / LoadingFadeOutMilliseconds, 0d, 1d);
}
