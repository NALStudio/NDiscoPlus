﻿using NDiscoPlus.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;

public readonly record struct NDPLightBounds(double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ);

// MUST BE READ-ONLY
// AS THIS IS SHARED WITH BOTH BACKGROUND CHANNEL AND BACKGROUND EFFECT CHANNEL
public sealed class NDPLightCollection : IReadOnlyDictionary<LightId, NDPLight>, IEnumerable<NDPLight>
{
    private readonly ImmutableDictionary<LightId, NDPLight> lights;

    public NDPLightBounds Bounds { get; }

    private NDPLightCollection(Dictionary<LightId, NDPLight> lights, NDPLightBounds bounds)
    {
        this.lights = lights.ToImmutableDictionary();

        Bounds = bounds;
    }
    /// <summary>
    /// Group lights from left to right.
    /// </summary>
    public List<NDPLight[]> GroupX(int count) => lights.Values.ChunkByPositionByGroupNumber(count, static l => l.Position.X).ToList();

    /// <summary>
    /// Group lights from back to front.
    /// </summary>
    public List<NDPLight[]> GroupY(int count) => lights.Values.ChunkByPositionByGroupNumber(count, static l => l.Position.Y).ToList();

    /// <summary>
    /// Group lights from bottom to top.
    /// </summary>
    public List<NDPLight[]> GroupZ(int count) => lights.Values.ChunkByPositionByGroupNumber(count, static l => l.Position.Z).ToList();


    /// <inheritdoc cref="Split"/>
    /// <remarks>
    /// Lights are ordered from left to right.
    /// </remarks>
    public List<NDPLight[]> SplitX(double tolerance) => Split(tolerance, static l => l.Position.X).ToList();

    /// <inheritdoc cref="Split"/>
    /// <remarks>
    /// Lights are ordered from back to front
    /// </remarks>
    public List<NDPLight[]> SplitY(double tolerance) => Split(tolerance, static l => l.Position.Y).ToList();

    /// <inheritdoc cref="Split"/>
    /// <remarks>
    /// Lights are ordered from bottom to top.
    /// </remarks>
    public List<NDPLight[]> SplitZ(double tolerance) => Split(tolerance, static l => l.Position.Z).ToList();

    /// <summary>
    /// Split lights into groups where their distance is more than <paramref name="tolerance"/>.
    /// </summary>
    private IEnumerable<NDPLight[]> Split(double tolerance, Func<NDPLight, double> positionSelector)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tolerance);

        (NDPLight Light, double Position)[] positions = lights.Values.Select(l => (l, positionSelector(l))).ToArray();
        Array.Sort(positions, static (a, b) => a.Position.CompareTo(b.Position)); // sort from smallest to largest (-1 to 1 since positions are clamped to this range)

        List<NDPLight> output = new();
        double? lastPos = null;
        foreach ((NDPLight light, double pos) in positions)
        {
            if (lastPos is null)
            {
                output.Add(light);
            }
            else
            {
                Debug.Assert(lastPos <= pos);
                if ((pos - lastPos) <= tolerance)
                {
                    output.Add(light);
                }
                else
                {
                    yield return output.ToArray();
                    output.Clear();
                }
            }
        }

        if (output.Count > 0)
            yield return output.ToArray();
    }


    public NDPLight Random(Random random)
    {
        int count = lights.Count;
        if (count < 1)
            throw new InvalidOperationException("Cannot take a random value from an empty collection.");

        IEnumerator<NDPLight> lightEnumerator = lights.Values.GetEnumerator();
        int index = random.Next(count);

        // -1 since at least one MoveNext must happen before current value can be fetched.
        while (index > -1)
        {
            bool mn = lightEnumerator.MoveNext();
            Debug.Assert(mn);
            index--;
        }

        return lightEnumerator.Current;
    }


    public static NDPLightCollection Create(IEnumerable<NDPLight> lights)
    {
        Dictionary<LightId, NDPLight> lightDict = new();

        double minX = 0;
        double maxX = 0;
        double minY = 0;
        double maxY = 0;
        double minZ = 0;
        double maxZ = 0;

        foreach (NDPLight light in lights)
        {
            lightDict.Add(light.Id, light);

            double x = light.Position.X;
            double y = light.Position.Y;
            double z = light.Position.Z;

            if (x < minX)
                minX = x;
            if (x > maxX)
                maxX = x;
            if (y < minY)
                minY = y;
            if (y > maxY)
                maxY = y;
            if (z < minZ)
                minZ = z;
            if (z > maxZ)
                maxZ = z;
        }

        return new NDPLightCollection(
            lightDict,
            new NDPLightBounds(MinX: minX, MaxX: maxX, MinY: minY, MaxY: maxY, MinZ: minZ, MaxZ: maxZ)
        );
    }

    public int Count => lights.Count;
    public IEnumerable<LightId> Keys => lights.Keys;
    public IEnumerable<NDPLight> Values => lights.Values;

    public NDPLight this[LightId key] => lights[key];

    public bool ContainsKey(LightId key) => lights.ContainsKey(key);
    public bool TryGetValue(LightId key, [MaybeNullWhen(false)] out NDPLight value) => lights.TryGetValue(key, out value);

    public IEnumerator<NDPLight> GetEnumerator() => lights.Values.GetEnumerator();
    IEnumerator<KeyValuePair<LightId, NDPLight>> IEnumerable<KeyValuePair<LightId, NDPLight>>.GetEnumerator() => lights.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}