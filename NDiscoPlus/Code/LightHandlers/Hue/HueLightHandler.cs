using NDiscoPlus.PhilipsHue.Api.HueApi;
using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
using NDiscoPlus.PhilipsHue.Authentication.Models;
using NDiscoPlus.PhilipsHue.Entertainment;
using NDiscoPlus.PhilipsHue.Entertainment.Models.Channels;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace NDiscoPlus.Code.LightHandlers.Hue;

public class HueLightHandler : LightHandler<HueLightHandlerConfig>
{
    // Implemented for future-proofing
    // Currently I can't find any info on Philips Hue Entertainment latency,
    // but this can be adjusted later if I observe any latency IRL.
    private static readonly TimeSpan _kExpectedLatency = TimeSpan.Zero;
    private static readonly TimeSpan _kSignalDuration = TimeSpan.FromSeconds(1);

    private readonly record struct ValidatedConfig(
        string BridgeIp,
        HueCredentials Credentials,
        HueEntertainmentConfiguration EntertainmentConfig
    );

    private HueEntertainment? entertainment;

    public HueLightHandler(HueLightHandlerConfig? config) : base(config)
    {
    }

    public override async IAsyncEnumerable<NDPLight> GetLights()
    {
        ValidatedConfig? config = await InternalValidateConfig(null);
        if (!config.HasValue)
            yield break;

        await foreach (NDPLight light in InternalGetLights(config.Value))
            yield return light;
    }

    private static async IAsyncEnumerable<NDPLight> InternalGetLights(ValidatedConfig config)
    {
        using LocalHueApi api = new(config.BridgeIp, config.Credentials);

        HueEntertainmentConfiguration entConfig = config.EntertainmentConfig;

        foreach (HueEntertainmentChannel channel in entConfig.Channels)
        {
            HueSegmentReference? member = channel.Members.SingleOrDefault();
            HueLight? light = null;
            if (member?.Service.ResourceType.IsEntertainmentService == true)
            {
                HueEntertainmentService? service = await api.GetEntertainmentService(member.Service.ResourceId);
                if (service?.RendererReference?.ResourceType.IsLight == true)
                    light = await api.GetLightAsync(service.RendererReference.ResourceId);
            }

            yield return ConstructLight(entConfig.Id, channel, light);
        }
    }

    private static NDPLight ConstructLight(Guid entertainmentConfiguration, HueEntertainmentChannel channel, HueLight? light)
    {
        HueLightId lightId = new()
        {
            EntertainmentConfigurationId = entertainmentConfiguration,
            ChannelId = channel.ChannelId,
            LightId = light?.Id
        };
        string? displayName = light?.Metadata.Name;
        LightPosition position = new(channel.Position.X, channel.Position.Y, channel.Position.Z);
        ColorGamut? gamut = TryConstructGamut(light?.Color?.Gamut);

        return new NDPLight()
        {
            Id = lightId,
            DisplayName = displayName,
            Position = position,
            ColorGamut = gamut,
            ExpectedLatency = _kExpectedLatency
        };
    }

    private static ColorGamut? TryConstructGamut(HueGamut? gamut)
    {
        if (gamut is null)
            return null;

        return new ColorGamut(
            red: new ColorGamutPoint(gamut.Red.X, gamut.Red.Y),
            green: new ColorGamutPoint(gamut.Green.X, gamut.Green.Y),
            blue: new ColorGamutPoint(gamut.Blue.X, gamut.Blue.Y)
        );
    }

    public override async ValueTask<NDPLight[]?> Start(ErrorMessageCollector? errors)
    {
        ValidatedConfig? validConfig = await InternalValidateConfig(errors);

        if (entertainment is not null)
        {
            errors?.Add("Light handler already running.");
            validConfig = null;
        }

        if (validConfig is not ValidatedConfig config)
            return null;

        List<NDPLight> lights = new();
        await foreach (NDPLight light in InternalGetLights(config))
            lights.Add(light);

        entertainment = await HueEntertainment.ConnectAsync(config.BridgeIp, config.Credentials, config.EntertainmentConfig.Id);
        return lights.ToArray();
    }

    public override ValueTask Stop()
    {
        entertainment?.Dispose();
        return ValueTask.CompletedTask;
    }

    public override void Update(LightColorCollection lights)
    {
        if (entertainment is null)
            throw new InvalidOperationException("Hue Light Handler not started.");

        List<HueXYEntertainmentChannel> channels = new();
        foreach ((HueLightId light, NDPColor color) in lights.OfType<HueLightId>()) // get lights of correct type 
        {
            if (light.EntertainmentConfigurationId != entertainment.EntertainmentConfiguration)
                continue;
            channels.Add(
                new HueXYEntertainmentChannel(light.ChannelId)
                {
                    X = BitResolution.AsUInt16(color.X),
                    Y = BitResolution.AsUInt16(color.Y),
                    Brightness = BitResolution.AsUInt16(color.Brightness)
                }
            );
        }

        entertainment.Send(channels);
    }

    public override async ValueTask<TimeSpan?> Signal(LightId lightId, NDPColor color)
    {
        HueLightId light = (HueLightId)lightId;

        if (!await ValidateConfig(null))
            return null;
        if (!light.LightId.HasValue)
            return null;

        var request = new HueLightRequestBuilder().WithSignaling(_kSignalDuration, new HueXY(color.X, color.Y));

        using (LocalHueApi api = new(Config.BridgeIP!, Config.BridgeCredentials!.Value))
        {
            await api.UpdateLightAsync(light.LightId.Value, request);
        }

        return _kSignalDuration;
    }

    public override async ValueTask<bool> ValidateConfig(ErrorMessageCollector? errors)
    {
        ValidatedConfig? config = await InternalValidateConfig(errors);
        return config.HasValue;
    }

    private async Task<ValidatedConfig?> InternalValidateConfig(ErrorMessageCollector? errors)
    {
        bool valid = true;

        if (Config.BridgeIP is null)
        {
            errors?.Add("No bridge IP selected.");
            valid = false;
        }
        else if (!System.Net.IPAddress.TryParse(Config.BridgeIP, out _))
        {
            errors?.Add("Invalid bridge IP.");
            valid = false;
        }

        if (Config.BridgeCredentials is null)
        {
            errors?.Add("Application not linked with bridge.");
            valid = false;
        }

        HueEntertainmentConfiguration? entertainmentConfiguration = null;
        if (!Config.EntertainmentConfiguration.HasValue)
        {
            errors?.Add("No entertainment configuration selected.");
            valid = false;
        }
        else
        {
            using (LocalHueApi api = new(Config.BridgeIP!, Config.BridgeCredentials!.Value))
            {
                entertainmentConfiguration = await api.TryGetEntertainmentConfigurationAsync(Config.EntertainmentConfiguration.Value);
            }

            if (entertainmentConfiguration is null)
            {
                errors?.Add("Entertainment configuration not found.");
                valid = false;
            }
        }

        if (valid)
        {
            Debug.Assert(Config.BridgeIP is not null);
            Debug.Assert(Config.BridgeCredentials.HasValue);
            Debug.Assert(entertainmentConfiguration is not null);

            return new ValidatedConfig(
                BridgeIp: Config.BridgeIP,
                Credentials: Config.BridgeCredentials.Value,
                EntertainmentConfig: entertainmentConfiguration
            );
        }
        else
        {
            return null;
        }
    }

    protected override HueLightHandlerConfig CreateConfig()
        => new();
}
