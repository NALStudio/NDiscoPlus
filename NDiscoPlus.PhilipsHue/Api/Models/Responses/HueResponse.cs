using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System.Collections.Immutable;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses;

public abstract class HueResponse<T>
{
    public required ImmutableArray<HueError> Errors { get; init; }
    public required T Data { get; init; }
}