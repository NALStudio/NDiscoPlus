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
    [JsonConstructor]
    internal HueResourceIdentifier(Guid resourceId, HueResourceType resourceType)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
    }

    [JsonPropertyName("rid")]
    public Guid ResourceId { get; }

    [JsonPropertyName("rtype")]
    public HueResourceType ResourceType { get; }
}
