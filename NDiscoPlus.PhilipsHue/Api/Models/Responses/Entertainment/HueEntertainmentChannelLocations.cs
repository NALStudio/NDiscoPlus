using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class HueEntertainmentChannelLocations
{
    [JsonConstructor]
    internal HueEntertainmentChannelLocations(ImmutableArray<HueServiceLocation> serviceLocations)
    {
        ServiceLocations = serviceLocations;
    }

    [JsonPropertyName("service_locations")]
    public ImmutableArray<HueServiceLocation> ServiceLocations { get; }
}
