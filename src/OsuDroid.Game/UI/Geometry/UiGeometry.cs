namespace OsuDroid.Game.UI.Geometry;

public readonly record struct UiPoint(float X, float Y);

public readonly record struct UiSize(float Width, float Height);

public readonly record struct UiRect(float X, float Y, float Width, float Height)
{
    public float Right => X + Width;

    public float Bottom => Y + Height;

    public bool Contains(UiPoint point) =>
        point.X >= X && point.X <= Right && point.Y >= Y && point.Y <= Bottom;
}

public readonly record struct UiColor(byte Red, byte Green, byte Blue, byte Alpha)
{
    public static UiColor Opaque(byte red, byte green, byte blue) => new(red, green, blue, 255);
}
