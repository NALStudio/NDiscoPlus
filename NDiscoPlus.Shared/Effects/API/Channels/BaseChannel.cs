using NDiscoPlus.Shared.Models;
using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Shared.Effects.API.Channels;
public abstract class BaseChannel
{
    public NDPLightCollection Lights { get; }

    protected BaseChannel(IEnumerable<NDPLight> lights)
    {
        Lights = NDPLightCollection.Create(lights);
    }
    protected BaseChannel(NDPLightCollection lights)
    {
        Lights = lights;
    }

    public NDPLight GetLight(LightId id) => Lights[id];
    public bool TryGetLight(LightId id, out NDPLight light) => Lights.TryGetValue(id, out light);
}
