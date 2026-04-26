#pragma warning disable CA1859

using OsuDroid.Game.Beatmaps.Difficulty.Reference.Math;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;

internal sealed class SliderPath
{
    public SliderPath(
        SliderPathType pathType,
        IReadOnlyList<ReferenceVector2> controlPoints,
        double expectedDistance,
        CancellationToken cancellationToken = default
    )
    {
        PathType = pathType;
        ControlPoints = controlPoints;
        ExpectedDistance = expectedDistance;
        CalculatePath(cancellationToken);
        CalculateCumulativeLength(cancellationToken);
    }

    public SliderPathType PathType { get; }

    public IReadOnlyList<ReferenceVector2> ControlPoints { get; }

    public double ExpectedDistance { get; }

    public List<ReferenceVector2> CalculatedPath { get; } = [];

    public List<double> CumulativeLength { get; } = [];

    public ReferenceVector2 PositionAt(double progress)
    {
        double distance = ProgressToDistance(progress);
        return InterpolateVertices(IndexOfDistance(distance), distance);
    }

    public List<ReferenceVector2> GetPathToProgress(
        double startProgress,
        double endProgress,
        CancellationToken cancellationToken = default
    )
    {
        double startDistance = ProgressToDistance(startProgress);
        double endDistance = ProgressToDistance(endProgress);
        int startEstimate = IndexOfDistance(startDistance);
        int endEstimate = IndexOfDistance(endDistance);
        int estimatedSize =
            endEstimate >= startEstimate
                ? endEstimate - startEstimate + 3
                : startEstimate - endEstimate + 3;

        var path = new List<ReferenceVector2>(estimatedSize);
        int index = 0;

        while (index < CalculatedPath.Count && CumulativeLength[index] < startDistance)
        {
            cancellationToken.ThrowIfCancellationRequested();
            index++;
        }

        path.Add(InterpolateVertices(index, startDistance));

        while (index < CalculatedPath.Count && CumulativeLength[index] <= endDistance)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path.Add(CalculatedPath[index++]);
        }

