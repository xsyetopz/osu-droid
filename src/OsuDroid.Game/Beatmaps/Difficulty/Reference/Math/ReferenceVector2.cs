namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

internal readonly record struct ReferenceVector2(float X, float Y)
{
    public float Length => MathF.Sqrt(X * X + Y * Y);

    public float LengthSquared => X * X + Y * Y;

    public float Dot(ReferenceVector2 vector) => X * vector.X + Y * vector.Y;

    public float DistanceTo(ReferenceVector2 vector)
    {
        float dx = vector.X - X;
        float dy = vector.Y - Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public float DistanceSquaredTo(ReferenceVector2 vector)
    {
        float dx = vector.X - X;
        float dy = vector.Y - Y;
        return dx * dx + dy * dy;
    }

    public float DistanceSquaredTo(float x, float y)
    {
        float dx = x - X;
        float dy = y - Y;
        return dx * dx + dy * dy;
    }

    public static ReferenceVector2 operator *(ReferenceVector2 left, ReferenceVector2 right) => new(left.X * right.X, left.Y * right.Y);

    public static ReferenceVector2 operator *(ReferenceVector2 vector, int scaleFactor) => vector * (float)scaleFactor;

    public static ReferenceVector2 operator *(ReferenceVector2 vector, float scaleFactor) => new(vector.X * scaleFactor, vector.Y * scaleFactor);

    public static ReferenceVector2 operator *(ReferenceVector2 vector, double scaleFactor) => vector * (float)scaleFactor;

    public static ReferenceVector2 operator /(ReferenceVector2 vector, int divideFactor) => vector / (float)divideFactor;

    public static ReferenceVector2 operator /(ReferenceVector2 vector, float divideFactor)
    {
        return divideFactor == 0f
            ? throw new DivideByZeroException("Division by 0")
            : new ReferenceVector2(vector.X / divideFactor, vector.Y / divideFactor);
    }

    public static ReferenceVector2 operator /(ReferenceVector2 vector, double divideFactor) => vector / (float)divideFactor;

    public static ReferenceVector2 operator +(ReferenceVector2 vector, float value) => new(vector.X + value, vector.Y + value);

    public static ReferenceVector2 operator +(ReferenceVector2 left, ReferenceVector2 right) => new(left.X + right.X, left.Y + right.Y);

    public static ReferenceVector2 operator -(ReferenceVector2 vector, float value) => new(vector.X - value, vector.Y - value);

    public static ReferenceVector2 operator -(ReferenceVector2 left, ReferenceVector2 right) => new(left.X - right.X, left.Y - right.Y);

    public static ReferenceVector2 operator -(ReferenceVector2 vector) => new(-vector.X, -vector.Y);

    public static float DistanceSquared(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return dx * dx + dy * dy;
    }
}
