using System.Collections;

namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal abstract class ControlPointManager<T>(T defaultControlPoint) : IEnumerable<T>
    where T : ControlPoint
{
    public T DefaultControlPoint { get; } = defaultControlPoint;

    public List<T> ControlPoints { get; } = [];

    public abstract T ControlPointAt(double time);

    public bool Add(T controlPoint)
    {
        T existing = ControlPointAt(controlPoint.Time);
        if (controlPoint.IsRedundant(existing))
        {
            return false;
        }

        while (controlPoint.Time == existing.Time)
        {
            if (!Remove(existing))
            {
                break;
            }

            existing = ControlPointAt(controlPoint.Time);
        }

        ControlPoints.Insert(FindInsertionIndex(controlPoint.Time), controlPoint);
        return true;
    }

    public bool Remove(T controlPoint) => ControlPoints.Remove(controlPoint);

    public T? Remove(int index)
    {
        if (index < 0 || index > ControlPoints.Count - 1)
        {
            return null;
        }

        T removed = ControlPoints[index];
        ControlPoints.RemoveAt(index);
        return removed;
    }

    public void Clear() => ControlPoints.Clear();

    public List<T> Between(double start, double end)
    {
        if (ControlPoints.Count == 0)
        {
            return [DefaultControlPoint];
        }

        if (start > end)
        {
            return [ControlPointAt(start)];
        }

        int startIndex = System.Math.Max(0, FindInsertionIndex(start) - 1);
        int endIndex = System.Math.Clamp(
            FindInsertionIndex(end),
            startIndex + 1,
            ControlPoints.Count
        );
        return ControlPoints.GetRange(startIndex, endIndex - startIndex);
    }

    protected T BinarySearchWithFallback(double time, T? fallback = null) =>
        BinarySearch(time) ?? fallback ?? DefaultControlPoint;

    protected T? BinarySearch(double time)
    {
        if (ControlPoints.Count == 0 || time < ControlPoints[0].Time)
        {
            return null;
        }

        T lastControlPoint = ControlPoints[^1];
        if (time >= lastControlPoint.Time)
        {
            return lastControlPoint;
        }

        int left = 0;
        int right = ControlPoints.Count - 2;

        while (left <= right)
        {
            int pivot = left + ((right - left) >> 1);
            T controlPoint = ControlPoints[pivot];
            if (controlPoint.Time < time)
            {
                left = pivot + 1;
            }
            else if (controlPoint.Time > time)
            {
                right = pivot - 1;
            }
            else
            {
                return controlPoint;
            }
        }

        return ControlPoints[left - 1];
    }

    private int FindInsertionIndex(double time)
    {
        if (ControlPoints.Count == 0 || time < ControlPoints[0].Time)
        {
            return 0;
        }

        if (time >= ControlPoints[^1].Time)
        {
            return ControlPoints.Count;
        }

        int left = 0;
        int right = ControlPoints.Count - 2;

        while (left <= right)
        {
            int pivot = left + ((right - left) >> 1);
            T controlPoint = ControlPoints[pivot];
            if (controlPoint.Time < time)
            {
                left = pivot + 1;
            }
            else if (controlPoint.Time > time)
            {
                right = pivot - 1;
            }
            else
            {
                return pivot + 1;
            }
        }

        return left;
    }

    public IEnumerator<T> GetEnumerator() => ControlPoints.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
