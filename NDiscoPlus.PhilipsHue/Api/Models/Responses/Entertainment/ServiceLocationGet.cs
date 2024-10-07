using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
public class ServiceLocationGet
{
    internal ServiceLocationGet(HueResourceIdentifier service, ImmutableArray<HuePosition> positions, double equalizationFactor)
    {
        Service = service;
        Positions = positions;
        EqualizationFactor = equalizationFactor;
    }

    public HueResourceIdentifier Service { get; }
    public ImmutableArray<HuePosition> Positions { get; }

    [JsonPropertyName("equalization_factor")]
    public double EqualizationFactor { get; }
}
