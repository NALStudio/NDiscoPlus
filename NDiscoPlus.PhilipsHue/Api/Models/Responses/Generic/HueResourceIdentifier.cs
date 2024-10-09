using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;

/// <summary>
/// Supports serialize and deserialize.
/// </summary>
public sealed class HueResourceIdentifier
{
    internal HueResourceIdentifier(Guid resourceId, string resourceType)
    {
        ResourceId = resourceId;
        ResourceType = new HueResourceType(resourceType);
    }

    [JsonPropertyName("rid")]
    public Guid ResourceId { get; }

    [JsonPropertyName("rtype")]
    public HueResourceType ResourceType { get; }
}