        path.Add(InterpolateVertices(index, endDistance));
        return path;
    }

    private void CalculatePath(CancellationToken cancellationToken)
    {
        CalculatedPath.Clear();

        if (ControlPoints.Count == 0)
        {
            return;
        }

        CalculatedPath.Add(ControlPoints[0]);
        int spanStart = 0;

        for (int index = 0; index < ControlPoints.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (
                index == ControlPoints.Count - 1
                || ControlPoints[index] == ControlPoints[index + 1]
            )
            {
                int spanEnd = index + 1;
                IReadOnlyList<ReferenceVector2> span = ControlPoints
                    .Skip(spanStart)
                    .Take(spanEnd - spanStart)
                    .ToArray();
                foreach (ReferenceVector2 point in CalculateSubPath(span, cancellationToken))
                {
                    if (CalculatedPath.Count == 0 || CalculatedPath[^1] != point)
                    {
                        CalculatedPath.Add(point);
                    }
                }

                spanStart = spanEnd;
            }
        }
    }

    private void CalculateCumulativeLength(CancellationToken cancellationToken)
    {
        CumulativeLength.Clear();
        CumulativeLength.Add(0);
        double calculatedLength = 0;

        for (int index = 0; index < CalculatedPath.Count - 1; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            calculatedLength += CalculatedPath[index + 1].DistanceTo(CalculatedPath[index]);
            CumulativeLength.Add(calculatedLength);
        }

        if (calculatedLength == ExpectedDistance)
        {
            return;
        }

        if (
            ControlPoints.Count >= 2
            && ControlPoints[^1] == ControlPoints[^2]
            && ExpectedDistance > calculatedLength
        )
        {
            return;
        }

        CumulativeLength.RemoveAt(CumulativeLength.Count - 1);
        int pathEndIndex = CalculatedPath.Count - 1;

        if (calculatedLength > ExpectedDistance)
        {
            while (CumulativeLength.Count > 0 && CumulativeLength[^1] >= ExpectedDistance)
            {
                cancellationToken.ThrowIfCancellationRequested();
                CumulativeLength.RemoveAt(CumulativeLength.Count - 1);
                CalculatedPath.RemoveAt(pathEndIndex--);
            }
        }

        if (pathEndIndex <= 0)
        {
            CumulativeLength.Add(0);
            return;
        }

        ReferenceVector2 previousPoint = CalculatedPath[pathEndIndex - 1];
        ReferenceVector2 endPoint = CalculatedPath[pathEndIndex];
        float lengthSquared = endPoint.DistanceSquaredTo(previousPoint);
        float inverseLength = lengthSquared == 0f ? 0f : 1f / MathF.Sqrt(lengthSquared);
        float extension = (float)(ExpectedDistance - CumulativeLength[^1]);

        CalculatedPath[pathEndIndex] = new ReferenceVector2(
            previousPoint.X + (endPoint.X - previousPoint.X) * inverseLength * extension,
            previousPoint.Y + (endPoint.Y - previousPoint.Y) * inverseLength * extension
        );

        CumulativeLength.Add(ExpectedDistance);
    }

    private IReadOnlyList<ReferenceVector2> CalculateSubPath(
        IReadOnlyList<ReferenceVector2> controlPoints,
        CancellationToken cancellationToken
    ) =>
        PathType switch
        {
            SliderPathType.Linear => PathApproximation.ApproximateLinear(controlPoints),
            SliderPathType.PerfectCurve when controlPoints.Count == 3 =>
                PathApproximation.ApproximateCircularArc(controlPoints, cancellationToken),
            SliderPathType.PerfectCurve => PathApproximation.ApproximateBezier(
                controlPoints,
                cancellationToken
            ),
            SliderPathType.Catmull => PathApproximation.ApproximateCatmull(
                controlPoints,
                cancellationToken
            ),
            SliderPathType.Bezier => PathApproximation.ApproximateBezier(
                controlPoints,
                cancellationToken
            ),
            _ => PathApproximation.ApproximateBezier(controlPoints, cancellationToken),
        };

    private double ProgressToDistance(double progress) =>
        System.Math.Clamp(progress, 0, 1) * ExpectedDistance;

    private ReferenceVector2 InterpolateVertices(int index, double distance)
    {
        if (CalculatedPath.Count == 0)
        {
            return new ReferenceVector2();
        }

        if (index <= 0)
        {
            return CalculatedPath[0];
        }

        if (index >= CalculatedPath.Count)
        {
            return CalculatedPath[^1];
        }

        ReferenceVector2 point0 = CalculatedPath[index - 1];
        ReferenceVector2 point1 = CalculatedPath[index];
        double distance0 = CumulativeLength[index - 1];
        double distance1 = CumulativeLength[index];

        if (Precision.AlmostEquals(distance0, distance1))
        {
            return point0;
        }

        float t = (float)((distance - distance0) / (distance1 - distance0));
        return new ReferenceVector2(
            point0.X + (point1.X - point0.X) * t,
            point0.Y + (point1.Y - point0.Y) * t
        );
    }

    private int IndexOfDistance(double distance)
    {
        if (CumulativeLength.Count == 0 || distance < CumulativeLength[0])
        {
            return 0;
        }

        if (distance >= CumulativeLength[^1])
        {
            return CumulativeLength.Count;
        }

        int left = 0;
        int right = CumulativeLength.Count - 2;

        while (left <= right)
        {
            int pivot = left + ((right - left) >> 1);
            double length = CumulativeLength[pivot];
            if (length < distance)
            {
                left = pivot + 1;
            }
            else if (length > distance)
            {
                right = pivot - 1;
            }
            else
            {
                return pivot;
            }
        }

        return left;
    }
}
