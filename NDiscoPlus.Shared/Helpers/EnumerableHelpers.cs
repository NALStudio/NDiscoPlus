using System.Numerics;

namespace NDiscoPlus.Shared.Helpers;

public static class EnumerableHelpers
{
    /// <summary>
    /// <see cref="Enumerable.Zip{TFirst, TSecond, TResult}(IEnumerable{TFirst}, IEnumerable{TSecond}, Func{TFirst, TSecond, TResult})"/> with forced equal length.
    /// </summary>
    public static IEnumerable<TResult> ZipStrict<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        using IEnumerator<TFirst> e1 = first.GetEnumerator();
        using IEnumerator<TSecond> e2 = second.GetEnumerator();

        while (true)
        {
            bool next1 = e1.MoveNext();
            bool next2 = e2.MoveNext();
            if (next1 != next2)
                throw new InvalidOperationException("Sequences differed in length.");

            if (next1)
                yield return resultSelector(e1.Current, e2.Current);
            else
                yield break;
        }
    }
    /// <summary>
    /// <see cref="Enumerable.Zip{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/> with forced equal length.
    /// </summary>
    public static IEnumerable<(TFirst First, TSecond Second)> ZipStrict<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
        => ZipStrict(first, second, static (first, second) => (first, second));

    /// <summary>
    /// Returns the enumerable with all <see langword="null"/> values skipped.
    /// </summary>
    public static IEnumerable<T> NonNulls<T>(this IEnumerable<T?> enumerable)
    {
        foreach (T? value in enumerable)
        {
            if (value is not null)
                yield return value;
        }
    }

    public static T EuclideanDistance<T>(IEnumerable<T> a, IEnumerable<T> b) where T : INumberBase<T>, IRootFunctions<T>
    {
        T x = T.Zero;
        foreach ((T v1, T v2) in ZipStrict(a, b))
        {
            // v1 and v2 direction does not matter as we square this value anyways...
            T diff = v1 - v2;
            x += diff * diff;
        }

        return T.Sqrt(x);
    }

    private readonly record struct ValuePosition<T>(T Value, double Position)
    {
        public void Deconstruct(out T value, out double position)
        {
            value = Value;
            position = Position;
        }
    }

    private readonly record struct PositionGroup<T>(double GroupPosition, List<T> Values);

    /// <summary>
    /// Chunk <paramref name="values"/> values into <paramref name="groups"/> groups by position.
    /// </summary>
    public static IEnumerable<T[]> ChunkByPositionByGroupNumber<T>(this IEnumerable<T> values, int groups, Func<T, double> positionSelector)
    {
        static double Distance(double a, double b)
            => Math.Abs(a - b);

        ArgumentOutOfRangeException.ThrowIfLessThan(groups, 1, nameof(groups));

        // Sort by position (start)
        ValuePosition<T>[] positions = values.Select(v => new ValuePosition<T>(v, positionSelector(v))).ToArray();

        // early return if no values provided
        if (positions.Length < 1)
            return Enumerable.Repeat(Array.Empty<T>(), groups).ToList();

        Array.Sort(positions, (a, b) => a.Position.CompareTo(b.Position));
        // Sort by position (end)

        // Generate group position points
        // these are kind of like hooks where the closest values will cling to.
        double minPos = positions[0].Position;
        double maxPos = positions[^1].Position;

        // Create groups
        PositionGroup<T>[] grouped = new PositionGroup<T>[groups];
        for (int i = 0; i < groups; i++)
        {
            double groupPos = DoubleHelpers.LerpUnclamped(minPos, maxPos, i / (double)(groups - 1));
            List<T> groupValues = new();
            grouped[i] = new PositionGroup<T>(groupPos, groupValues);
        }

        // Assign values to closest group (by group position point)
        int closestIndex = 0;
        foreach ((T val, double pos) in positions) // values are ordered from minPos to maxPos
        {
            int lastIndex = groups - 1;
            if (closestIndex < lastIndex) // when we get to the last index, the closest group cannot be any further right so we don't check distances anymore
            {
                int nextIndex = closestIndex + 1;

                double leftDist = Distance(grouped[closestIndex].GroupPosition, pos);
                double rightDist = Distance(pos, grouped[nextIndex].GroupPosition);

                if (rightDist < leftDist)
                    closestIndex = nextIndex;
            }

            grouped[closestIndex].Values.Add(val);
        }

        return grouped.Select(g => g.Values.ToArray());
    }
}
