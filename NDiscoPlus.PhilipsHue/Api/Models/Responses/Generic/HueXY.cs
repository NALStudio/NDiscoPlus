using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
public readonly record struct HueXY(double X, double Y)
{
    public Dictionary<string, object> ToObjectDictionary()
    {
        return new Dictionary<string, object>()
        {
            { "x", X },
            { "y", Y }
        };
    }
}