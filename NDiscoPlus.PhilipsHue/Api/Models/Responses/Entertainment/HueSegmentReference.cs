using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;

public class HueSegmentReference
{
    [JsonConstructor]
    internal HueSegmentReference(HueResourceIdentifier service, int index)
    {
        Service = service;
        Index = index;
    }

    public HueResourceIdentifier Service { get; }
    public int Index { get; }
}
