using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
public class HueLightColor
{
    [JsonConstructor]
    internal HueLightColor(HueXY xY, HueGamut gamut)
    {
        XY = xY;
        Gamut = gamut;
    }

    public HueXY XY { get; }
    public HueGamut Gamut { get; }
    // TODO: Add gamut type
}
