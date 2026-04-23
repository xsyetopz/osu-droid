using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public enum BootstrapLoadingKind
{
    Generic,
    BeatmapProcessing,
}

public readonly record struct BootstrapLoadingProgress(int Percent, string StatusText, BootstrapLoadingKind Kind = BootstrapLoadingKind.Generic);

public static class BootstrapLoadingScene
{
    // Legacy source: third_party/osu-droid-legacy/.../menu/SplashScene.java.
    private const float LoadingSpinnerScale = 0.4f;
    private const float ProgressTextScale = 0.5f;
    private const float StatusTextScale = 0.6f;
    private const float StartupFontSize = 28f;
    private const float StatusBottomPadding = 20f;

    private static readonly UiColor Black = UiColor.Opaque(0, 0, 0);
    private static readonly UiColor White = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor LoadingText = UiColor.Opaque(220, 220, 230);

    public static GameFrameSnapshot CreateSnapshot(VirtualViewport viewport, BootstrapLoadingProgress progress, TimeSpan elapsed)
    {
        var elements = new List<UiElementSnapshot>
        {
            new("bootstrap-background", UiElementKind.Fill, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), Black, 1f),
        };

        if (progress.Kind == BootstrapLoadingKind.BeatmapProcessing)
            AddLoadingTitle(elements, viewport);
        AddSpinner(elements, viewport, elapsed);
        AddProgressText(elements, viewport, progress);
        AddStatusText(elements, viewport, progress);

        return new GameFrameSnapshot("Bootstrap", "Loading", progress.StatusText, Array.Empty<string>(), 0, false, new UiFrameSnapshot(viewport, elements, DroidAssets.StartupManifest));
    }

    private static void AddLoadingTitle(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var asset = DroidAssets.StartupManifest.Get(DroidAssets.LoadingTitle);
        var scale = viewport.VirtualWidth / asset.NativeSize.Width;
        var height = asset.NativeSize.Height * scale;
        elements.Add(new UiElementSnapshot(
            "bootstrap-loading-title",
            UiElementKind.Sprite,
            new UiRect(0f, 0f, viewport.VirtualWidth, height),
            White,
            1f,
            DroidAssets.LoadingTitle));
    }

    private static void AddSpinner(List<UiElementSnapshot> elements, VirtualViewport viewport, TimeSpan elapsed)
    {
        var asset = DroidAssets.StartupManifest.Get(DroidAssets.Loading);
        var size = asset.NativeSize.Width * LoadingSpinnerScale;
        var rotation = (float)(elapsed.TotalMilliseconds % 900d / 900d * 360d);
        elements.Add(new UiElementSnapshot(
            "bootstrap-loading-spinner",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - size) / 2f, (viewport.VirtualHeight - size) / 2f, size, size),
            White,
            1f,
            DroidAssets.Loading,
            RotationDegrees: rotation));
    }

    private static void AddProgressText(List<UiElementSnapshot> elements, VirtualViewport viewport, BootstrapLoadingProgress progress)
    {
        var loadingAsset = DroidAssets.StartupManifest.Get(DroidAssets.Loading);
        var progressTextSize = StartupFontSize * ProgressTextScale;
        var progressY = (viewport.VirtualHeight + loadingAsset.NativeSize.Height) / 2f - loadingAsset.NativeSize.Height / 4f;
        elements.Add(new UiElementSnapshot(
            "bootstrap-loading-progress",
            UiElementKind.Text,
            new UiRect(0f, progressY, viewport.VirtualWidth, progressTextSize + 6f),
            LoadingText,
            1f,
            Text: $"{Math.Clamp(progress.Percent, 0, 100)} %",
            TextStyle: new UiTextStyle(progressTextSize, Alignment: UiTextAlignment.Center)));
    }

    private static void AddStatusText(List<UiElementSnapshot> elements, VirtualViewport viewport, BootstrapLoadingProgress progress)
    {
        var statusTextSize = StartupFontSize * StatusTextScale;
        elements.Add(new UiElementSnapshot(
            "bootstrap-loading-text",
            UiElementKind.Text,
            new UiRect(0f, viewport.VirtualHeight - statusTextSize - StatusBottomPadding, viewport.VirtualWidth, statusTextSize + 6f),
            LoadingText,
            1f,
            Text: progress.StatusText,
            TextStyle: new UiTextStyle(statusTextSize, Alignment: UiTextAlignment.Center)));
    }
}
