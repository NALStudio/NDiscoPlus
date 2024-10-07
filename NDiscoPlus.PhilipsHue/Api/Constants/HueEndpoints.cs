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
}

internal class EndpointsV1 : HueEndpoints
{
    public EndpointsV1(string bridgeIp) : base(bridgeIp)
    {
    }

    public string Authentication => $"https://{bridge}/api";
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We don't want users to access these properties without setting their BaseAddress first.")]
internal sealed class EndpointsV2 : HueEndpoints
{
    public EndpointsV2(string bridgeIp) : base(bridgeIp)
    {
    }

    public string BaseAddress => $"https://{bridge}/clip/v2";

    public string Lights => "/resource/light";
    public string Light(Guid id) => $"/resource/light/{id}";

    public string EntertainmentConfigurations => "/resource/entertainment_configuration";
    public string EntertainmentConfiguration(Guid id) => $"/resource/entertainment_configuration/{id}";
}