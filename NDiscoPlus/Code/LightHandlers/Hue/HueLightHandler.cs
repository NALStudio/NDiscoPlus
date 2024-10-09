using NDiscoPlus.PhilipsHue.Api.HueApi;
using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Entertainment;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Lights;
using NDiscoPlus.PhilipsHue.Authentication.Models;
using NDiscoPlus.PhilipsHue.Entertainment;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace NDiscoPlus.Code.LightHandlers.Hue;

public class HueLightHandler : LightHandler<HueLightHandlerConfig>
{
    private readonly record struct ValidatedConfig(
        string BridgeIp,
        HueCredentials Credentials,
        Guid EntertainmentConfig
    );

    private HueEntertainment? entertainment;

    public HueLightHandler(HueLightHandlerConfig? config) : base(config)
    {
    }

    public override async IAsyncEnumerable<NDPLight> GetLights()
    {
        ValidatedConfig? config = InternalValidateConfig(null);
        if (!config.HasValue)
            yield break;

        await foreach (NDPLight light in InternalGetLights(config.Value))
            yield return light;
    }

    private static async IAsyncEnumerable<NDPLight> InternalGetLights(ValidatedConfig config)
    {
        using LocalHueApi api = new(config.BridgeIp, config.Credentials);

        HueEntertainmentConfiguration? configuration = await api.GetEntertainmentConfigurationAsync(config.EntertainmentConfig);
        if (configuration is null)
            yield break;

        foreach (HueEntertainmentChannel channel in configuration.Channels)
        {
            // We only support 1 light per channel
            // so we find the first light in the channel and return that.
            HueSegmentReference? member = channel.Members.FirstOrDefault(m => m.Service.ResourceType.IsLight);
            if (member is null)
                continue;

            HueLight? light = await api.GetLightAsync(member.Service.ResourceId);
            if (light is null)
                continue;

            Debug.Assert(configuration.Id == config.EntertainmentConfig);
            yield return ConstructLight(configuration.Id, channel, light);
        }
    }

    private static NDPLight ConstructLight(Guid entertainmentConfiguration, HueEntertainmentChannel channel, HueLight light)
    {
        HueLightId lightId = new()
        {
            EntertainmentConfigurationId = entertainmentConfiguration,
            ChannelId = channel.ChannelId,
            LightId = light.Id
        };
        string displayName = light.Metadata.Name;
        LightPosition position = new(channel.Position.X, channel.Position.Y, channel.Position.Z);
        ColorGamut? gamut = TryConstructGamut(light.Color?.Gamut);

        return new NDPLight(lightId, displayName, position, gamut);
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
        ValidatedConfig? validConfig = InternalValidateConfig(errors);

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

        entertainment = await HueEntertainment.ConnectAsync(config.BridgeIp, config.Credentials, config.EntertainmentConfig);
        return lights.ToArray();
    }

    public override ValueTask Stop()
    {
        entertainment?.Dispose();
        return ValueTask.CompletedTask;
    }

    public override ValueTask Update(LightColorCollection lights)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask<TimeSpan?> Signal(LightId lightId, NDPColor color)
    {
        HueLightId light = (HueLightId)lightId;

        if (!await ValidateConfig(null))
            return null;

        const int durationMs = 1000;

        var request = new HueLightRequestBuilder().WithSignaling(durationMs, new HueXY(color.X, color.Y));

        using (LocalHueApi api = new(Config.BridgeIP!, Config.BridgeCredentials!.Value))
        {
            await api.UpdateLightAsync(light.LightId, request);
        }

        return TimeSpan.FromMilliseconds(durationMs);
    }

    public override ValueTask<bool> ValidateConfig(ErrorMessageCollector? errors)
    {
        ValidatedConfig? config = InternalValidateConfig(errors);
        return new(config is not null);
    }

    private ValidatedConfig? InternalValidateConfig(ErrorMessageCollector? errors)
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

        if (valid)
        {
            Debug.Assert(Config.BridgeIP is not null);
            Debug.Assert(Config.BridgeCredentials.HasValue);
            Debug.Assert(Config.EntertainmentConfiguration.HasValue);

            return new ValidatedConfig(
                BridgeIp: Config.BridgeIP,
                Credentials: Config.BridgeCredentials.Value,
                EntertainmentConfig: Config.EntertainmentConfiguration.Value
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
