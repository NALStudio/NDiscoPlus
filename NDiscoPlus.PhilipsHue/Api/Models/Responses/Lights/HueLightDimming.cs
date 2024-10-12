using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
public class HueLightDimming
{
    [JsonConstructor]
    internal HueLightDimming(double brightness, double minDimLevel)
    {
        Brightness = brightness;
        MinDimLevel = minDimLevel;
    }

    public double Brightness { get; }

    [JsonPropertyName("min_dim_level")]
    public double MinDimLevel { get; }
}
