using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;
public class LightColorCollection : IReadOnlyDictionary<LightId, NDPColor>
{
    private readonly IReadOnlyDictionary<LightId, NDPColor> values;

    public LightColorCollection(IEnumerable<KeyValuePair<LightId, NDPColor>> values) : this(values.ToImmutableDictionary())
    {
    }

    private LightColorCollection(IReadOnlyDictionary<LightId, NDPColor> values)
    {
        this.values = values;
    }

    public NDPColor this[LightId key] => values[key];
    public IEnumerable<LightId> Keys => values.Keys;
    public IEnumerable<NDPColor> Values => values.Values;
    public int Count => values.Count;

    public static LightColorCollection UnsafeRef(IDictionary<LightId, NDPColor> values)
        => new((IReadOnlyDictionary<LightId, NDPColor>)values);
    public static LightColorCollection Black(IEnumerable<NDPLight> lights)
        => new(lights.ToDictionary(key => key.Id, value => value.ColorGamut.GamutBlack()));
    public static LightColorCollection Black(IEnumerable<LightRecord> lights)
        => Black(lights.Select(l => l.Light));

    public IEnumerable<KeyValuePair<T, NDPColor>> OfType<T>() where T : LightId
    {
        foreach ((LightId lightId, NDPColor color) in values)
        {
            if (lightId is T li)
                yield return new KeyValuePair<T, NDPColor>(li, color);
        }
    }

    public bool ContainsKey(LightId key) => values.ContainsKey(key);
    public bool TryGetValue(LightId key, [MaybeNullWhen(false)] out NDPColor value)
        => values.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<LightId, NDPColor>> GetEnumerator() => values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
