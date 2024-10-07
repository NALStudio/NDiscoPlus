using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;

public abstract partial class HueRequestBuilder
{
    private readonly Dictionary<string, object> data = new();

    /// <summary>
    /// Add a new property object to this builder.
    /// </summary>
    protected void AddProperty(string name, Dictionary<string, object> values)
    {
        if (!data.TryAdd(name, values))
            throw new ArgumentException($"Property with name '{name}' has been added already.");
        data.Add(name, values);
    }

    /// <summary>
    /// Add a new property using a wrapper dictionary with value: { innerName: value }
    /// </summary>
    protected void AddProperty(string name, string innerName, object value) => AddProperty(name, new Dictionary<string, object>() { { innerName, value } });

    protected PropertyBuilder BuildProperty(string name) => new(this, name);

    public object Build()
    {
        return data.ToImmutableArray();
    }
}