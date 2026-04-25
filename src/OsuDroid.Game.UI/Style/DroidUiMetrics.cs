using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.UI.Style;

public static partial class DroidUiMetrics
{
    public const float ReferencePixelWidth = 2340f;
    public const float ReferenceDensity = 3f;
    public const float DpScale = VirtualViewport.AndroidReferenceWidth / (ReferencePixelWidth / ReferenceDensity);
    public const float SpScale = DpScale;
}
