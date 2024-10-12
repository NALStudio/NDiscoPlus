using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;

public class HueError
{
    public string Description { get; }

    [JsonConstructor]
    internal HueError(string description)
    {
        Description = description;
    }
}