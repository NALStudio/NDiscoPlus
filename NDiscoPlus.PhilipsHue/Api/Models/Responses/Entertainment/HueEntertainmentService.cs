using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class HueEntertainmentService
{
    [JsonConstructor]
    internal HueEntertainmentService(Guid id, HueResourceIdentifier owner, bool renderer, HueResourceIdentifier rendererReference, bool proxy, bool equalizer)
    {
        Id = id;
        Owner = owner;
        Renderer = renderer;
        RendererReference = rendererReference;
        Proxy = proxy;
        Equalizer = equalizer;
    }

    public Guid Id { get; }
    public HueResourceIdentifier Owner { get; }
    public bool Renderer { get; }

    [JsonPropertyName("renderer_reference")]
    public HueResourceIdentifier? RendererReference { get; }

    public bool Proxy { get; }
    public bool Equalizer { get; }

    // TODO: Implement segments
}
