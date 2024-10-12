using System.Net;

namespace NDiscoPlus.PhilipsHue.Api.Constants;

internal abstract class HueEndpoints
{
    protected readonly string bridge;

    protected HueEndpoints(string bridgeIp)
    {
        VerifyBridgeIp(bridgeIp);
        bridge = bridgeIp;
    }

    private static void VerifyBridgeIp(string bridgeIp)
    {
        if (!IPAddress.TryParse(bridgeIp, out _))
            throw new ArgumentException($"Invalid bridge ip: '{bridgeIp}'");
    }

    public string BridgeAddress => $"https://{bridge}";
}

internal readonly struct ClipV2Endpoint
{
    private const string ClipPath = "/clip/v2";
    private string EndpointPath { get; }

    public ClipV2Endpoint(string subEndpoint)
    {
        if (!subEndpoint.StartsWith('/'))
            throw new ArgumentException("Endpoint must start with a slash.");
        EndpointPath = subEndpoint;
    }

    public Uri GetUri() => new(ClipPath + EndpointPath, UriKind.Relative);

    public static implicit operator ClipV2Endpoint(string subEndpoint)
        => new(subEndpoint);
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We don't want users to access these properties without setting their BaseAddress first.")]
internal class EndpointsV1 : HueEndpoints
{
    public EndpointsV1(string bridgeIp) : base(bridgeIp)
    {
    }

    public string Authentication => "/api";
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We don't want users to access these properties without setting their BaseAddress first.")]
internal sealed class EndpointsV2 : HueEndpoints
{
    public EndpointsV2(string bridgeIp) : base(bridgeIp)
    {
    }

    public ClipV2Endpoint Lights => "/resource/light";
    public ClipV2Endpoint Light(Guid id) => $"/resource/light/{id}";

    public ClipV2Endpoint EntertainmentConfigurations => "/resource/entertainment_configuration";
    public ClipV2Endpoint EntertainmentConfiguration(Guid id) => $"/resource/entertainment_configuration/{id}";

    public ClipV2Endpoint EntertainmentServices => "/resource/entertainment";
    public ClipV2Endpoint EntertainmentService(Guid id) => $"/resource/entertainment/{id}";
}