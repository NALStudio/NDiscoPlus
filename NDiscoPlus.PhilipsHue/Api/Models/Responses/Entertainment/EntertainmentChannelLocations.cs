using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class EntertainmentChannelLocations
{
    internal EntertainmentChannelLocations(ImmutableArray<ServiceLocationGet> serviceLocations)
    {
        ServiceLocations = serviceLocations;
    }

    [JsonPropertyName("service_locations")]
    public ImmutableArray<ServiceLocationGet> ServiceLocations { get; }
}
