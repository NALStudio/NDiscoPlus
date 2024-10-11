using NDiscoPlus.Shared.Models.Color;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Shared.Models;

public readonly struct LightPosition
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public LightPosition(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public readonly struct NDPLight
{
    public required LightId Id { get; init; }
    public required string? DisplayName { get; init; }

    public required LightPosition Position { get; init; }
    public required ColorGamut? ColorGamut { get; init; }
    public required TimeSpan? ExpectedLatency { get; init; }
}