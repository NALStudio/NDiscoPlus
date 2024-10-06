using NDiscoPlus.Components.Elements.LightHandlerConfigEditor.HueLightHandlerConfigEditor;
using NDiscoPlus.PhilipsHue.Authentication.Models;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;

namespace NDiscoPlus.Code.LightHandlers.Hue;

public class HueLightHandlerConfig : LightHandlerConfig
{
    public string? BridgeIP { get; set; } = null;
    public HueCredentials? BridgeCredentials { get; set; } = null;
    public Guid? EntertainmentConfiguration { get; set; } = null;

    public override LightHandler CreateLightHandler()
        => new HueLightHandler(this);

    public override Type GetEditorType()
        => typeof(HueLightHandlerConfigEditor);
}

public class HueLightHandler : LightHandler<HueLightHandlerConfig>
{
    public HueLightHandler(HueLightHandlerConfig? config) : base(config)
    {
    }

    public override IAsyncEnumerable<NDPLight> GetLights()
    {
        throw new NotImplementedException();
    }

    public override ValueTask<bool> Start(ErrorMessageCollector? errors, out NDPLight[] lights)
    {
        throw new NotImplementedException();
    }

    public override ValueTask Stop()
    {
        throw new NotImplementedException();
    }

    public override ValueTask Update(LightColorCollection lights)
    {
        throw new NotImplementedException();
    }

    public override ValueTask Signal(LightId lightId, NDPColor color)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<bool> ValidateConfig(ErrorMessageCollector? errors)
    {
        bool valid = true;

        if (Config.BridgeIP is null)
        {
            errors?.Add("No bridge IP selected.");
            valid = false;
        }

        if (Config?.BridgeCredentials is null)
        {
            errors?.Add("Application not linked with bridge.");
            valid = false;
        }

        return new(valid);
    }

    protected override HueLightHandlerConfig CreateConfig()
        => new();
}
