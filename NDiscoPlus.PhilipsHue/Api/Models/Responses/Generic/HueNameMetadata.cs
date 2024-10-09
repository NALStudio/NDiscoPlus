using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
public readonly struct HueNameMetadata
{
    internal HueNameMetadata(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
