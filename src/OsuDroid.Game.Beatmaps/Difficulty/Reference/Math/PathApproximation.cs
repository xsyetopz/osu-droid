namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

internal static class PathApproximation
{
    public const int CatmullDetail = 50;

    private const float BezierTolerance = 0.25f;
    private const float CircularArcTolerance = 0.1f;
    private const float BezierToleranceThreshold = BezierTolerance * BezierTolerance * 4f;

    public static List<ReferenceVector2> ApproximateBezier(IReadOnlyList<ReferenceVector2> controlPoints, CancellationToken cancellationToken = default)
    {
        var output = new List<ReferenceVector2>();
        int count = controlPoints.Count - 1;

        if (count < 0)
        {
            return output;
        }

        var toFlatten = new Stack<ReferenceVector2[]>();
        var freeBuffers = new Stack<ReferenceVector2[]>();

        toFlatten.Push(controlPoints.ToArray());
        var subdivisionBuffer1 = new ReferenceVector2[count + 1];
        var subdivisionBuffer2 = new ReferenceVector2[count * 2 + 1];

        while (toFlatten.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReferenceVector2[] parent = toFlatten.Pop();

            if (BezierIsFlatEnough(parent, cancellationToken))
            {
                BezierApproximate(parent, output, subdivisionBuffer1, subdivisionBuffer2, count + 1, cancellationToken);
                freeBuffers.Push(parent);
                continue;
            }

            ReferenceVector2[] rightChild = freeBuffers.Count > 0 ? freeBuffers.Pop() : new ReferenceVector2[count + 1];
            BezierSubdivide(parent, subdivisionBuffer2, rightChild, subdivisionBuffer1, count + 1, cancellationToken);

            for (int i = 0; i <= count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                parent[i] = subdivisionBuffer2[i];
            }

            toFlatten.Push(rightChild);
            toFlatten.Push(parent);
        }

        output.Add(controlPoints[count]);
        return output;
    }

    public static List<ReferenceVector2> ApproximateCatmull(IReadOnlyList<ReferenceVector2> controlPoints, CancellationToken cancellationToken = default)
    {
        int segmentCount = System.Math.Max(controlPoints.Count - 1, 0);
        var result = new List<ReferenceVector2>(segmentCount * CatmullDetail * 2);
        float inverseDetail = 1f / CatmullDetail;

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReferenceVector2 v1 = i > 0 ? controlPoints[i - 1] : controlPoints[i];
            ReferenceVector2 v2 = controlPoints[i];
            ReferenceVector2 v3 = i < controlPoints.Count - 1
                ? controlPoints[i + 1]
                : new ReferenceVector2(2 * v2.X - v1.X, 2 * v2.Y - v1.Y);
            ReferenceVector2 v4 = i < controlPoints.Count - 2
                ? controlPoints[i + 2]
                : new ReferenceVector2(2 * v3.X - v2.X, 2 * v3.Y - v2.Y);

            for (int c = 0; c < CatmullDetail; c++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.Add(CatmullFindPoint(v1, v2, v3, v4, c * inverseDetail));
                result.Add(CatmullFindPoint(v1, v2, v3, v4, (c + 1) * inverseDetail));
            }
        }

