using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.Startup;

public enum BootstrapLoadingKind
{
    Generic,
    BeatmapProcessing,
}

public readonly record struct BootstrapLoadingProgress(
    int Percent,
    string StatusText,
    BootstrapLoadingKind Kind = BootstrapLoadingKind.Generic
);

public static class BootstrapLoadingScene
{
    // Android source: menu/SplashScene.java.
    private const float LoadingSpinnerScale = 0.4f;
    private const float ProgressTextScale = 0.5f;
    private const float StatusTextScale = 0.6f;
    private const float StartupFontSize = 28f;
    private const float StatusBottomPadding = 20f;

    private static readonly UiColor s_black = UiColor.Opaque(0, 0, 0);
    private static readonly UiColor s_white = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor s_loadingText = UiColor.Opaque(220, 220, 230);

    public static GameFrameSnapshot CreateSnapshot(
        VirtualViewport viewport,
        BootstrapLoadingProgress progress,
        TimeSpan elapsed
    )
    {
        var elements = new List<UiElementSnapshot>
        {
            new(
                "bootstrap-background",
                UiElementKind.Fill,
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_black,
                1f
            ),
        };

        if (progress.Kind == BootstrapLoadingKind.BeatmapProcessing)
        {
            AddLoadingTitle(elements, viewport);
        }

        AddSpinner(elements, viewport, elapsed);
        AddProgressText(elements, viewport, progress);
        AddStatusText(elements, viewport, progress);

        return new GameFrameSnapshot(
            "Bootstrap",
            "Loading",
            progress.StatusText,
            Array.Empty<string>(),
            0,
            false,
            new UiFrameSnapshot(viewport, elements, DroidAssets.StartupManifest)
        );
    }

    private static void AddLoadingTitle(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiAssetEntry asset = DroidAssets.StartupManifest.Get(DroidAssets.LoadingTitle);
        float scale = viewport.VirtualWidth / asset.NativeSize.Width;
        float height = asset.NativeSize.Height * scale;
        elements.Add(
            new UiElementSnapshot(
                "bootstrap-loading-title",
                UiElementKind.Sprite,
                new UiRect(0f, 0f, viewport.VirtualWidth, height),
                s_white,
                1f,
                DroidAssets.LoadingTitle
            )
        );
    }

    private static void AddSpinner(
        List<UiElementSnapshot> elements,
        VirtualViewport viewport,
        TimeSpan elapsed
    )
    {
        UiAssetEntry asset = DroidAssets.StartupManifest.Get(DroidAssets.Loading);
        float size = asset.NativeSize.Width * LoadingSpinnerScale;
        float rotation = (float)(elapsed.TotalMilliseconds % 900d / 900d * 360d);
        elements.Add(
            new UiElementSnapshot(
                "bootstrap-loading-spinner",
                UiElementKind.Sprite,
                new UiRect(
                    (viewport.VirtualWidth - size) / 2f,
                    (viewport.VirtualHeight - size) / 2f,
                    size,
                    size
                ),
                s_white,
                1f,
                DroidAssets.Loading,
                RotationDegrees: rotation
            )
        );
    }

    private static void AddProgressText(
        List<UiElementSnapshot> elements,
        VirtualViewport viewport,
        BootstrapLoadingProgress progress
    )
    {
        UiAssetEntry loadingAsset = DroidAssets.StartupManifest.Get(DroidAssets.Loading);
        float progressTextSize = StartupFontSize * ProgressTextScale;
        float progressY =
            (viewport.VirtualHeight + loadingAsset.NativeSize.Height) / 2f
            - loadingAsset.NativeSize.Height / 4f;
        elements.Add(
            new UiElementSnapshot(
                "bootstrap-loading-progress",
                UiElementKind.Text,
                new UiRect(0f, progressY, viewport.VirtualWidth, progressTextSize + 6f),
                s_loadingText,
                1f,
                Text: $"{Math.Clamp(progress.Percent, 0, 100)} %",
                TextStyle: new UiTextStyle(progressTextSize, Alignment: UiTextAlignment.Center)
            )
        );
    }

    private static void AddStatusText(
        List<UiElementSnapshot> elements,
        VirtualViewport viewport,
        BootstrapLoadingProgress progress
    )
    {
        float statusTextSize = StartupFontSize * StatusTextScale;
        elements.Add(
            new UiElementSnapshot(
                "bootstrap-loading-text",
                UiElementKind.Text,
                new UiRect(
                    0f,
                    viewport.VirtualHeight - statusTextSize - StatusBottomPadding,
                    viewport.VirtualWidth,
                    statusTextSize + 6f
                ),
                s_loadingText,
                1f,
                Text: progress.StatusText,
                TextStyle: new UiTextStyle(statusTextSize, Alignment: UiTextAlignment.Center)
            )
        );
    }
}
