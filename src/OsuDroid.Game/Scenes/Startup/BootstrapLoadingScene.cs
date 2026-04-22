using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public readonly record struct BootstrapLoadingProgress(int Percent, string StatusText);

public static class BootstrapLoadingScene
{
    private static readonly UiColor Black = UiColor.Opaque(0, 0, 0);
    private static readonly UiColor White = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor LoadingText = UiColor.Opaque(220, 220, 230);

    public static GameFrameSnapshot CreateSnapshot(VirtualViewport viewport, BootstrapLoadingProgress progress, TimeSpan elapsed)
    {
        var elements = new List<UiElementSnapshot>
        {
            new("bootstrap-background", UiElementKind.Fill, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), Black, 1f),
        };

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
        var size = 112f;
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
        elements.Add(new UiElementSnapshot(
            "bootstrap-loading-progress",
            UiElementKind.Text,
            new UiRect(0f, viewport.VirtualHeight * 0.57f, viewport.VirtualWidth, 34f),
            LoadingText,
            1f,
            Text: $"{Math.Clamp(progress.Percent, 0, 100)} %",
            TextStyle: new UiTextStyle(22f, Alignment: UiTextAlignment.Center)));
    }

    private static void AddStatusText(List<UiElementSnapshot> elements, VirtualViewport viewport, BootstrapLoadingProgress progress)
    {
        elements.Add(new UiElementSnapshot(
            "bootstrap-loading-text",
            UiElementKind.Text,
            new UiRect(0f, viewport.VirtualHeight - 76f, viewport.VirtualWidth, 34f),
            LoadingText,
            1f,
            Text: progress.StatusText,
            TextStyle: new UiTextStyle(22f, Alignment: UiTextAlignment.Center)));
    }
}
