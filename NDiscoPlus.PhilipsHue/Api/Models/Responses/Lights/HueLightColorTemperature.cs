using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
public class HueLightColorTemperature
{
    [JsonConstructor]
    internal HueLightColorTemperature(int? mirek, bool mirekValid, HueMirekSchema mirekSchema)
    {
        Debug.Assert((mirek is not null) == mirekValid);

        Mirek = mirek;
        MirekValid = mirekValid;
        MirekSchema = mirekSchema;
    }

    public int? Mirek { get; }

    [JsonPropertyName("mirek_valid"), MemberNotNullWhen(true, nameof(Mirek))]
    public bool MirekValid { get; }

    [JsonPropertyName("mirek_schema")]
    public HueMirekSchema MirekSchema { get; }
}