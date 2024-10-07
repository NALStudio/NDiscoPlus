using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
public readonly struct NameMetadata
{
    internal NameMetadata(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
