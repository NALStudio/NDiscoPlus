using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;

internal readonly record struct HueLightOn(bool On);
public class HueLight
{
    internal HueLight(Guid id, HueResourceIdentifier owner, HueNameMetadata metadata, HueLightOn hueLightOn, HueLightDimming? dimming, HueLightColorTemperature? colorTemperature, HueLightColor? color)
    {
        Id = id;
        Owner = owner;
        Metadata = metadata;
        this.hueLightOn = hueLightOn;
        Dimming = dimming;
        ColorTemperature = colorTemperature;
        Color = color;
    }

    public Guid Id { get; }
    public HueResourceIdentifier Owner { get; }
    public HueNameMetadata Metadata { get; } // TODO: Include archetype and function metadata

    [JsonInclude, JsonPropertyName("on")]
    private readonly HueLightOn hueLightOn;
    [JsonIgnore]
    public bool On => hueLightOn.On;

    public HueLightDimming? Dimming { get; }

    [JsonPropertyName("color_temperature")]
    public HueLightColorTemperature? ColorTemperature { get; }

    public HueLightColor? Color { get; }

    // TODO: Add missing members
}
