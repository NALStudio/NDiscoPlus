using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Authentication.Models;
internal class AuthenticationRequest
{
    [JsonPropertyName("devicetype")]
    public required string DeviceType { get; init; }

    [JsonPropertyName("generateclientkey")]
    public bool GenerateClientKey { get; } = true;
}
