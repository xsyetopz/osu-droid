namespace OsuDroid.Game.UI.Style;

public static partial class DroidUiMetrics
{
    public const float ReferencePixelWidth = 2340f;
    public const float ReferenceDensity = 3f;
    public const float DpScale = VirtualViewport.LegacyWidth / (ReferencePixelWidth / ReferenceDensity);
    public const float SpScale = DpScale;
}
