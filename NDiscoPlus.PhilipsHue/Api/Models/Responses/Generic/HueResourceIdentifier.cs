using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;

/// <summary>
/// Supports serialize and deserialize.
/// </summary>
public class HueResourceIdentifier
{
    internal HueResourceIdentifier(string resourceId, string resourceType)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
    }

    [JsonPropertyName("rid")]
    public string ResourceId { get; }

    [JsonPropertyName("rtype")]
    public string ResourceType { get; }
}
