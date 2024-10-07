using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
public class HueLightColorTemperature
{
    internal HueLightColorTemperature(int mirek, bool mirekValid, HueMirekSchema mirekSchema)
    {
        Mirek = mirek;
        MirekValid = mirekValid;
        MirekSchema = mirekSchema;
    }

    public int Mirek { get; }

    [JsonPropertyName("mirek_valid")]
    public bool MirekValid { get; }

    [JsonPropertyName("mirek_schema")]
    public HueMirekSchema MirekSchema { get; }
}