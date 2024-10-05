using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Helpers;
public static class RandomHelpers
{
    /// <summary>
    /// Picks a random element from the list.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">List is empty.</exception>
    public static T Choice<T>(this Random random, IReadOnlyList<T> values)
    {
        int valuesCount = values.Count;
        if (valuesCount < 1)
            throw new ArgumentOutOfRangeException(nameof(values), "Cannot choose an element from an empty list.");
        return values[random.Next(valuesCount)];
    }

    /// <summary>
    /// Pick an item from a list using weights.
    /// </summary>
    /// <param name="random">null for <see cref="Random.Shared"/></param>
    public static T WeightedChoice<T>(this Random random, IList<T> values, IList<int> weights)
    {
        int sum = weights.Sum();
        int rand = random.Next(sum);

        int cumsum = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            int w = weights[i];
            if (w < 0)
                throw new ArgumentException("All weights must be positive.", nameof(weights));

            cumsum += w;
            if (cumsum > rand)
                return values[i];
        }

        throw new Exception("Unreachable.");
    }

    /// <summary>
    /// Group items into <paramref name="count"/> amount of equal sized portions.
    /// </summary>
    public static IEnumerable<T[]> Group<T>(this Random random, IEnumerable<T> values, int count)
    {
        // randomize values
        T[] randomized = values.ToArray();
        random.Shuffle(randomized);

        int valuesCount = randomized.Length; // this array is created from values so the lengths are the same

        // split values into n (count) chunks
        int chunkSize = valuesCount / count;
        int leftOver = valuesCount - (count * chunkSize);

        List<T[]> output = new(count);
        int index = 0;
        for (int i = 0; i < count; i++)
        {
            int end = index + chunkSize;
            if (i < leftOver)
                end++;
            output.Add(randomized[index..end]);
            index = end;
        }

        Debug.Assert(index == valuesCount);

        // return split chunks in random order (so that the longer and shorter chunk order is randomized)
        while (output.Count > 0)
        {
            int yieldIndex = random.Next(output.Count);
            yield return output[yieldIndex];
            output.RemoveAt(yieldIndex);
        }
    }

    public static string GetHex(this Random random, int length)
    {
        (int bytesLength, int remaining) = Math.DivRem(length, 2);

        if (remaining != 0)
            throw new ArgumentException("Length must be a multiple of 2.");

        Span<byte> bytes = stackalloc byte[bytesLength];
        random.NextBytes(bytes);

        string hex = Convert.ToHexString(bytes);
        Debug.Assert(hex.Length == length);
        return hex;
    }
}