        return result;
    }

    public static List<ReferenceVector2> ApproximateCircularArc(IReadOnlyList<ReferenceVector2> controlPoints, CancellationToken cancellationToken = default)
    {
        if (controlPoints.Count != 3)
        {
            return ApproximateBezier(controlPoints, cancellationToken);
        }

        ReferenceVector2 a = controlPoints[0];
        ReferenceVector2 b = controlPoints[1];
        ReferenceVector2 c = controlPoints[2];

        if (Precision.AlmostEquals(0f, (b.Y - a.Y) * (c.X - a.X) - (b.X - a.X) * (c.Y - a.Y)))
        {
            return ApproximateBezier(controlPoints, cancellationToken);
        }

        float d = 2 * (a.X * (b.Y - a.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
        float aSquared = a.LengthSquared;
        float bSquared = b.LengthSquared;
        float cSquared = c.LengthSquared;

        float centerX = (aSquared * (b.Y - c.Y) + bSquared * (c.Y - a.Y) + cSquared * (a.Y - b.Y)) / d;
        float centerY = (aSquared * (c.X - b.X) + bSquared * (a.X - c.X) + cSquared * (b.X - a.X)) / d;

        float radius = MathF.Sqrt((a.X - centerX) * (a.X - centerX) + (a.Y - centerY) * (a.Y - centerY));
        double thetaStart = System.Math.Atan2(a.Y - centerY, a.X - centerX);
        double thetaEnd = System.Math.Atan2(c.Y - centerY, c.X - centerX);

        while (thetaEnd < thetaStart)
        {
            cancellationToken.ThrowIfCancellationRequested();
            thetaEnd += System.Math.Tau;
        }

        double direction = 1d;
        double thetaRange = thetaEnd - thetaStart;

        float orthogonalX = c.Y - a.Y;
        float orthogonalY = -(c.X - a.X);

        if (orthogonalX * (b.X - a.X) + orthogonalY * (b.Y - a.Y) < 0)
        {
            direction = -direction;
            thetaRange = System.Math.Tau - thetaRange;
        }

        int pointCount = 2 * radius <= CircularArcTolerance
            ? 2
            : System.Math.Max(2, (int)System.Math.Ceiling(thetaRange / (2 * System.Math.Acos(1 - CircularArcTolerance / radius))));

        var output = new List<ReferenceVector2>(pointCount);

        for (int i = 0; i < pointCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double fraction = i / (double)(pointCount - 1);
            double theta = thetaStart + direction * fraction * thetaRange;

            output.Add(new ReferenceVector2(
                centerX + (float)System.Math.Cos(theta) * radius,
                centerY + (float)System.Math.Sin(theta) * radius));
        }

        return output;
    }

    public static List<ReferenceVector2> ApproximateLinear(IReadOnlyList<ReferenceVector2> controlPoints) => controlPoints.ToList();

    private static bool BezierIsFlatEnough(ReferenceVector2[] controlPoints, CancellationToken cancellationToken)
    {
        for (int i = 1; i < controlPoints.Length - 1; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReferenceVector2 previous = controlPoints[i - 1];
            ReferenceVector2 current = controlPoints[i];
            ReferenceVector2 next = controlPoints[i + 1];
            float dx = previous.X - current.X * 2 + next.X;
            float dy = previous.Y - current.Y * 2 + next.Y;
            float lengthSquared = dx * dx + dy * dy;
            if (lengthSquared > BezierToleranceThreshold)
            {
                return false;
            }
        }

        return true;
    }

    private static void BezierApproximate(
        ReferenceVector2[] controlPoints,
        List<ReferenceVector2> output,
        ReferenceVector2[] subdivisionBuffer1,
        ReferenceVector2[] subdivisionBuffer2,
        int count,
        CancellationToken cancellationToken)
    {
        BezierSubdivide(controlPoints, subdivisionBuffer2, subdivisionBuffer1, subdivisionBuffer1, count, cancellationToken);

        if (count > 1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Array.Copy(subdivisionBuffer1, 1, subdivisionBuffer2, count, count - 1);
        }

        output.Add(controlPoints[0]);

        for (int i = 1; i < count - 1; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int index = 2 * i;
            ReferenceVector2 previous = subdivisionBuffer2[index - 1];
            ReferenceVector2 current = subdivisionBuffer2[index];
            ReferenceVector2 next = subdivisionBuffer2[index + 1];

            output.Add(new ReferenceVector2(
                0.25f * (previous.X + current.X * 2 + next.X),
                0.25f * (previous.Y + current.Y * 2 + next.Y)));
        }
    }

    private static void BezierSubdivide(
        ReferenceVector2[] controlPoints,
        ReferenceVector2[] left,
        ReferenceVector2[] right,
        ReferenceVector2[] subdivisionBuffer,
        int count,
        CancellationToken cancellationToken)
    {
        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            subdivisionBuffer[i] = controlPoints[i];
        }

        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            left[i] = subdivisionBuffer[0];
            right[count - i - 1] = subdivisionBuffer[count - i - 1];

            for (int j = 0; j < count - i - 1; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReferenceVector2 leftPoint = subdivisionBuffer[j];
                ReferenceVector2 rightPoint = subdivisionBuffer[j + 1];
                subdivisionBuffer[j] = new ReferenceVector2(
                    (leftPoint.X + rightPoint.X) * 0.5f,
                    (leftPoint.Y + rightPoint.Y) * 0.5f);
            }
        }
    }

    private static ReferenceVector2 CatmullFindPoint(
        ReferenceVector2 vector1,
        ReferenceVector2 vector2,
        ReferenceVector2 vector3,
        ReferenceVector2 vector4,
        float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return new ReferenceVector2(
            0.5f * (2 * vector2.X + (-vector1.X + vector3.X) * t + (2 * vector1.X - 5 * vector2.X + 4 * vector3.X - vector4.X) * t2 + (-vector1.X + 3 * vector2.X - 3 * vector3.X + vector4.X) * t3),
            0.5f * (2 * vector2.Y + (-vector1.Y + vector3.Y) * t + (2 * vector1.Y - 5 * vector2.Y + 4 * vector3.Y - vector4.Y) * t2 + (-vector1.Y + 3 * vector2.Y - 3 * vector3.Y + vector4.Y) * t3));
    }
}
