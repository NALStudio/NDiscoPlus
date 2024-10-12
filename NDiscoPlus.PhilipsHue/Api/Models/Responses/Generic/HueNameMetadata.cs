using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
public readonly struct HueNameMetadata
{
    public string Name { get; }

    [JsonConstructor]
    internal HueNameMetadata(string name)
    {
        Name = name;
    }
}
