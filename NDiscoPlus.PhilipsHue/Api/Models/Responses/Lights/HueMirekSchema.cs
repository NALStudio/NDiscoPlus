using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
public class HueMirekSchema
{
    [JsonPropertyName("mirek_minimum")]
    public int MirekMinimum { get; }

    [JsonPropertyName("mirek_maximum")]
    public int MirekMaximum { get; }
}