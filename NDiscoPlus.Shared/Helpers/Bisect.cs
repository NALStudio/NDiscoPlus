using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Helpers;

// Port of bisect.py to C#
// https://github.com/python/cpython/blob/69c68de43aef03dd52fabd21f99cb3b0f9329201/Lib/bisect.py
public static class Bisect
{
    /// <summary>
    /// <para>Insert item x in list a, and keep it sorted assuming a is sorted.</para>
    /// <para>If x is already in a, insert it to the right of the rightmost x.</para>
    /// </summary>
    public static void InsortRight<T>(IList<T> a, T x) where T : IComparable<T>
        => InsortRight(a, x, low: 0, high: a.Count);

    /// <summary>
    /// <inheritdoc cref="InsortRight{T}(IList{T}, T)"/>
    /// </summary>
    /// <param name="low">Bound the slice to be searched.</param>
    /// <param name="high">Bound the slice to be searched.</param>
    public static void InsortRight<T>(IList<T> a, T x, int low, int high) where T : IComparable<T>
    {
        int index = BisectRight(a, x, low: low, high: high);
        a.Insert(index, x);
    }

    /// <summary>
    /// <inheritdoc cref="InsortRight{T}(IList{T}, T)"/>
    /// </summary>
    public static void InsortRight<TCollection, TKey>(IList<TCollection> a, TCollection x, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
        => InsortRight(a, x, low: 0, high: a.Count, keySelector);

    /// <summary>
    /// <inheritdoc cref="InsortRight{T}(IList{T}, T)"/>
    /// </summary>
    public static void InsortRight<TCollection, TKey>(IList<TCollection> a, TCollection x, int low, int high, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
    {
        int index = BisectRight(a, keySelector(x), low: low, high: high, keySelector);
        a.Insert(index, x);
    }

    /// <summary>
    /// <para>Return the index where to insert item x in list a, assuming a is sorted.</para>
    /// <para>
    /// The return value i is such that all e in a[..i] have e &lt;= x, and all e in
    /// a[i..] have e > x. So if x already appears in the list, a.Insert(i, x) will
    /// insert just after the rightmost x already there.
    /// </para>
    /// </summary>
    public static int BisectRight<T>(IList<T> a, T x) where T : IComparable<T>
        => BisectRight(a, x, low: 0, high: a.Count);

    /// <inheritdoc/>
    /// <param name="low">Bound the slice to be searched.</param>
    /// <param name="high">Bound the slice to be searched.</param>
    public static int BisectRight<T>(IList<T> a, T x, int low, int high) where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(low, 0, nameof(low));

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (x.CompareTo(a[mid]) < 0)
                high = mid;
            else
                low = mid + 1;
        }

        return low;
    }

    /// <summary>
    /// <inheritdoc cref="BisectRight{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectRight<TCollection, TKey>(IList<TCollection> a, TKey x, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
        => BisectRight(a, x, low: 0, high: a.Count, keySelector);

    /// <summary>
    /// <inheritdoc cref="BisectRight{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectRight<TCollection, TKey>(IList<TCollection> a, TCollection x, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
        => BisectRight(a, keySelector(x), keySelector);

    /// <summary>
    /// <inheritdoc cref="BisectRight{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectRight<TCollection, TKey>(IList<TCollection> a, TCollection x, int low, int high, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
        => BisectRight(a, keySelector(x), low: low, high: high, keySelector);

    /// <summary>
    /// <inheritdoc cref="BisectRight{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectRight<TCollection, TKey>(IList<TCollection> a, TKey x, int low, int high, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(low, 0, nameof(low));

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (x.CompareTo(keySelector(a[mid])) < 0)
                high = mid;
            else
                low = mid + 1;
        }

        return low;
    }

    /// <summary>
    /// <para>Insert item x in list a, and keep it sorted assuming a is sorted.</para>
    /// <para>If x is already in a, insert it to the left of the leftmost x.</para>
    /// </summary>
    public static void InsortLeft<T>(IList<T> a, T x) where T : IComparable<T>
        => InsortLeft(a, x, low: 0, high: a.Count);

    /// <summary>
    /// <inheritdoc cref="InsortLeft{T}(IList{T}, T)"/>
    /// </summary>
    /// <param name="low">Bound the slice to be searched.</param>
    /// <param name="high">Bound the slice to be searched.</param>
    public static void InsortLeft<T>(IList<T> a, T x, int low, int high) where T : IComparable<T>
    {
        int index = BisectLeft(a, x, low: low, high: high);
        a.Insert(index, x);
    }

    /// <summary>
    /// <inheritdoc cref="InsortLeft{T}(IList{T}, T)"/>
    /// </summary>
    public static void InsortLeft<TCollection, TKey>(IList<TCollection> a, TCollection x, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
        => InsortLeft(a, x, low: 0, high: a.Count, keySelector);

    /// <summary>
    /// <inheritdoc cref="InsortLeft{T}(IList{T}, T)"/>
    /// </summary>
    public static void InsortLeft<TCollection, TKey>(IList<TCollection> a, TCollection x, int low, int high, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
    {
        int index = BisectLeft(a, keySelector(x), low: low, high: high, keySelector);
        a.Insert(index, x);
    }

    /// <summary>
    /// <para>Return the index where to insert item x in list a, assuming a is sorted.</para>
    /// <para>
    /// The return value i is such that all e in a[..i] have e &lt; x, and all e in
    /// a[i..] have e >= x.So if x already appears in the list, a.Insert(i, x) will
    /// insert just before the leftmost x already there.
    /// </para>
    /// </summary>
    public static int BisectLeft<T>(IList<T> a, T x) where T : IComparable<T>
        => BisectLeft(a, x, low: 0, high: a.Count);

    /// <summary>
    /// <inheritdoc cref="BisectLeft{T}(IList{T}, T)"/>
    /// </summary>
    /// <param name="low">Bound the slice to be searched.</param>
    /// <param name="high">Bound the slice to be searched.</param>
    public static int BisectLeft<T>(IList<T> a, T x, int low, int high) where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(low, 0, nameof(low));

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (a[mid].CompareTo(x) < 0)
                low = mid + 1;
            else
                high = mid;
        }

        return low;
    }

    /// <summary>
    /// <inheritdoc cref="BisectLeft{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectLeft<TCollection, TKey>(IList<TCollection> a, TKey x, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
        => BisectLeft(a, x, low: 0, high: a.Count, keySelector);

    /// <summary>
    /// <inheritdoc cref="BisectLeft{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectLeft<TCollection, TKey>(IList<TCollection> a, TCollection x, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
    => BisectLeft(a, keySelector(x), keySelector);

    /// <summary>
    /// <inheritdoc cref="BisectLeft{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectLeft<TCollection, TKey>(IList<TCollection> a, TCollection x, int low, int high, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
    => BisectLeft(a, keySelector(x), low: low, high: high, keySelector);

    /// <summary>
    /// <inheritdoc cref="BisectLeft{T}(IList{T}, T)"/>
    /// </summary>
    public static int BisectLeft<TCollection, TKey>(IList<TCollection> a, TKey x, int low, int high, Func<TCollection, TKey> keySelector) where TKey : IComparable<TKey>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(low, 0, nameof(low));

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (keySelector(a[mid]).CompareTo(x) < 0)
                low = mid + 1;
            else
                high = mid;
        }

        return low;
    }
}
