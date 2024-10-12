using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
public class HueGamut
{
    [JsonConstructor]
    internal HueGamut(HueXY red, HueXY green, HueXY blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public HueXY Red { get; }
    public HueXY Green { get; }
    public HueXY Blue { get; }
}
