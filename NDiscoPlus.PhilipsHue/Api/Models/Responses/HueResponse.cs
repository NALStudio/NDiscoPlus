using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NDiscoPlus.PhilipsHue.Api.Models.Responses;

internal sealed class HueResponse<T>
{
    public ImmutableArray<HueError> Errors { get; }
    public T Data { get; }

    [JsonConstructor]
    internal HueResponse(ImmutableArray<HueError> errors, T data)
    {
        Errors = errors;
        Data = data;
    }
}