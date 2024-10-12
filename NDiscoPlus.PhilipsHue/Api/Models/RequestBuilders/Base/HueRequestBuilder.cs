using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;

public abstract partial class HueRequestBuilder
{
    private bool immutable = false;
    private readonly Dictionary<string, object> data = new();

    /// <summary>
    /// Add a new property object to this builder.
    /// </summary>
    protected void AddProperty(string name, Dictionary<string, object> values)
    {
        ThrowIfImmutable();

        if (!data.TryAdd(name, values))
            throw new ArgumentException($"Property with name '{name}' has been added already.");
    }

    protected void AddOrUpdateProperty(string name, Func<Dictionary<string, object>> onAdd, Func<Dictionary<string, object>, Dictionary<string, object>> onUpdate)
    {
        ThrowIfImmutable();

        Dictionary<string, object> value;
        if (data.TryGetValue(name, out object? v))
            value = onUpdate((Dictionary<string, object>)v);
        else
            value = onAdd();

        data[name] = value;
    }

    /// <summary>
    /// Add a new property using a wrapper dictionary with value: { innerName: value }
    /// </summary>
    protected void AddProperty(string name, string innerName, object value) => AddProperty(name, new Dictionary<string, object>() { { innerName, value } });

    protected void AddOrUpdateProperty(string name, string innerName, Func<object> onAdd, Func<object, object> onUpdate)
    {
        Dictionary<string, object> OnAdd() => new() { { innerName, onAdd() } };
        Dictionary<string, object> OnUpdate(Dictionary<string, object> value)
        {
            value[innerName] = onUpdate(value[innerName]);
            return value;
        }

        AddOrUpdateProperty(name, OnAdd, OnUpdate);
    }

    protected PropertyBuilder BuildProperty(string name) => new(this, name);

    public object Build()
    {
        immutable = true;
        return data.AsReadOnly();
    }

    private void ThrowIfImmutable()
    {
        if (immutable)
            throw new InvalidOperationException("Request has already been built.");
    }
}