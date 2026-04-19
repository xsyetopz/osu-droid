namespace OsuDroid.Game.UI;

public sealed record VirtualViewport(
    int SurfaceWidth,
    int SurfaceHeight,
    float VirtualWidth,
    float VirtualHeight,
    float Scale,
    float OffsetX,
    float OffsetY)
{
    public const float LegacyWidth = 1280f;

    public static VirtualViewport LegacyLandscape { get; } = FromSurface(1280, 720);

    public static VirtualViewport FromSurface(int surfaceWidth, int surfaceHeight)
    {
        if (surfaceWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(surfaceWidth), surfaceWidth, null);

        if (surfaceHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(surfaceHeight), surfaceHeight, null);

        var scale = surfaceWidth / LegacyWidth;
        var virtualHeight = surfaceHeight / scale;
        return new VirtualViewport(surfaceWidth, surfaceHeight, LegacyWidth, virtualHeight, scale, 0f, 0f);
    }

    public UiPoint ToVirtual(float surfaceX, float surfaceY) =>
        new((surfaceX - OffsetX) / Scale, (surfaceY - OffsetY) / Scale);

    public UiRect ToSurface(UiRect virtualBounds) =>
        new(
            OffsetX + virtualBounds.X * Scale,
            OffsetY + virtualBounds.Y * Scale,
            virtualBounds.Width * Scale,
            virtualBounds.Height * Scale);
}
